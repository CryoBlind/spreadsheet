Carter Dean
Abdulahad Asim

10/21/2023

XAML Overview-
Three seperate labels and entries were added in MainPage.xaml. The labels display "Name", "Value", and "Content". The
entries along side each one display the actual value. So if the cell "A1" was clicked "Name" would display A1, and "Value"
and "Content" would display whatever the backing spreadsheet is storing for A1. Four Buttons were also added: "Input Content",
"Sum", "Average", "Median". The Input Content button is used after enteing a value into the Content entry to finalize it
in the backing spreadsheet. The other buttons are all math functions that can be used after clikcing them.

C# Overview
The method OnContentsChanged is fired when a content of a cell is changed and is used to get the cell selection based on 
user input and updates all the Cells accordingly. The New Clicked method and the Save Method were also implemented and 
they fire when they are clicked on in the spreadsheet application. Both use methods from the backing spreadsheet to clear 
and save. Lastly a Help menu was added as a display an alert message which lets the user the feautures of the spreadsheet 
and how it works. Methods to implement the math functions in the XAML overview were also added. They do all the calculations
and then place the output in the Content entry which can then be inputted.

