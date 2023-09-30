using SpreadsheetUtilities;
using System.Collections;
using System.Text.RegularExpressions;
using SS;

var s = new Spreadsheet();
s.SetContentsOfCell("A1", "2");
s.SetContentsOfCell("A2", "=A3+1");
s.Save("test.json");
s = new Spreadsheet("test.json", (s) => s, (s) => true, "default");
Console.WriteLine(File.ReadAllText("test.json"));