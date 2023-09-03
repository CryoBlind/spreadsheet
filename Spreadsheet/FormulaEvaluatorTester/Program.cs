// See https://aka.ms/new-console-template for more information
using FormulaEvaluator;
using System.Collections;
using System.Runtime.CompilerServices;

Console.WriteLine(Evaluator.Evaluate("1 + 1", GetValue));
Console.WriteLine(Evaluator.Evaluate("1 + G1", GetValue));
Console.WriteLine(Evaluator.Evaluate("1 * 5", GetValue));
try
{
    Console.WriteLine(Evaluator.Evaluate("1 / 0", GetValue));
}
catch(ArgumentException e)
{
    Console.WriteLine(e.Message);
}

try
{
    Console.WriteLine(Evaluator.Evaluate("1 + 3))))", GetValue));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}

try
{
    Console.WriteLine(Evaluator.Evaluate("(1 + 3", GetValue));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}

Console.WriteLine(Evaluator.Evaluate("(1 + 2) * 5", GetValue));
Console.WriteLine(Evaluator.Evaluate("2 * G", GetValue));




//test lookup method
int GetValue(String key)
{
    return 15;
}