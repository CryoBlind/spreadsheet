// These tests are for private use only
// Redistributing this file is strictly against SoC policy.

using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Diagnostics;

namespace PS4GradingTests;



/// <summary>
///This is a test class for SpreadsheetTest and is intended
///to contain all SpreadsheetTest Unit Tests
///</summary>
[TestClass()]
public class SpreadsheetTest
{

    // EMPTY SPREADSHEETS
    [TestMethod(), Timeout(2000)]
    [TestCategory("2")]
    [ExpectedException(typeof(InvalidNameException))]
    public void TestEmptyGetContents()
    {
        Spreadsheet s = new Spreadsheet();
        s.GetCellContents("1AA");
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("3")]
    public void TestGetEmptyContents()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.AreEqual("", s.GetCellContents("A2"));
    }

    // SETTING CELL TO A DOUBLE
    [TestMethod(), Timeout(2000)]
    [TestCategory("5")]
    [ExpectedException(typeof(InvalidNameException))]
    public void TestSetInvalidNameDouble()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("1A1A", "1.5");
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("6")]
    public void TestSimpleSetDouble()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("Z7", "1.5");
        Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
    }

    // SETTING CELL TO A STRING
    [TestMethod(), Timeout(2000)]
    [TestCategory("9")]
    [ExpectedException(typeof(InvalidNameException))]
    public void TestSetSimpleString()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("1AZ", "hello");
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("10")]
    public void TestSetGetSimpleString()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("Z7", "hello");
        Assert.AreEqual("hello", s.GetCellContents("Z7"));
    }

    // SETTING CELL TO A FORMULA
    [TestMethod(), Timeout(2000)]
    [TestCategory("13")]
    [ExpectedException(typeof(InvalidNameException))]
    public void TestSetSimpleForm()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("1AZ", "=2");
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("14")]
    public void TestSetGetForm()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("Z7", "=3");
        Formula f = (Formula)s.GetCellContents("Z7");
        Assert.AreEqual(new Formula("3"), f);
        Assert.AreNotEqual(new Formula("2"), f);
    }

    // CIRCULAR FORMULA DETECTION
    [TestMethod(), Timeout(2000)]
    [TestCategory("15")]
    [ExpectedException(typeof(CircularException))]
    public void TestSimpleCircular()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=A2");
        s.SetContentsOfCell("A2", "=A1");
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("16")]
    [ExpectedException(typeof(CircularException))]
    public void TestComplexCircular()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=A2+A3");
        s.SetContentsOfCell("A3", "=A4+A5");
        s.SetContentsOfCell("A5", "=A6+A7");
        s.SetContentsOfCell("A7", "=A1+A1");
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("17")]
    [ExpectedException(typeof(CircularException))]
    public void TestUndoCircular()
    {
        Spreadsheet s = new Spreadsheet();
        try
        {
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "15");
            s.SetContentsOfCell("A3", "30");
            s.SetContentsOfCell("A2", "=A3*A1");
        }
        catch (CircularException e)
        {
            Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9);
            throw e;
        }
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("17b")]
    [ExpectedException(typeof(CircularException))]
    public void TestUndoCellsCircular()
    {
        Spreadsheet s = new Spreadsheet();
        try
        {
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }
        catch (CircularException e)
        {
            Assert.AreEqual("", s.GetCellContents("A2"));
            var n = s.GetNamesOfAllNonemptyCells();
            Assert.IsTrue(new HashSet<string> { "A1" }.SetEquals(n));
            throw e;
        }
    }

    // NONEMPTY CELLS
    [TestMethod(), Timeout(2000)]
    [TestCategory("18")]
    public void TestEmptyNames()
    {
        Spreadsheet s = new Spreadsheet();
        Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("19")]
    public void TestExplicitEmptySet()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "");
        Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("20")]
    public void TestSimpleNamesString()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "hello");
        Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("21")]
    public void TestSimpleNamesDouble()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "52.25");
        Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("22")]
    public void TestSimpleNamesFormula()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "=3.5");
        Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("23")]
    public void TestMixedNames()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "17.2");
        s.SetContentsOfCell("C1", "hello");
        s.SetContentsOfCell("B1", "=3.5");
        Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "A1", "B1", "C1" }));
    }

    // RETURN VALUE OF SET CELL CONTENTS
    [TestMethod(), Timeout(2000)]
    [TestCategory("24")]
    public void TestSetSingletonDouble()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("B1", "hello");
        s.SetContentsOfCell("C1", "=5");
        Assert.IsTrue(s.SetContentsOfCell("A1", "17.2").SequenceEqual(new List<string>() { "A1" }));
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("25")]
    public void TestSetSingletonString()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "17.2");
        s.SetContentsOfCell("C1", "=5");
        Assert.IsTrue(s.SetContentsOfCell("B1", "hello").SequenceEqual(new List<string>() { "B1" }));
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("26")]
    public void TestSetSingletonFormula()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "17.2");
        s.SetContentsOfCell("B1", "hello");
        Assert.IsTrue(s.SetContentsOfCell("C1", "=5").SequenceEqual(new List<string>() { "C1" }));
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("27")]
    public void TestSetChain()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=A2+A3");
        s.SetContentsOfCell("A2", "6");
        s.SetContentsOfCell("A3", "=A2+A4");
        s.SetContentsOfCell("A4", "=A2+A5");
        var c = s.SetContentsOfCell("A5", "82.5");
        Assert.IsTrue(c.SequenceEqual(new List<string>() { "A5", "A4", "A3", "A1" }));
    }

    // CHANGING CELLS
    [TestMethod(), Timeout(2000)]
    [TestCategory("28")]
    public void TestChangeFtoD()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=A2+A3");
        s.SetContentsOfCell("A1", "2.5");
        Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
    }

    //[TestMethod(), Timeout(2000)]
    //[TestCategory("29")]
    //public void TestChangeFtoS()
    //{
    //    Spreadsheet s = new Spreadsheet();
    //    s.SetCellContents("A1", new Formula("A2+A3"));
    //    s.SetCellContents("A1", "Hello");
    //    Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
    //}

    [TestMethod(), Timeout(2000)]
    [TestCategory("30")]
    public void TestChangeStoF()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "Hello");
        s.SetContentsOfCell("A1", "=23");
        Assert.AreEqual(new Formula("23"), (Formula)s.GetCellContents("A1"));
        Assert.AreNotEqual(new Formula("24"), (Formula)s.GetCellContents("A1"));
    }

    // STRESS TESTS
    [TestMethod(), Timeout(2000)]
    [TestCategory("31")]
    public void TestStress1()
    {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=B1+B2");
        s.SetContentsOfCell("B1", "=C1-C2");
        s.SetContentsOfCell("B2", "=C3*C4");
        s.SetContentsOfCell("C1", "=D1*D2");
        s.SetContentsOfCell("C2", "=D3*D4");
        s.SetContentsOfCell("C3", "=D5*D6");
        s.SetContentsOfCell("C4", "=D7*D8");
        s.SetContentsOfCell("D1", "=E1");
        s.SetContentsOfCell("D2", "=E1");
        s.SetContentsOfCell("D3", "=E1");
        s.SetContentsOfCell("D4", "=E1");
        s.SetContentsOfCell("D5", "=E1");
        s.SetContentsOfCell("D6", "=E1");
        s.SetContentsOfCell("D7", "=E1");
        s.SetContentsOfCell("D8", "=E1");
        IList<String> cells = s.SetContentsOfCell("E1", "0");
        Assert.IsTrue(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }.SetEquals(cells));
    }

    // Repeated for extra weight
    [TestMethod(), Timeout(2000)]
    [TestCategory("32")]
    public void TestStress1a()
    {
        TestStress1();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("33")]
    public void TestStress1b()
    {
        TestStress1();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("34")]
    public void TestStress1c()
    {
        TestStress1();
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("35")]
    public void TestStress2()
    {
        Spreadsheet s = new Spreadsheet();
        ISet<String> cells = new HashSet<string>();
        for (int i = 1; i < 200; i++)
        {
            cells.Add("A" + i);
            Assert.IsTrue(cells.SetEquals(s.SetContentsOfCell("A" + i, "=A" + (i + 1))));
        }
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("36")]
    public void TestStress2a()
    {
        TestStress2();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("37")]
    public void TestStress2b()
    {
        TestStress2();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("38")]
    public void TestStress2c()
    {
        TestStress2();
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("39")]
    public void TestStress3()
    {
        Spreadsheet s = new Spreadsheet();
        for (int i = 1; i < 200; i++)
        {
            s.SetContentsOfCell("A" + i, "=A" + (i + 1));
        }
        try
        {
            s.SetContentsOfCell("A150", "=A50");
            Assert.Fail();
        }
        catch (CircularException)
        {
        }
    }

    [TestMethod(), Timeout(2000)]
    [TestCategory("40")]
    public void TestStress3a()
    {
        TestStress3();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("41")]
    public void TestStress3b()
    {
        TestStress3();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("42")]
    public void TestStress3c()
    {
        TestStress3();
    }

    [TestMethod()]
    [TestCategory("43")]
    public void TestStress4()
    {
        Spreadsheet s = new Spreadsheet();
        for (int i = 0; i < 500; i++)
        {
            s.SetContentsOfCell("A1" + i, "=A1" + (i + 1));
        }
        LinkedList<string> firstCells = new LinkedList<string>();
        LinkedList<string> lastCells = new LinkedList<string>();
        for (int i = 0; i < 250; i++)
        {
            firstCells.AddFirst("A1" + i);
            lastCells.AddFirst("A1" + (i + 250));
        }
        Assert.IsTrue(s.SetContentsOfCell("A1249", "25.0").SequenceEqual(firstCells));
        var c = s.SetContentsOfCell("A1499", "0");
        foreach (var t in c)
        {
            Console.Write(t + ", ");
        }
        Console.WriteLine();
        foreach (var t in lastCells)
        {
            Console.Write(t + ", ");
        }
        Console.Write(":");
        foreach (var t in firstCells)
        {
            Console.Write(t + ", ");
        }
        Assert.IsTrue(c.SequenceEqual(lastCells));
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("44")]
    public void TestStress4a()
    {
        TestStress4();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("45")]
    public void TestStress4b()
    {
        TestStress4();
    }
    [TestMethod(), Timeout(2000)]
    [TestCategory("46")]
    public void TestStress4c()
    {
        TestStress4();
    }

    /// <summary>
    /// Generates a random cell name with a capital letter and number between 1 - 99
    /// </summary>
    /// <param name="rand"></param>
    /// <returns></returns>
    private String randomName(Random rand)
    {
        return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
    }

    /// <summary>
    /// Generates a random Formula
    /// </summary>
    /// <param name="rand"></param>
    /// <returns></returns>
    private String randomFormula(Random rand)
    {
        String f = randomName(rand);
        for (int i = 0; i < 10; i++)
        {
            switch (rand.Next(4))
            {
                case 0:
                    f += "+";
                    break;
                case 1:
                    f += "-";
                    break;
                case 2:
                    f += "*";
                    break;
                case 3:
                    f += "/";
                    break;
            }
            switch (rand.Next(2))
            {
                case 0:
                    f += 7.2;
                    break;
                case 1:
                    f += randomName(rand);
                    break;
            }
        }
        return f;
    }

}