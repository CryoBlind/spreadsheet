using System;
using System.Collections.Generic;
using System.Linq;
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
        [TestMethod()]
        public void TestSetCellContentsString()
        {
            var s = new Spreadsheet();
            s.SetCellContents("A1", "hello");
            Assert.AreEqual("hello",s.GetCellContents("A1"));
        }

        [TestMethod()]
        public void TestSetCellContentsDouble()
        {
            var s = new Spreadsheet();
            s.SetCellContents("b1", 2);
            Assert.AreEqual(2d, s.GetCellContents("b1"));
        }

        [TestMethod()]
        public void TestSetCellContentsInvalidName()
        {
            var s = new Spreadsheet();
            Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("1f", "hello"));
            Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("1f", 2d));
            Assert.ThrowsException<InvalidNameException>(() => s.SetCellContents("1f", new Formula("2+2")));
        }

        [TestMethod()]
        public void TestSetCellContentsFormula()
        {
            var f = new Formula("2+2");
            var s = new Spreadsheet();
            s.SetCellContents("_1", f);
            Assert.IsTrue(f.Equals(s.GetCellContents("_1")));
        }

        [TestMethod()]
        public void TestSetCellContentsComplexFormulaWithVariables()
        {
            var f = new Formula("2+A1*10/(5-B2+C2)");
            var s = new Spreadsheet();
            s.SetCellContents("A1", "hi");
            s.SetCellContents("_1", f);
            Assert.IsTrue(f.Equals(s.GetCellContents("_1")));
        }

        [TestMethod()]
        public void TestLookupVariable()
        {
            var f = new Formula("2+A1");
            var s = new Spreadsheet();
            s.SetCellContents("_1", f);
            s.SetCellContents("A1", 2d);
            Assert.AreEqual("4", s.GetCellValue("_1"));
        }

        [TestMethod()]
        public void TestCircularException()
        {
            var f = new Formula("2+A1");
            var s = new Spreadsheet();
            s.SetCellContents("_1", f);
            Assert.ThrowsException<CircularException>(() => s.SetCellContents("A1", new Formula("3 * _1")));
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
                s.SetCellContents("a" + i, "hi");
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

            s.SetCellContents("a1", 2);
            s.SetCellContents("a1", "");

            var c = s.GetNamesOfAllNonemptyCells().ToArray();
            Assert.AreEqual(0, c.Length);
        }
    }
}
