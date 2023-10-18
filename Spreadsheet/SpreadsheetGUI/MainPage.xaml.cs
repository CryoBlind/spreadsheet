using SpreadsheetUtilities;
using SS;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    Spreadsheet s;

    private bool Validate(string s) {
        return Regex.IsMatch(s, @"^[A-Z][0-9][0-9]$");        
    }

    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();

        s = new Spreadsheet();

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(2, 3);
    }

    private void displaySelection(ISpreadsheetGrid grid)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.GetValue(col, row, out string value);
        string CellName = "" + (char)(col + (int)('A')) + (row + 1);


        //set name
        inputName.Text = CellName;

        //set contents and value
        var CellContents = s.GetCellContents(CellName);
        if (CellContents.GetType() == typeof(string))
        {
            inputContent.Text = (string)CellContents;
            inputValue.Text = (string)CellContents; 
        }
        else if(CellContents.GetType() == typeof(double))
        {
            inputContent.Text = ((double)CellContents).ToString();
            inputValue.Text = ((double)CellContents).ToString();
        }
        else
        {
            inputContent.Text = "=" + ((Formula)CellContents).ToString();
            inputValue.Text ="" +  s.GetCellValue(CellName);
            //TODO could be formula error.  Handle this
        }

        



        //if (value == "")
        //{
        //    spreadsheetGrid.SetValue(col, row, DateTime.Now.ToLocalTime().ToString("T"));
        //    spreadsheetGrid.GetValue(col, row, out value);
        //    DisplayAlert("Selection:", "column " + col + " row " + row + " value " + value, "OK");
        //}
    }

    /// <summary>
    /// Fires when contents of cell are changed
    /// </summary>
    private void OnContentsChanged(object sender, EventArgs e)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);
        System.Diagnostics.Debug.WriteLine("col: " + col + " row: " + row + " text: " + inputContent.Text);

        try
        {
            var name = (char)('A' + col) + "" + (row + 1);
            var toUpdate = s.SetContentsOfCell(name, inputContent.Text);

            spreadsheetGrid.SetValue(col, row, "" + s.GetCellValue(name));
            foreach (string cell in toUpdate)
            {
                spreadsheetGrid.SetValue((int)(name[0]) - 'A', int.Parse(name.Substring(1)) - 1, "" + s.GetCellValue(name));
            }
        }
        //CARE ABOUT LATERRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR
        catch(Exception) 
        { 
        
        }
        
        //TODOOOOOOOOOOOOOOOOOOOOOOO
    }


    private void NewClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.Clear();
    }

    private void PopulateCells(string fileName)
    {
        var tempSheet = new Spreadsheet(fileName, Validate, (s) => s.ToUpper(), "1.0");
        spreadsheetGrid.Clear();
        foreach (var e in s.GetNamesOfAllNonemptyCells())
        {
            //update each nonempty cell
            spreadsheetGrid.SetValue((int)(e[0]) - 'A', int.Parse(e.Substring(1)), tempSheet.GetCellValue(e).ToString());

        }
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
        System.Diagnostics.Debug.WriteLine( "Successfully chose file: " + fileResult.FileName );
        // for windows, replace Console.WriteLine statements with:
        //System.Diagnostics.Debug.WriteLine( ... );

        string fileContents = File.ReadAllText(fileResult.FullPath);
                System.Diagnostics.Debug.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
                //populate the cells
                PopulateCells(fileResult.FullPath);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error opening file:");
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
