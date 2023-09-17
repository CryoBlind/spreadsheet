using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaEvaluator
{
    /// <summary>
    /// This class contains methods to evaluate basic math functions
    /// </summary>
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /// <summary>
        /// This method takes a expression in the form a string and evaluates the result
        /// </summary>
        /// <param name="exp">The expression to evaluate</param>
        /// <param name="variableEvaluator">The lookup for variables</param>
        /// <returns>the Integer solution of the expression</returns>
        //    public static int Evaluate(string exp, Lookup variableEvaluator)
        //    {
        //        Stack<String> ops = new Stack<String>();
        //        Stack<int> values = new Stack<int>();

        //        string[] subs = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
        //        int length = subs.Length;

        //        //trim whitespace
        //        for (int i = 0; i < length; i++) subs[i] = subs[i].Trim();

        //        for (int i = 0; i < length; i++)
        //        {
        //            if (subs[i].Length == 0) { continue; }
        //            int c = subs[i].ElementAt(0);
        //            int n = 0;
        //            bool isInteger = false;

        //            //check for integers or whitespace
        //            switch (c)
        //            {
        //                case > 47 and < 58:
        //                    //integer case - translate substring to int
        //                    n = int.Parse(subs[i]);
        //                    isInteger = true;
        //                    break;

        //                case (> 64 and < 91) or (> 97 and < 123):
        //                    //variable case - fetch variable data
        //                    try
        //                    {
        //                        n = variableEvaluator(subs[i]);
        //                        isInteger = true;
        //                    }
        //                    catch (ArgumentException e)
        //                    {
        //                        //no variable data
        //                        Console.WriteLine(e.Message);
        //                        break;
        //                    }
        //                    break;

        //                default:
        //                    //operator or unrecognized
        //                    isInteger = false;
        //                    break;
        //            }

        //            //handle integers
        //            if (isInteger) {
        //                if (ops.Count > 0 && (ops.Peek().Equals("*") || ops.Peek().Equals("/")))
        //                {
        //                    // * or / is on top of stack
        //                    if (values.Count == 0) throw new ArgumentException("missing values");
        //                    var result = values.Pop();
        //                    var op = ops.Pop();
        //                    //apply operator
        //                    switch (op)
        //                    {
        //                        case "*":
        //                            result = result * n;
        //                            break;
        //                        case "/":
        //                            if (n == 0) throw new ArgumentException("Can't divide by zero");
        //                            result = result / n;
        //                            break;
        //                    }

        //                    values.Push(result);
        //                    continue;
        //                }
        //                else
        //                {
        //                    values.Push(n);
        //                    continue;
        //                }
        //            }

        //            //handle operators
        //            if (ops.Count > 0)
        //            {
        //                switch (c)
        //                {
        //                    case 43 or 45:
        //                        //+- case
        //                        {
        //                            switch (ops.Peek())
        //                            {
        //                                case "+":
        //                                    if (values.Count < 2) throw new ArgumentException("Invalid input");
        //                                    ops.Pop();
        //                                    values.Push(values.Pop() + values.Pop());
        //                                    break;

        //                                case "-":
        //                                    if (values.Count < 2) throw new ArgumentException("Invalid input");
        //                                    ops.Pop();
        //                                    values.Push(values.Pop() - values.Pop());
        //                                    break;
        //                            }
        //                            ops.Push(subs[i]);
        //                            continue;
        //                        }

        //                    case 42 or 47:
        //                        //*/ case
        //                        ops.Push(subs[i]);
        //                        continue;
        //                    case 40:
        //                        //( case
        //                        ops.Push(subs[i]);
        //                        continue;
        //                    case 41:
        //                        //) case
        //                        switch (ops.Peek())
        //                        {
        //                            case "+":
        //                                if (values.Count < 2) throw new ArgumentException("Invalid input");
        //                                ops.Pop();
        //                                values.Push(values.Pop() + values.Pop());
        //                                break;

        //                            case "-":
        //                                if (values.Count < 2) throw new ArgumentException("Invalid input");
        //                                ops.Pop();
        //                                values.Push(values.Pop() - values.Pop());
        //                                break;

        //                            case "*":
        //                                ops.Pop();
        //                                values.Push(values.Pop() * values.Pop());
        //                                break;

        //                            case "/":
        //                                var v1 = values.Pop();
        //                                var v2 = values.Pop();
        //                                if (v2 == 0) throw new ArgumentException("Can't divide by zero");
        //                                ops.Pop();
        //                                values.Push(v1 / v2);
        //                                break;
        //                        }
        //                        if(ops.Count == 0) throw new ArgumentException("Missing Parenthesis");
        //                        if (ops.Peek().ElementAt(0) != '(') throw new ArgumentException("Missing Parenthesis");
        //                        ops.Pop();
        //                        continue;
        //                    default:
        //                        continue;
        //                }
        //            }
        //            else
        //            {
        //                ops.Push(subs[i]);
        //            }

        //        }
        //        //determine what to return and return
        //        if(ops.Count == 0)
        //        {
        //            return values.Pop();
        //        }
        //        else
        //        {
        //            switch (ops.Pop())
        //            {
        //                case "+":
        //                    values.Push(values.Pop() + values.Pop());
        //                    break;

        //                case "-":
        //                    values.Push(values.Pop() - values.Pop());
        //                    break;
        //            }
        //            return values.Pop();
        //        }
        //    }
        //}

        public static int Evaluate(string exp, Lookup variableEvaluator)
        {
            Stack<String> ops = new Stack<String>();
            Stack<int> values = new Stack<int>();

            string[] subs = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            int length = subs.Length;

            //trim whitespace
            for (int i = 0; i < length; i++) subs[i] = subs[i].Trim();


            for (int i = 0; i < length; i++)
            {
                if (subs[i].Length == 0) { continue; }
                int c = subs[i].ElementAt(0);
                int n = 0;
                bool isInteger = false;

                //check for integers or whitespace
                switch (c)
                {
                    case > 47 and < 58: 
                        //integer case - translate substring to int
                        n = int.Parse(subs[i]);
                        isInteger = true;
                        break;

                    case (> 64 and < 91) or (> 97 and < 123):
                        //variable case - fetch variable data
                        try
                        {
                            n = variableEvaluator(subs[i]);
                            isInteger = true;
                        }
                        catch (ArgumentException e)
                        {
                            //no variable data
                            Console.WriteLine(e.Message);
                            break;
                        }
                        break;

                    default:
                        //operator or unrecognized
                        isInteger = false;
                        break;
                }

                //handle integers
                if (isInteger)
                {
                    if (ops.Count > 0 && (ops.Peek().Equals("*") || ops.Peek().Equals("/")))
                    {
                        // * or / is on top of stack
                        if (values.Count == 0) throw new ArgumentException("missing values");
                        var result = values.Pop();
                        var op = ops.Pop();
                        //apply operator
                        switch (op)
                        {
                            case "*":
                                result = result * n;
                                break;
                            case "/":
                                if (n == 0) throw new ArgumentException("Can't divide by zero");
                                result = result / n;
                                break;
                        }

                        values.Push(result);
                        continue;
                    }
                    else
                    {
                        values.Push(n);
                        continue;
                    }
                }

                //handle operators
                if (ops.Count > 0)
                {
                    switch (c)
                    {
                        case 43 or 45:
                            //+- case
                            {
                                switch (ops.Peek())
                                {
                                    case "+":
                                        if (values.Count < 2) throw new ArgumentException("Invalid input");
                                        ops.Pop();
                                        values.Push(values.Pop() + values.Pop());
                                        break;

                                    case "-":
                                        if (values.Count < 2) throw new ArgumentException("Invalid input");
                                        ops.Pop();
                                        values.Push(values.Pop() - values.Pop());
                                        break;
                                }
                                ops.Push(subs[i]);
                                continue;
                            }

                        case 42 or 47:
                            //*/ case
                            ops.Push(subs[i]);
                            continue;
                        case 40:
                            //( case
                            ops.Push(subs[i]);
                            continue;
                        case 41:
                            //) case
                            switch (ops.Peek())
                            {
                                case "+":
                                    if (values.Count < 2) throw new ArgumentException("Invalid input");
                                    ops.Pop();
                                    values.Push(values.Pop() + values.Pop());
                                    break;

                                case "-":
                                    if (values.Count < 2) throw new ArgumentException("Invalid input");
                                    ops.Pop();
                                    values.Push(values.Pop() - values.Pop());
                                    break;

                                case "*":
                                    ops.Pop();
                                    values.Push(values.Pop() * values.Pop());
                                    break;

                                case "/":
                                    var v1 = values.Pop();
                                    var v2 = values.Pop();
                                    if (v2 == 0) throw new ArgumentException("Can't divide by zero");
                                    ops.Pop();
                                    values.Push(v1 / v2);
                                    break;
                            }
                            if (ops.Count == 0) throw new ArgumentException("Missing Parenthesis");
                            if (ops.Peek().ElementAt(0) != '(') throw new ArgumentException("Missing Parenthesis");
                            ops.Pop();
                            continue;
                        default:
                            continue;
                    }
                }
                else
                {
                    ops.Push(subs[i]);
                }

            }
            //determine what to return and return
            if (ops.Count == 0)
            {
                return values.Pop();
            }
            else
            {
                switch (ops.Pop())
                {
                    case "+":
                        values.Push(values.Pop() + values.Pop());
                        break;

                    case "-":
                        values.Push(values.Pop() - values.Pop());
                        break;
                }
                return values.Pop();
            }
        }

        public static void T(String exp)
        {
            string[] subs = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            int length = subs.Length;

            //trim whitespace
            for (int i = 0; i < length; i++) subs[i] = subs[i].Trim();

            //remove empty strings
            List<String> copy = new List<String>();
            for (int i = 0; i < length; i++)
            {
                if (subs[i].Length > 0) copy.Add(subs[i]);
            }
            subs = copy.ToArray();

            for (int i = 0; i < length; i++)
                Console.Write(subs[i] + "|");
        }
    }
}
