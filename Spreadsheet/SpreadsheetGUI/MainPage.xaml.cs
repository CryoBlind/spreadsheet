using SpreadsheetUtilities;
using SS;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    Spreadsheet s;


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
        //set value
        inputValue.Text= value;

        //set contents
        var CellContents = s.GetCellContents(CellName);
        if (CellContents.GetType() == typeof(string))
        {
            inputContent.Text = (string)CellContents;
        }
        else if(CellContents.GetType() == typeof(double))
        {
            inputContent.Text = ((double)CellContents).ToString();
        }
        else
        {
            inputContent.Text = ((Formula)CellContents).ToString();
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
        //s.SetContentsOfCell(inputName.Text, inputContent.Text);
        //TODOOOOOOOOOOOOOOOOOOOOOOO
    }


    private void NewClicked(Object sender, EventArgs e)
    {
        spreadsheetGrid.Clear();
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
        Console.WriteLine( "Successfully chose file: " + fileResult.FileName );
        // for windows, replace Console.WriteLine statements with:
        //System.Diagnostics.Debug.WriteLine( ... );

        string fileContents = File.ReadAllText(fileResult.FullPath);
                Console.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
    }
}
