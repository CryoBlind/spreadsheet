// Carter Dean
// Abdulahad Asim

// 10/21/2023

using Microsoft.UI.Xaml.Automation;
using SpreadsheetUtilities;
using SS;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Windows.Media.Devices;
using Windows.Security.Cryptography.Certificates;
using WinRT;
using System.Windows.Input;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly string VERSION = "ps6";
    private Spreadsheet s;
    private string filePath;
    private string lastPath;


    /// <summary>
    /// validate function that prevents the use of underscores or numbers larger than 99
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private bool Validate(string s)
    {
        return Regex.IsMatch(s, @"^[A-Z][0-9]$|^[A-Z][0-9][0-9]$");
    }

    /// <summary>
    /// Normalize function that returns the uppercase variable
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private string Normalize(string s)
    {
        return s.ToUpper();
    }

    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();

        s = new Spreadsheet(Validate, Normalize, VERSION);
        filePath = "example.sprd";
        lastPath = "example.sprd";

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(0, 0);

        inputName.Text = "A1";
    }

    /// <summary>
    /// Display the selection box around a cell and update boxes on top of screen accordingly.
    /// </summary>
    /// <param name="grid"></param>
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
        else if (CellContents.GetType() == typeof(double))
        {
            inputContent.Text = ((double)CellContents).ToString();
            inputValue.Text = ((double)CellContents).ToString();
        }
        else
        {
            if (s.GetCellValue(CellName).GetType() == typeof(FormulaError))
            {
                inputContent.Text = "=" + ((Formula)CellContents).ToString();
                DisplayAlert("Formula Error", ((FormulaError)s.GetCellValue(CellName)).Reason, "OK");
            }
            else
            {
                inputContent.Text = "=" + ((Formula)CellContents).ToString();
                inputValue.Text = "" + s.GetCellValue(CellName);
            }
        }
    }



    /// <summary>
    /// Fires when contents of cell should be changed
    /// </summary>
    private void OnContentsChanged(object sender, EventArgs e)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);

        var name = (char)('A' + col) + "" + (row + 1);
        try
        {
            IList<string> toUpdate;
            if (inputContent.Text.StartsWith("="))
                toUpdate = s.SetContentsOfCell(name, Normalize(inputContent.Text));
            else
                toUpdate = s.SetContentsOfCell(name, inputContent.Text);

            foreach (string cell in toUpdate)
            {
                var cellValue = s.GetCellValue(cell);
                //If the returned cell type is a formla error, the value of that cell in the backing array is set to the string "Formula Error"
                if (cellValue.GetType() == typeof(FormulaError))
                    spreadsheetGrid.SetValue((int)(cell[0]) - 'A', int.Parse(cell.Substring(1)) - 1, "" + "Formula Error");
                //else the value of the cell is set
                else
                    spreadsheetGrid.SetValue((int)(cell[0]) - 'A', int.Parse(cell.Substring(1)) - 1, "" + cellValue);
            }
        }
        catch (Exception exception)
        {
            Type type = exception.GetType();


            if (type == typeof(CircularException))
            {
                //display warning about
                DisplayAlert("Circular Exception Occured", "Can not change " + name + " content", "OK");
            }
            else //(type == typeof(FormulaFormatException))
            {
                //display warning about
                DisplayAlert("Exception Occured", type.ToString() + ": " + exception.Message, "OK");
            }
        }

        spreadsheetGrid.SetSelection(col, row);
    }

    /// <summary>
    /// Clears the spreadsheet and creates a new spreadsheet.  warns if unsaved changes have been made
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NewClicked(Object sender, EventArgs e)
    {
        //checks if spreadsheet has changed, if so prints a warning message
        bool response;
        if (s.Changed)
            response = await DisplayAlert("Unsaved Changes", "Continue?", "Yes", "No");
        else
            response = true;

        if (response)
        {
            spreadsheetGrid.Clear();
            s = new Spreadsheet(Validate, Normalize, VERSION);

            //update top boxes
            spreadsheetGrid.GetSelection(out int col, out int row);
            spreadsheetGrid.SetSelection(col, row);
        }
    }

    /// <summary>
    /// Displays save dialog box prompting the user to input a save location
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveClicked(Object sender, EventArgs e)
    {
        filePath = await DisplayPromptAsync("Where do you want to save?", "Enter file path", "Ok", "Cancel", filePath);

        if (!filePath.EndsWith(".sprd"))
        {
            filePath = filePath + ".sprd";
        }


        //check if file already exists
        bool fileExists;
        try
        {
            FileStream f = File.OpenRead(filePath);
            fileExists = true;
            f.Close();
        }
        catch (FileNotFoundException)
        {
            fileExists = false;
        }

        bool result;
        if (lastPath != filePath && fileExists)
        {
            result = await DisplayAlert("File Already Exists", "Do you want to overwrite it?", "Yes", "No");
        }
        else result = true;

        //save file
        if (result)
        {
            try
            {
                s.Save(filePath);
                lastPath = filePath;
            }
            catch (SpreadsheetReadWriteException ex)
            {
                await DisplayAlert("Save Failed", ex.Message, "OK");
            }
        }
    }

    //Function Buttons

    /// <summary>
    /// Creates a formula that adds all the cells in the range together
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SumClicked(Object sender, EventArgs e)
    {
        string entry = await DisplayPromptAsync("Enter Range to Sum:", "Format as A1:C3");

        if (entry != null)
        {
            string formula = "=";
            var variables = Regex.Split(entry, ":");

            bool firstLoop = true;

            //Goes through the range and adds the cell to the formula 
            for (int col = (int)variables[0][0]; col <= (int)variables[1][0]; col++)
            {
                for (int row = int.Parse(variables[0].Substring(1)); row <= int.Parse(variables[1].Substring(1)); row++)
                {
                    if (firstLoop)
                    {
                        formula += "" + (char)col + row; firstLoop = false;
                    }
                    else
                        formula += "+" + (char)col + row;
                }
            }

            //formula is outputted in the Content entry
            inputContent.Text = formula;
        }
    }

    /// <summary>
    /// Creates a formula that will average all the cells in range and inputs it into the content box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AverageClicked(Object sender, EventArgs e)
    {
        string entry = await DisplayPromptAsync("Enter Range to Average:", "Format as A1:C3");

        if (entry != null)
        {
            string formula = "=";
            var variables = Regex.Split(entry, ":");

            bool firstLoop = true;
            int count = 0;

            //Goes through the range and adds the cell to the formula 
            for (int col = (int)variables[0][0]; col <= (int)variables[1][0]; col++)
            {
                for (int row = int.Parse(variables[0].Substring(1)); row <= int.Parse(variables[1].Substring(1)); row++)
                {
                    if (firstLoop)
                    {
                        formula += "(" + (char)col + row; firstLoop = false;
                    }
                    else
                        formula += "+" + (char)col + row;
                    count++;
                }
            }

            formula += ")/" + count;

            //formula is outputted in the Content entry
            inputContent.Text = formula;
        }
    }

    /// <summary>
    /// When the median button is clicked, this method fires. It sorts the values of all decimal cells
    /// based on the range provided by the users and outputs the median on the Content entry in the spreadsheet
    /// application.
    /// </summary>

    private async void MedianClicked(Object sender, EventArgs e)
    {
        string entry = await DisplayPromptAsync("Enter Range to Find Median:", "Format as A1:C3");

        if (entry != null)
        {
            List<double> values = new List<double>();

            var variables = Regex.Split(entry, ":");
            double median;

            double val;

            for (int col = (int)variables[0][0]; col <= (int)variables[1][0]; col++)
            {
                for (int row = int.Parse(variables[0].Substring(1)); row <= int.Parse(variables[1].Substring(1)); row++)
                {
                    var valCheck = s.GetCellValue((char)col + "" + row);
                    if (valCheck.GetType() == typeof(double))
                    {
                        val = (double)s.GetCellValue((char)col + "" + row);
                        values.Add(val);
                    }
                }
            }

            var arr = values.ToArray();
            Array.Sort(arr);

            if (arr.Count() != 0)
            {
                median = arr[arr.Length / 2];
                inputContent.Text = "" + median;
            }
        }

    }

    /// <summary>
    /// Gets fired when the Help menu is clicked in the spreadsheet. Displays informatin in a DisplayAlert window
    /// to help the user navigate the spreadsheet.
    /// </summary>
    private void HelpClicked(Object sender, EventArgs e)
    {
        string message = "Input Contents:" +
                         "  You can set the contents of a cell by clicking the grey box next to Content. \n \n" +

                         "Content Validity:" +
                         "  To make the content of a cell and equation you will have to add an equal sign at " +
                         "  the beginning of the equation (e.g =A1+2). \n \n" +
                         "Math Functions: " +
                         "  The Sum, Average, and Median math functions are provided to you on the top right hand side of the screen \n \n" +
                         "Menus:" +
                         "  You can find the menus on the top right hand side of the spreadsheet. You can use the file menu to save, load, or make a new spreadsheet."

                            ;
        DisplayAlert("Help Menu", message, "OK");
    }

    /// <summary>
    /// Takes a file name as a pramater and makes a new spreadsheet using that JSON file. Using the new spreadsheet
    /// is sets the values of each nonempty cell ion the spreadsheet.
    /// </summary>
    /// <param name="fileName"> name of file </param>
    private void PopulateCells(string fileName)
    {
        var tempSheet = new Spreadsheet(fileName, Validate, Normalize, VERSION);
        spreadsheetGrid.Clear();
        foreach (var e in tempSheet.GetNamesOfAllNonemptyCells())
        {
            //update each nonempty cell
            spreadsheetGrid.SetValue((int)(e[0]) - 'A', int.Parse(e.Substring(1)) - 1, "" + tempSheet.GetCellValue(e));


        }
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        bool response;
        if (s.Changed)
        {
            response = await DisplayAlert("Unsaved Changes", "Continue?", "Yes", "No");
        }
        else response = true;

        if (response)
        {
            try
            {
                FileResult fileResult = await FilePicker.Default.PickAsync();
                if (fileResult != null)
                {
                    System.Diagnostics.Debug.WriteLine("Successfully chose file: " + fileResult.FileName);
                    string fileContents = File.ReadAllText(fileResult.FullPath);
                    Debug.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
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

}
