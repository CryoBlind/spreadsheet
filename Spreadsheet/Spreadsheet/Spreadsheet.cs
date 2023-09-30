using SpreadsheetUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        //a hashtable that pairs a name with a cell object
        private Hashtable p_cells;
        public Hashtable Cells
        {
            get { return p_cells; }
        }
        //the dependency graph for the spreadsheet
        private readonly DependencyGraph dependencyGraph;

        //delegates for IsValid and Normalize
        private Func<string, bool> IsValid;
        private Func<string, string> Normalize;

        /// <summary>
        /// creates a Spreadsheet object
        /// </summary>
        public Spreadsheet() : base("default")
        {
            this.p_cells = new Hashtable();
            this.dependencyGraph = new DependencyGraph();
            Normalize = (s) => s;
            IsValid = (s) => true;
            Changed = false;
        }

        public Spreadsheet(Func<string, string> Normalize, Func<string, bool> IsValid, string version) : base(version)
        {
            this.p_cells = new Hashtable();
            this.dependencyGraph = new DependencyGraph();
            this.Normalize= Normalize;
            this.IsValid = IsValid;
            Changed = false;
        }

        public Spreadsheet(string filepath, Func<string, string> Normalize, Func<string, bool> IsValid, string version) : base(version)
        {
            this.p_cells = new Hashtable();
            this.dependencyGraph = new DependencyGraph();
            this.Normalize = Normalize;
            this.IsValid = IsValid;
            Changed = false;

            //Read from file
            string? json;
            try
            {
                json = File.ReadAllText(filepath);
            }
            catch (Exception e) { throw new SpreadsheetReadWriteException("File Read Failed: " + e.Message); }

            SpreadsheetData? temp;
            try
            {
                temp = JsonSerializer.Deserialize<SpreadsheetData>(json);
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }

            if (temp!.Version == null || temp.Cells == null) throw new SpreadsheetReadWriteException("Null values in Json File");
            if (!temp.Version.Equals(Version)) throw new SpreadsheetReadWriteException("Version of file does not match spreadsheet version");
            try
            {
                foreach (string key in temp.Cells!.Keys)
                {
                    SetContentsOfCell(key, ((JsonElement)temp.Cells[key]!).GetProperty("StringForm").ToString());
                }
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException(e.Message);
            }

        }

        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            if(!IsValidName(name)) throw new InvalidNameException();

            if (p_cells[name] != null) return ((Cell)p_cells[name]!).Contents;
            else return "";
        }

        public override object GetCellValue(string name)
        {
            name = Normalize(name);
            if (!IsValidName(name)) throw new InvalidNameException();

            if (p_cells[name] != null) return ((Cell)p_cells[name]!).Value;
            else return "";
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            var nonemptyCells = new List<string>();
            foreach(var cell in p_cells.Values)
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
            name = Normalize(name);
            if (!IsValidName(name)) throw new InvalidNameException();

            double parseResult;
            if (Double.TryParse(content, out parseResult))
            {
                return SetCellContents(name, parseResult);
            }
            else if (content.StartsWith("="))
            {
                return SetCellContents(name, new Formula(content.Substring(1), Normalize, IsValid));
            }
            else return SetCellContents(name, content);
        }

        protected override IList<string> SetCellContents(string name, double number)
        {
            var previous = p_cells[name];

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

            p_cells[name] = new Cell(name, number);

            var allDependents = GetCellsToRecalculate(name);
            List<string> toReturn;
            if (!allDependents.Contains<string>(name)) toReturn = allDependents.Prepend(name).ToList();
            else toReturn = allDependents.ToList();

            RecalculateCells(allDependents);
            Changed = true;
            return toReturn;
        }

        protected override IList<string> SetCellContents(string name, string text)
        {
            var previous = p_cells[name];

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

            p_cells[name] = new Cell(name, text);

            var allDependents = GetCellsToRecalculate(name);
            List<string> toReturn;
            if (!allDependents.Contains<string>(name)) toReturn = allDependents.Prepend(name).ToList();
            else toReturn = allDependents.ToList();

            RecalculateCells(allDependents);
            Changed = true;
            return toReturn;
        }

        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            var previous = p_cells[name];

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

            p_cells[name] = new Cell(name, formula, LookupVariable);

            //add dependency relationships
            foreach (var s in formula.GetVariables())
            {
                dependencyGraph.AddDependency(s, name);
            }
            
            IEnumerable<string> allDependents;
            List<string> toReturn = new List<string>();

            try
            {
                allDependents = GetCellsToRecalculate(name);
                if (!allDependents.Contains<string>(name)) toReturn = allDependents.Prepend(name).ToList();
                else toReturn = allDependents.ToList();
            }
            catch (CircularException)
            {
                p_cells[name] = previous;

                //remove dependency relationships
                foreach (var s in formula.GetVariables())
                {
                    dependencyGraph.RemoveDependency(s, name);
                }

                throw new CircularException();
            }

            RecalculateCells(allDependents);
            Changed = true;
            return toReturn;
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            name = Normalize(name);
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
                if (p_cells[s] != null) ((Cell)p_cells[s]!).UpdateValue();
            }
        }

        /// <summary>
        /// checks if a given string is a valid name
        /// </summary>
        /// <returns>true if the string is valid, false otherwise</returns>
        private bool IsValidName(string name)
        {
            name = Normalize(name);
            if (Regex.IsMatch(name, @"^[_a-zA-Z][_a-zA-Z0-9]*$") && IsValid(name)) return true;
            else return false;
        }

        /// <summary>
        /// looks up variable names
        /// </summary>
        /// <param name="name">the variable name</param>
        /// <returns>a double value of the variable</returns>
        private double LookupVariable(string name)
        {
            name = Normalize(name);
            var v = p_cells[name];
            if (v == null) throw new ArgumentException("variable not found");

            if (((Cell)v).Value.GetType() == typeof(double))
            {
                return (double)((Cell)v).Value;
            }
            else throw new ArgumentException("variable has invalid value");
        }

        public override void Save(string filename)
        {
            //serialize
            Changed = false;
            JsonSerializerOptions jso = new();
            jso.WriteIndented = true;


            // Write to file.
            try
            {
                var s = JsonSerializer.Serialize(this, jso);
                using (StreamWriter outputFile = new StreamWriter(filename)) { outputFile.Write(s); }
            }
            catch (Exception e)
            {
                throw new SpreadsheetReadWriteException("Error writing Json: " + e.Message);
            }
        }

        

        
    }
}


