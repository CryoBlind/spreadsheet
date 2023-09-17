using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTests
{
    [TestClass()]
    public class FormulaTests
    {
        //Validate() tests-----------------------------------------------------------------------------------------------------
        [TestMethod()]
        public void TestValidFormula()
        {
            var f = new Formula("1 + 1");
            var s = "1+1";
            Assert.IsTrue(s.Equals(f.ToString()));
        }
        [TestMethod()]
        public void TestParseError()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("1 + 1 + $"));
        }

        [TestMethod()]
        public void TestEmptyError()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula(""));
        }

        [TestMethod()]
        public void TestRightParenRule()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("1 + (1+1)+1)"));
        }

        [TestMethod()]
        public void TestUnbalancedParens()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("1+(1+(1+1)"));
        }

        [TestMethod()]
        public void TestStartTokenRule()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("*5+2"));
        }

        [TestMethod()]
        public void TestEndTokenRule()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("5+2*"));
        }

        [TestMethod()]
        public void TestOperatorFollowingRule()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("1+(*2+1)"));
        }

        [TestMethod()]
        public void TestExtraFollowingRule()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("1 + 2 2"));
        }

        [TestMethod()]
        public void TestExtraFollowingRule2()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("1 + a a"));
        }

        [TestMethod()]
        public void TestCustomValidator()
        {
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("1 + 2 + s7", s => s, s => false));
        }

        //Evaluate() tests-----------------------------------------------------------------------------------------------------

        [TestMethod(), Timeout(5000)]
        [TestCategory("1")]
        public void TestSingleNumber()
        {
            var f = new Formula("5");
            Assert.AreEqual(5d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("2")]
        public void TestSingleVariable()
        {
            var f = new Formula("X5");
            Assert.AreEqual(13d, (double)f.Evaluate(s => 13));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("3")]
        public void TestAddition()
        {
            var f = new Formula("5 + 3");
            Assert.AreEqual(8d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("4")]
        public void TestSubtraction()
        {
            var f = new Formula("18-10-5");
            Assert.AreEqual(3d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("5")]
        public void TestMultiplication()
        {
            var f = new Formula("2*4");
            Assert.AreEqual(8d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("6")]
        public void TestDivision()
        {
            var f = new Formula("14/(5+4/2)");
            Assert.AreEqual(2d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("7")]
        public void TestArithmeticWithVariable()
        {
            var f = new Formula("2+X1");
            Assert.AreEqual(6d, (double)f.Evaluate(s => 4));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("8")]
        public void TestUnknownVariable()
        {
            var f = new Formula("2+X1");
            Assert.AreEqual(typeof(FormulaError), f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }).GetType());
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("9")]
        public void TestLeftToRight()
        {
            var f = new Formula("2*6+3");
            Assert.AreEqual(15d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("10")]
        public void TestOrderOperations()
        {
            var f = new Formula("2+6*3");
            Assert.AreEqual(20d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("11")]
        public void TestParenthesesTimes()
        {
            var f = new Formula("(2+6)*3");
            Assert.AreEqual(24d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("12")]
        public void TestTimesParentheses()
        {
            var f = new Formula("2*(3+5)");
            Assert.AreEqual(16d, (double)(double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("13")]
        public void TestPlusParentheses()
        {
            var f = new Formula("2+(3+5)");
            Assert.AreEqual(10d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("14")]
        public void TestPlusComplex()
        {
            var f = new Formula("2+(3+5*9)");
            Assert.AreEqual(50d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("15")]
        public void TestOperatorAfterParens()
        {
            var f = new Formula("(1*1)-2/2");
            Assert.AreEqual(0d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("16")]
        public void TestComplexTimesParentheses()
        {
            var f = new Formula("2+3*(3+5)");
            Assert.AreEqual(26d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("17")]
        public void TestComplexAndParentheses()
        {
            var f = new Formula("2+3*5+(3+4*8)*5+2");
            Assert.AreEqual(194d, (double)f.Evaluate(s => 0));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("18")]
        public void TestDivideByZero()
        {
            var f = new Formula("5/0");
            Assert.AreEqual(typeof(FormulaError), f.Evaluate(s => 0).GetType());
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("18")]
        public void TestDivideByZeroWithParens()
        {
            var f = new Formula("5/(2-2)");
            Assert.AreEqual(typeof(FormulaError), f.Evaluate(s => 0).GetType());
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("26")]
        public void TestComplexMultiVar()
        {
            var f = new Formula("y1*3-8/2+4*(8-9*2)*x7");
            Assert.AreEqual(-32d, (double)f.Evaluate(s => (s == "x7") ? 1 : 4));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("27")]
        public void TestComplexNestedParensRight()
        {
            var f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
            Assert.AreEqual(6d, (double)f.Evaluate(s => 1));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("28")]
        public void TestComplexNestedParensLeft()
        {
            var f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
            Assert.AreEqual(12d, (double)f.Evaluate(s => 2));
        }

        [TestMethod(), Timeout(5000)]
        [TestCategory("29")]
        public void TestRepeatedVar()
        {
            var f = new Formula("a4-a4*a4/a4");
            Assert.AreEqual(0d, (double)f.Evaluate(s => 3));
        }

        //other method tests-------------------------------------------------------------------------------------------------------------------
        [TestMethod()]
        public void TestGetVariables()
        {
            var f = new Formula("x + y + z");
            var vars = f.GetVariables().ToList();
            var correct = new List<string>();
            correct.Add("x");
            correct.Add("y");
            correct.Add("z");
            for(int i = 0; i < vars.Count; i++)
            {
                Assert.AreEqual(correct[i], vars[i]);
            }
        }

        [TestMethod()]
        public void TestGetVariablesWithDuplicates()
        {
            var f = new Formula("x + y + z + z + y");
            var vars = f.GetVariables().ToList();
            var correct = new List<string>();
            correct.Add("x");
            correct.Add("y");
            correct.Add("z");
            for (int i = 0; i < vars.Count; i++)
            {
                Assert.AreEqual(correct[i], vars[i]);
            }
        }

        [TestMethod()]
        public void TestEqualFormulas()
        {
            var f = new Formula("x + 10 + 5      +2.00 +2e2 -10.000000000");
            var f2 = new Formula("x+10 +   5 +2 +200 -10.00000");

            Assert.IsTrue(f.Equals(f2));
        }

        [TestMethod()]
        public void TestUnEqualFormulas()
        {
            var f = new Formula("x + 10 + 5      +2.00 +2e2 -10.000000000");
            var f2 = new Formula("x+10 +   5.2 +2 +200 -10.00000");

            Assert.IsFalse(f.Equals(f2));
        }

        [TestMethod()]
        public void TestEqualNull()
        {
            var f = new Formula("x + 10 + 5      +2.00 +2e2 -10.000000000");
            var f2 = new Formula("x+10 +   5.2 +2 +200 -10.00000");

            Assert.IsFalse(f.Equals(null));
        }

        [TestMethod()]
        public void TestEqualNotFormula()
        {
            var f = new Formula("x + 10 + 5      +2.00 +2e2 -10.000000000");
            var f2 = new Formula("x+10 +   5.2 +2 +200 -10.00000");

            var s = new Object();

            Assert.IsFalse(f.Equals(s));
        }

        [TestMethod()]
        public void TestGetHash()
        {
            var f = new Formula("x + 10 + 5      +2.00 +2e2 -10.000000000");
            var f2 = new Formula("x+10 +   5.0 +2 +200 -10.00000");
            var f3 = new Formula("x+10 +   5.0 +2 +201 -10.00000");

            Assert.IsTrue(f.GetHashCode() == f2.GetHashCode());
            Assert.IsTrue(f.GetHashCode() != f3.GetHashCode());
        }

        [TestMethod()]
        public void TestOverrideOperators()
        {
            var f = new Formula("x + 10 + 5      +2.00 +2e2 -10.000000000");
            var f2 = new Formula("x+10 +   5.0 +2 +200 -10.00000");
            var f3 = new Formula("x+10 +   5.0 +2 +201 -10.00000");

            Assert.IsTrue(f == f2);
            Assert.IsTrue(f != f3);
        }
    }
}
