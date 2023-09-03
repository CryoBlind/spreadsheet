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
    public static class Evaluator
    {
        public delegate int Lookup(String v);
        public static int Evaluate(string exp, Lookup variableEvaluator)
        {
            Stack<String> ops = new Stack<String>();
            Stack<int> values = new Stack<int>();

            string[] subs = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            int length = subs.Length;

            for (int i = 0; i < length; i++)
            {
                int c = subs[i].ElementAt(0);
                int n = 0;
                bool isInteger = false;

                //check for integers or whitespace
                switch (c)
                {
                    case 32:
                        //whitespace case - skip
                        continue;

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
                if (isInteger) {
                    if (ops.Peek().Equals("*") || ops.Peek().Equals("/"))
                    {
                        // * or / is on top of stack
                        //TODO check for divide by zero and ensure value stack is not empty
                        var result = values.Pop();
                        var op = ops.Pop();
                        //apply operator
                        switch (op)
                        {
                            case "*":
                                result = result * n;
                                break;
                            case "/":
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
                switch (c)
                {
                    case 43 or 45:
                        //+- case
                        //TODO handle if the value stack has < 2 values
                        {
                            switch (ops.Peek())
                            {
                                case "+":
                                    ops.Pop();
                                    values.Push(values.Pop() + values.Pop());
                                    break;

                                case "-":
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
                        //TODO handle if the value stack has < 2 values, handle a '(' not being found where it should be, and check for divide by 0
                        switch (ops.Peek())
                        {
                            case "+":
                                ops.Pop();
                                values.Push(values.Pop() + values.Pop());
                                break;

                            case "-":
                                ops.Pop();
                                values.Push(values.Pop() - values.Pop());
                                break;

                            case "*":
                                ops.Pop();
                                values.Push(values.Pop() * values.Pop());
                                break;

                            case "/":
                                ops.Pop();
                                values.Push(values.Pop() / values.Pop());
                                break;
                        }
                        ops.Pop();
                        continue;
                    default:
                        continue;
                }

            }
            if(ops.Count == 0)
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
    }
}
