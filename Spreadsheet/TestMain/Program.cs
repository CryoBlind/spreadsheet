// See https://aka.ms/new-console-template for more information
using FormulaEvaluator;
using SpreadsheetUtilities;
using System.Collections;
using System.Text.RegularExpressions;

var a = new ArrayList();
a.Add(5.5d);
a.Add("hello");
a.Add(Double.Parse("500.5"));
a.Add(Double.Parse("500.500"));
a.Add(Double.Parse("5.005e2"));
a.Add(Double.Parse("500.500000000"));
foreach (var i in a)
{
    Console.WriteLine(i.GetType() + " - " + i.ToString());
}
Console.WriteLine("");
Console.WriteLine(a[1].ToString());