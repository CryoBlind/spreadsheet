using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetUtilities
{
    public class Cell
    {
        /// <summary>
        /// name of the cell
        /// </summary>
        private string p_name;
        public string Name
        {
            get { return p_name; }
            private set { p_name = value; }
        }

        /// <summary>
        /// raw contents, double, formula, or string
        /// </summary>
        private object p_contents;
        public object Contents
        {
            get { return p_contents; }
            set { p_contents = value; UpdateValue(); }
        }

        /// <summary>
        /// value
        /// </summary>
        private object p_value;
        public object Value
        {
            get { return p_value; }
            private set { p_value = value; }
        }

        private readonly Func<string, double>? lookup;

        /// <summary>
        /// creates a cell with a number in it
        /// </summary>
        /// <param name="name">the name of the cell</param>
        public Cell(string name, double number)
        {
            p_value = "";
            p_contents = number;
            p_name = name;
            Name = name;
            Contents = number;
        }
        /// <summary>
        /// creates a cell with a string in it
        /// </summary>
        /// <param name="name">the name of the cell</param>
        public Cell(string name, string text)
        {
            p_value = "";
            p_contents = text;
            p_name = name;
            Name = name;
            Contents = text;
        }
        /// <summary>
        /// creates a cell with a formula and lookup function
        /// </summary>
        /// <param name="name">the name of the cell</param>
        /// <param name="lookup"> lookup function used for formulas</param>
        public Cell(string name, Formula formula, Func<string, double> lookup)
        {
            p_value = "";
            p_contents = formula;
            p_name = name;
            this.lookup = lookup;
            Name = name;
            Contents = formula;
        }

        /// <summary>
        /// updates the value using what is in the contents.  Done automatically when setting the contents to something new.
        /// </summary>
        public void UpdateValue()
        {
            //if string, set value as string
            if (Contents.GetType() == typeof(string)) {
                Value = (string)Contents;
                return;
            }

            //if formula, evalute
            if (Contents.GetType() == typeof(Formula)) 
            {
                var s = ((Formula)Contents).Evaluate(lookup!); 
                Value = s;
                return;
            }
            //if double, set value to contents
            if(Contents.GetType() == typeof(double))
            {
                Value = (double)Contents;
                return;
            }
        }
    }
}
