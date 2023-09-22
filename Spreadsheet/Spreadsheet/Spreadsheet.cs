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
        public Spreadsheet()
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

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            var nonemptyCells = new List<string>();
            foreach(var cell in cells.Values)
            {
                if (((Cell)cell).Contents.GetType() == typeof(string))
                    if (!((Cell)cell).Value.Equals(""))
                        nonemptyCells.Add(((Cell)cell).Name);
            }

            return nonemptyCells;
        }

        public override IList<string> SetCellContents(string name, double number)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            cells[name] = new Cell(name, number);

            var allDependents = GetCellsToRecalculate(name);
            var toReturn = allDependents.Prepend(name).ToList();

            RecalculateCells(allDependents);
            return toReturn;
        }

        public override IList<string> SetCellContents(string name, string text)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            cells[name] = new Cell(name, text);

            var allDependents = GetCellsToRecalculate(name);
            var toReturn = allDependents.Prepend(name).ToList();

            RecalculateCells(allDependents);
            return toReturn;
        }

        public override IList<string> SetCellContents(string name, Formula formula)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            cells[name] = new Cell(name, formula, LookupVariable);

            //add dependency relationships
            foreach (var s in formula.GetVariables())
            {
                dependencyGraph.AddDependency(s, name);
            }

            var allDependents = GetCellsToRecalculate(name);
            var toReturn = allDependents.Prepend(name).ToList();

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
        /// works the same as getCellContents, but returns the value instead
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidNameException"></exception>
        public string GetCellValue(string name)
        {
            if (!IsValidName(name)) throw new InvalidNameException();

            if (cells[name] != null) return ((Cell)cells[name]!).Value;
            else return "";
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

            double parseResult;
            if (Double.TryParse(((Cell)v).Value, out parseResult))
            {
                return parseResult;
            }
            else throw new ArgumentException("variable not found");
        }
    }
}

