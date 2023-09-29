using SpreadsheetUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        //a hashtable that pairs a name with a cell object
        private readonly Hashtable cells;
        //the dependency graph for the spreadsheet
        private readonly DependencyGraph dependencyGraph;

        /// <summary>
        /// creates a Spreadsheet object
        /// </summary>
        public Spreadsheet() : base("")
        {
            this.cells = new Hashtable();
            this.dependencyGraph = new DependencyGraph();
        }

        public Spreadsheet(string version) : base(version)
        {
            this.cells = new Hashtable();
            this.dependencyGraph = new DependencyGraph();
        }

        public override object GetCellContents(string name)
        {
            if(!IsValidName(name)) throw new InvalidNameException();

            if (cells[name] != null) return ((Cell)cells[name]!).Contents;
            else return "";
        }

        public override object GetCellValue(string name)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            if (cells[name] != null) return ((Cell)cells[name]!).Value;
            else return "";
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            var nonemptyCells = new List<string>();
            foreach(var cell in cells.Values)
            {
                if (cell != null)
                {
                    if (((Cell)cell).Contents.GetType() == typeof(string))
                    {
                        if (!((Cell)cell).Contents.Equals(""))
                            nonemptyCells.Add(((Cell)cell).Name);
                    }
                    else
                    {
                        nonemptyCells.Add(((Cell)cell).Name);
                    }
                }
            }

            return nonemptyCells;
        }

        public override IList<string> SetContentsOfCell(string name, string content)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            double parseResult;
            if (Double.TryParse(content, out parseResult))
            {
                return SetCellContents(name, parseResult);
            }
            else if (content.StartsWith("="))
            {
                return SetCellContents(name, new Formula(content.Substring(1)));
            }
            else return SetCellContents(name, content);
        }

        protected override IList<string> SetCellContents(string name, double number)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            var previous = cells[name];

            //check if a dependency needs to be removed
            if (previous != null)
            {
                var p = ((Cell)previous).Contents;
                if (p.GetType() == typeof(Formula))
                {
                    //remove dependency relationships
                    foreach (var s in ((Formula)p).GetVariables())
                    {
                        dependencyGraph.RemoveDependency(s, name);
                    }
                }
            }

            cells[name] = new Cell(name, number);

            var allDependents = GetCellsToRecalculate(name);
            List<string> toReturn;
            if (!allDependents.Contains<string>(name)) toReturn = allDependents.Prepend(name).ToList();
            else toReturn = allDependents.ToList();

            RecalculateCells(allDependents);
            return toReturn;
        }

        protected override IList<string> SetCellContents(string name, string text)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            var previous = cells[name];

            //check if a dependency needs to be removed
            if (previous != null)
            {
                var p = ((Cell)previous).Contents;
                if (p.GetType() == typeof(Formula))
                {
                    //remove dependency relationships
                    foreach (var s in ((Formula)p).GetVariables())
                    {
                        dependencyGraph.RemoveDependency(s, name);
                    }
                }
            }

            cells[name] = new Cell(name, text);

            var allDependents = GetCellsToRecalculate(name);
            List<string> toReturn;
            if (!allDependents.Contains<string>(name)) toReturn = allDependents.Prepend(name).ToList();
            else toReturn = allDependents.ToList();

            RecalculateCells(allDependents);
            return toReturn;
        }

        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            var previous = cells[name];

            //check if a dependency needs to be removed
            if(previous != null)
            {
                var p = ((Cell)previous).Contents;
                if (p.GetType() == typeof(Formula))
                {
                    //remove dependency relationships
                    foreach (var s in ((Formula)p).GetVariables())
                    {
                        dependencyGraph.RemoveDependency(s, name);
                    }
                }
            }

            cells[name] = new Cell(name, formula, LookupVariable);

            //add dependency relationships
            foreach (var s in formula.GetVariables())
            {
                dependencyGraph.AddDependency(s, name);
            }
            
            IEnumerable<string> allDependents = new List<string>();
            List<string> toReturn = new List<string>();

            try
            {
                allDependents = GetCellsToRecalculate(name);
                if (!allDependents.Contains<string>(name)) toReturn = allDependents.Prepend(name).ToList();
                else toReturn = allDependents.ToList();
            }
            catch (CircularException)
            {
                cells[name] = previous;

                //remove dependency relationships
                foreach (var s in formula.GetVariables())
                {
                    dependencyGraph.RemoveDependency(s, name);
                }

                throw new CircularException();
            }

            RecalculateCells(allDependents);
            return toReturn;
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            var toReturn = dependencyGraph.GetDependents(name);

            return toReturn;
        }

        /// <summary>
        /// recalculates all cells
        /// </summary>
        /// <param name="toRecalculate">an enumable of cells to recalculate</param>
        private void RecalculateCells(IEnumerable<string> toRecalculate)
        {
            foreach(var s in toRecalculate)
            {
                if (cells[s] != null) ((Cell)cells[s]!).UpdateValue();
            }
        }

        /// <summary>
        /// checks if a given string is a valid name
        /// </summary>
        /// <returns>true if the string is valid, false otherwise</returns>
        private bool IsValidName(string s)
        {
            if (Regex.IsMatch(s, @"^[_a-zA-Z][_a-zA-Z0-9]*$")) return true;
            else return false;
        }

        /// <summary>
        /// looks up variable names
        /// </summary>
        /// <param name="name">the variable name</param>
        /// <returns>a double value of the variable</returns>
        private double LookupVariable(string name)
        {
            var v = cells[name];
            if (v == null) throw new ArgumentException("variable not found");

            if (((Cell)v).Value.GetType() == typeof(double))
            {
                return (double)((Cell)v).Value;
            }
            else throw new ArgumentException("variable not found");
        }








        public override void Save(string filename)
        {
            throw new NotImplementedException();
        }

        

        
    }
}

