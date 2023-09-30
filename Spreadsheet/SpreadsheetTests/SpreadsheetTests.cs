using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTests
{
    
    [TestClass]
    public class SpreadsheetTests
    {
        //PS5 My Tests--------------------------------------------------------------------------------
        [TestMethod()]
        public void TestAbstractSpreadsheetPolymorphism()
        {
            AbstractSpreadsheet sheet1 = new Spreadsheet();
            AbstractSpreadsheet sheet2 = new Spreadsheet((s) => true, (s) => s, "default"); ;
            AbstractSpreadsheet sheet3 = new Spreadsheet("test.json", (s) => true, (s) => s, "default");
        }


        [TestMethod()]
        public void TestNonDefaultNormalize()
        {
            Spreadsheet s = new Spreadsheet((s) => true, (s) => s.ToUpper(),"default");
            s.SetContentsOfCell("a1", "hi");
            s.SetContentsOfCell("b1", "2");
            s.SetContentsOfCell("c1", "=B1 + 2");
            s.SetContentsOfCell("d1", "=b1 + 2");
            Assert.AreEqual(4d, s.GetCellValue("C1"));
            Assert.AreEqual(4d, s.GetCellValue("D1"));
            Assert.AreEqual(4d, s.GetCellValue("c1"));
            Assert.AreEqual(4d, s.GetCellValue("d1"));
            Assert.AreEqual("hi", s.GetCellValue("A1"));
            Assert.AreEqual("hi", s.GetCellContents("A1"));
            Assert.AreEqual("hi", s.GetCellValue("a1"));
            Assert.AreEqual("hi", s.GetCellContents("a1"));
        }

        [TestMethod()]
        public void TestSaveAndLoad()
        {
            Spreadsheet s1 = new Spreadsheet((s) => true, (s) => s.ToUpper(),"default");
            s1.SetContentsOfCell("a1", "hi");
            s1.SetContentsOfCell("b1", "2");
            s1.SetContentsOfCell("c1", "=B1 + 2");
            s1.Save("test.json");

            //check that all cells are the same both contents and values
            Spreadsheet s2 = new Spreadsheet("test.json", (s) => true, (s) => s.ToUpper(), "default");
            Assert.AreEqual(s1.GetCellValue("A1"), s2.GetCellValue("A1"));
            Assert.AreEqual(s1.GetCellContents("A1"), s2.GetCellContents("A1"));
            Assert.AreEqual(s1.GetCellValue("B1"), s2.GetCellValue("B1"));
            Assert.AreEqual(s1.GetCellContents("B1"), s2.GetCellContents("B1"));
            Assert.AreEqual(s1.GetCellValue("C1"), s2.GetCellValue("C1"));
            Assert.AreEqual(s1.GetCellContents("C1"), s2.GetCellContents("C1"));
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestLoadInvalidFileName()
        {
           var s = new Spreadsheet("DNE.json", (s) => true, (s) => s.ToUpper(), "default");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestLoadCircularDependency()
        {
            using (StreamWriter outputFile = new StreamWriter("circular.json")) { outputFile.Write("{\"Cells\":{\"A1\":{\"StringForm\":\"=A2\"},\"A2\":{\"StringForm\":\"=A1\\u002B1\"}},\"Version\":\"default\"}"); }
            var s = new Spreadsheet("circular.json", (s) => true, (s) => s.ToUpper(), "default");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestLoadFileEmptyValues()
        {
            using (StreamWriter outputFile = new StreamWriter("emptyvalues.json")) { outputFile.Write("{}"); }

            var s = new Spreadsheet("emptyvalues.json", (s) => true, (s) => s.ToUpper(), "default");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestLoadFileEmptyFile()
        {
            using (StreamWriter outputFile = new StreamWriter("empty.json")) { outputFile.Write(""); }

            var s = new Spreadsheet("empty.json", (s) => true, (s) => s.ToUpper(), "default");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestLoadWrongVersion()
        {
            using (StreamWriter outputFile = new StreamWriter("wrongversion.json")) { outputFile.Write("{\"Cells\":{\"A1\":{\"StringForm\":\"2\"},\"A2\":{\"StringForm\":\"=A3\\u002B1\"}},\"Version\":\"NOT Default\"}"); }

            var s = new Spreadsheet("wrongversion.json", (s) => true, (s) => s.ToUpper(), "default");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestSaveError()
        {
            Spreadsheet s1 = new Spreadsheet((s) => true, (s) => s.ToUpper(), "default");
            s1.SetContentsOfCell("a1", "hi");
            s1.SetContentsOfCell("b1", "2");
            s1.SetContentsOfCell("c1", "=B1 + 2");
            s1.Save("notvalid\\test.json");
        }


        //PS4 Grading Tests---------------------------------------------------------------------------
        [TestMethod()]
        [TestCategory("2")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestEmptyGetContents()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("1AA");
        }

        [TestMethod()]
        [TestCategory("3")]
        public void TestGetEmptyContents()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod()]
        [TestCategory("5")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetInvalidNameDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1A1A", "1.5");
        }

        [TestMethod()]
        [TestCategory("6")]
        public void TestSimpleSetDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "1.5");
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod()]
        [TestCategory("9")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "hello");
        }

        [TestMethod()]
        [TestCategory("10")]
        public void TestSetGetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod()]
        [TestCategory("13")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleForm()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "=2");
        }

        [TestMethod()]
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
        [TestMethod()]
        [TestCategory("15")]
        [ExpectedException(typeof(CircularException))]
        public void TestSimpleCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod()]
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

        [TestMethod()]
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

        [TestMethod()]
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
        [TestMethod()]
        [TestCategory("18")]
        public void TestEmptyNames()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        [TestCategory("19")]
        public void TestExplicitEmptySet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        [TestCategory("20")]
        public void TestSimpleNamesString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        [TestCategory("21")]
        public void TestSimpleNamesDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "52.25");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
        [TestCategory("22")]
        public void TestSimpleNamesFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod()]
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
        [TestMethod()]
        [TestCategory("24")]
        public void TestSetSingletonDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("A1", "17.2").SequenceEqual(new List<string>() { "A1" }));
        }

        [TestMethod()]
        [TestCategory("25")]
        public void TestSetSingletonString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("B1", "hello").SequenceEqual(new List<string>() { "B1" }));
        }

        [TestMethod()]
        [TestCategory("26")]
        public void TestSetSingletonFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(s.SetContentsOfCell("C1", "=5").SequenceEqual(new List<string>() { "C1" }));
        }

        [TestMethod()]
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
        [TestMethod()]
        [TestCategory("28")]
        public void TestChangeFtoD()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "2.5");
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        //[TestMethod()]
        //[TestCategory("29")]
        //public void TestChangeFtoS()
        //{
        //    Spreadsheet s = new Spreadsheet();
        //    s.SetCellContents("A1", new Formula("A2+A3"));
        //    s.SetCellContents("A1", "Hello");
        //    Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        //}

        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        [TestCategory("32")]
        public void TestStress1a()
        {
            TestStress1();
        }
        [TestMethod()]
        [TestCategory("33")]
        public void TestStress1b()
        {
            TestStress1();
        }
        [TestMethod()]
        [TestCategory("34")]
        public void TestStress1c()
        {
            TestStress1();
        }

        [TestMethod()]
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
        [TestMethod()]
        [TestCategory("36")]
        public void TestStress2a()
        {
            TestStress2();
        }
        [TestMethod()]
        [TestCategory("37")]
        public void TestStress2b()
        {
            TestStress2();
        }
        [TestMethod()]
        [TestCategory("38")]
        public void TestStress2c()
        {
            TestStress2();
        }

        [TestMethod()]
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

        [TestMethod()]
        [TestCategory("40")]
        public void TestStress3a()
        {
            TestStress3();
        }
        [TestMethod()]
        [TestCategory("41")]
        public void TestStress3b()
        {
            TestStress3();
        }
        [TestMethod()]
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
            Assert.IsTrue(c.SequenceEqual(lastCells));
        }
        [TestMethod()]
        [TestCategory("44")]
        public void TestStress4a()
        {
            TestStress4();
        }
        [TestMethod()]
        [TestCategory("45")]
        public void TestStress4b()
        {
            TestStress4();
        }
        [TestMethod()]
        [TestCategory("46")]
        public void TestStress4c()
        {
            TestStress4();
        }

        [TestMethod()]
        [TestCategory("47")]
        public void TestStress4WithText()
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
            var c = s.SetContentsOfCell("A1499", "hi");
            Assert.IsTrue(c.SequenceEqual(lastCells));
        }


        //PS4 My Tests--------------------------------------------------------------------------------
        [TestMethod()]
        public void TestSetCellContentsString()
        {
            var s = new Spreadsheet();
            s.SetContentsOfCell("A1", "hello");
            Assert.AreEqual("hello",s.GetCellContents("A1"));
        }

        [TestMethod()]
        public void TestSetCellContentsDouble()
        {
            var s = new Spreadsheet();
            s.SetContentsOfCell("b1", "2");
            Assert.AreEqual(2d, s.GetCellContents("b1"));
        }

        [TestMethod()]
        public void TestSetCellContentsInvalidName()
        {
            var s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("1f", "hello"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("1f", "2"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("1f", "=2+2"));
        }

        [TestMethod()]
        public void TestSetCellContentsFormula()
        {
            var f = new Formula("2+2");
            var s = new Spreadsheet();
            s.SetContentsOfCell("_1", "=2+2");
            Assert.IsTrue(f.Equals(s.GetCellContents("_1")));
        }

        [TestMethod()]
        public void TestSetCellContentsComplexFormulaWithVariables()
        {
            var f = new Formula("2+A1*10/(5-B2+C2)");
            var s = new Spreadsheet();
            s.SetContentsOfCell("A1", "hi");
            s.SetContentsOfCell("_1", "=2+A1*10/(5-B2+C2)");
            Assert.IsTrue(f.Equals(s.GetCellContents("_1")));
        }

        [TestMethod()]
        public void TestLookupVariable()
        {
            var f = new Formula("2+A1");
            var s = new Spreadsheet();
            s.SetContentsOfCell("_1", "=2+A1");
            s.SetContentsOfCell("A1", "2");
            Assert.AreEqual(4d, s.GetCellValue("_1"));
        }

        [TestMethod()]
        public void TestCircularException()
        {
            var f = new Formula("2+A1");
            var s = new Spreadsheet();
            s.SetContentsOfCell("_1", "=2+A1");
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("A1", "=3 * _1"));
        }

        [TestMethod()]
        public void TestGetCellValue()
        {
            var s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellValue("1f"));
            Assert.AreEqual("", s.GetCellValue("a2"));
        }

        [TestMethod()]
        public void TestGetCellContentsInvalidName()
        {
            var s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() =>  s.GetCellContents("1f"));
        }

        [TestMethod()]
        public void TestGetCellContentsEmptyCell()
        {
            var s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("a2"));
        }

        [TestMethod()]
        public void TestGetAllNonEmptyCells()
        {
            var s = new Spreadsheet();

            for(int i = 0; i < 10; i++)
            {
                s.SetContentsOfCell("a" + i, "hi");
            }

            var c = s.GetNamesOfAllNonemptyCells().ToArray();

            for(int i = 0; i < 10; i++)
            {
                Assert.IsTrue(c.Contains("a" + i));
            }
        }

        [TestMethod()]
        public void TestGetAllNonEmptyCellsAfterClearingCell()
        {
            var s = new Spreadsheet();

            s.SetContentsOfCell("a1", "2");
            s.SetContentsOfCell("a1", "");

            var c = s.GetNamesOfAllNonemptyCells().ToArray();
            Assert.AreEqual(0, c.Length);
        }
    }
}
