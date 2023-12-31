﻿// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!
// Last updated: August 2023 (small tweak to API)

using System.Collections;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private ArrayList tokens;
    private String stringFormula;
    private Func<string, string> normalize;   

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        tokens = ArrayList.Adapter(GetTokens(formula).ToList());
        int length = tokens.Count;
        

        //trim whitespace
        for (int i = 0; i < length; i++)
        {
            tokens[i] = ((String)tokens[i]!).Trim();
        }

        //normalize.  after, it is no longer garunteed that tokens contains only strings
        tokens = Normalize(this.tokens, normalize);
        this.normalize = normalize;

        //validate.
        Validate(tokens, isValid);

        //store validated, normalized, trimmed string version for equals
        var builder = new StringBuilder();
        foreach (var t in tokens)
        {
            builder.Append(t.ToString());
        }
        stringFormula = builder.ToString();
    }

    private void Validate(ArrayList tokens, Func<string, bool> isValid)
    {
        int leftParens = 0;
        int rightParens = 0;
        bool previousTokenWasOperator = true;

        //2. One Token Rule
        if (tokens.Count == 0) throw new FormulaFormatException("Formula must have at least one token.  Add some.");

        foreach (var token in tokens)
        {
            if (token.GetType() == typeof(System.String))
            {
                //1. Parsing
                if (!Regex.IsMatch((string)token, @"(^[\(\)\+\-\*\/]$)|(^[_a-zA-Z][_a-zA-Z0-9]*$)")) throw new FormulaFormatException("Invalid tokens.  The only accepted tokens are: (, ), +, -, *, /, valid variables, and decimal real numbers (including scientific notation).");

                //isValidVariable
                if (Regex.IsMatch((string)token, @"^[_a-zA-Z][_a-zA-Z0-9]*$"))
                    if (!isValid((string)token)) throw new FormulaFormatException("Validator delegate found invalid token");

                //7. Paren/Operator Following rule
                if (previousTokenWasOperator)
                {
                    if (!Regex.IsMatch((string)token, @"(^\($)|(^[_a-zA-Z][_a-zA-Z0-9]*$)")) throw new FormulaFormatException("Starting token or a token following an opening parenthesis or operator is not a number, variable, or another opening parenthesis");
                }
                //8. Extra Following rule
                else
                {
                    if (Regex.IsMatch((string)token, @"(^\($)|(^[_a-zA-Z][_a-zA-Z0-9]*$)")) throw new FormulaFormatException("A number, variable, or closing parenthesis is not followed by an operator or closing parenthesis");
                }

                //3. right parenthesis
                if (Regex.IsMatch((string)token, @"\)"))
                {
                    rightParens++;
                    if (rightParens > leftParens) throw new FormulaFormatException("Unmatched ')'. Ensure parenthesis are opened.");
                }
                if (Regex.IsMatch((string)token, @"\("))
                {
                    leftParens++;
                }

                //7
                if (Regex.IsMatch((string)token, @"^[\(\+\-\*\/]$")) previousTokenWasOperator = true;
                //8
                else previousTokenWasOperator = false;
            }
            else
            {
                //7. paren/operator Following Rule
                if (previousTokenWasOperator) previousTokenWasOperator = false;
                //8. extra following rule
                else throw new FormulaFormatException("A number, variable, or closing parenthesis is not followed by an operator or closing parenthesis");

                continue;
            }
        }
        //5. Starting token rule
        //if (tokens[0].GetType() == typeof(System.String))
        //    if (!Regex.IsMatch((string)tokens[0], @"(^\($)|(^[_a-zA-Z][_a-zA-Z0-9]*$)")) throw new FormulaFormatException("Starting token is not '(', a variable, or a number.");

        //6. ending token rule
        if (tokens[tokens.Count - 1]!.GetType() == typeof(System.String))
            if (!Regex.IsMatch((string)tokens[tokens.Count - 1]!, @"(^\)$)|(^[_a-zA-Z]{1,1}[_a-zA-Z0-9]*$)")) throw new FormulaFormatException("Ending token is not ')', a variable, or a number.");

        //4. balanced parenthesis
        if (leftParens != rightParens) throw new FormulaFormatException("Unbalanced Parenthesis.  Ensure all parenthesis are closed.");
    }

    private ArrayList Normalize(ArrayList tokens, Func<string, string> normalize)
    {
        var copy = new ArrayList();
        var length = tokens.Count;

        for(int i = 0; i < length; i++)
        {
            try
            {
                var n = Double.Parse((String)tokens[i]!);
                //number case
                copy.Add(n);
            }
            catch(FormatException)
            {
                //not number
                if(Regex.IsMatch((String)tokens[i]!, @"^[\(\)\+\-\*\/]$")) copy.Add(tokens[i]);
                else copy.Add(normalize((String)tokens[i]!));
            }
        }
        return copy;
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        Stack<String> ops = new Stack<String>();
        Stack<double> values = new Stack<double>();

        int length = tokens.Count;

        foreach (var token in tokens)
        {
            // value or variable case
            if (token.GetType() == typeof(System.Double) || Regex.IsMatch(token.ToString()!, @"^[_a-zA-Z][_a-zA-Z0-9]*$"))
            {
                double num;
                if (token.GetType() == typeof(System.Double))
                    num = (double)token;
                else
                {
                    //lookup if variable
                    try
                    {
                        num = lookup(normalize((String)token));
                    }
                    catch(ArgumentException)
                    {
                        return new FormulaError("Undefined Variable " + token.ToString());
                    }
                }


                String op;
                if (ops.TryPeek(out op!))
                {
                    if (Regex.IsMatch(op, @"^\*$"))
                    {
                        //* is at top of operator stack
                        ops.Pop();
                        values.Push(values.Pop() * num);
                    }
                    else if (Regex.IsMatch(op, @"^\/$"))
                    {
                        //'/' is at top of operator stack
                        if (num == 0) return new FormulaError("Can't divide by 0");
                        ops.Pop();
                        values.Push(values.Pop() / num);
                    }
                    else values.Push(num);
                }
                else values.Push(num);
            }
            //operator case
            else
            {
                switch ((String)token)
                {
                    case "+" or "-":
                        {
                            String op;
                            if (ops.TryPeek(out op!))
                            {
                                if (op.Equals("+"))
                                {
                                    ops.Pop();
                                    values.Push(values.Pop() + values.Pop());
                                }
                                else if (op.Equals("-"))
                                {
                                    ops.Pop();
                                    var num2 = values.Pop();
                                    values.Push(values.Pop() - num2);
                                }
                            }

                            ops.Push((String)token);

                            continue;
                        }
                    case "*" or "/" or "(":
                        {
                            ops.Push((String)token);
                            continue;
                        }
                    case ")":
                        {
                            String op = ops.Peek();

                            //+ or -
                            if (op.Equals("+"))
                            {
                                ops.Pop();
                                values.Push(values.Pop() + values.Pop());
                            }
                            else if (op.Equals("-"))
                            {
                                ops.Pop();
                                var num2 = values.Pop();
                                values.Push(values.Pop() - num2);
                            }

                            ops.Pop();
                            if(ops.Count > 0) op = ops.Peek();

                            //* or /
                            if (op.Equals("*"))
                            {
                                ops.Pop();
                                values.Push(values.Pop() * values.Pop());
                            }
                            else if (op.Equals("/"))
                            {
                                ops.Pop();
                                double num2 = values.Pop();
                                if (num2 == 0) return new FormulaError("Can't divide by 0");
                                values.Push(values.Pop() / num2);
                            }

                            continue;
                        }
                }
            }
        }

        //return result
        if (values.Count == 1) return values.Pop();
        else
        {
            var op = ops.Pop();
            if (op.Equals("+"))
            {
                values.Push(values.Pop() + values.Pop());
            }
            if (op.Equals("-"))
            {
                var num2 = values.Pop();
                values.Push(values.Pop() - num2);
            }

            return values.Pop();
        }
    }

    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        var list = new List<String>();

        foreach(var t in tokens)
        {
            if(t.GetType() == typeof(System.String))
                if(Regex.IsMatch((string)t, @"^[_a-zA-Z]{1,1}[_a-zA-Z0-9]*$"))
                    if(!list.Contains((String)t)) list.Add((String)t);
        }

        return list;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return stringFormula;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() == typeof(Formula))
        {
            return this.ToString().Equals(((Formula)obj).ToString());
        }
        else return false;
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.ToString().Equals(f2.ToString());
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !(f1 == f2);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        return stringFormula.GetHashCode();
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }

    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

