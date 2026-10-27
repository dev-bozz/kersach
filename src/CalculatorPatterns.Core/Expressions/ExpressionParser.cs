using System.Globalization;
using CalculatorPatterns.Core.Operations;

namespace CalculatorPatterns.Core.Expressions;

public sealed class ExpressionParser(IOperationFactory factory)
{
    private List<string> _tokens = [];
    private int _index;
    private AngleMode _mode;
    public double Evaluate(string expression, AngleMode mode)
    {
        _tokens = Tokenize(expression); _index = 0; _mode = mode;
        var value = ParseExpression();
        if (_index != _tokens.Count) throw new FormatException("Некорректное выражение.");
        return EnsureFinite(value);
    }
    private double ParseExpression()
    {
        var value = ParseTerm();
        while (Match("+") || Match("-")) { var op = Previous; var right = ParseTerm(); value = Apply(op == "+" ? OperationKind.Add : OperationKind.Subtract, value, right); }
        return value;
    }
    private double ParseTerm()
    {
        var value = ParseUnary();
        while (Match("*") || Match("/")) { var op = Previous; var right = ParseUnary(); value = Apply(op == "*" ? OperationKind.Multiply : OperationKind.Divide, value, right); }
        return value;
    }
    private double ParseUnary()
    {
        if (Match("+")) return ParseUnary();
        if (Match("-")) return -ParseUnary();
        return ParsePower();
    }
    private double ParsePower()
    {
        var value = ParsePrimary();
        if (Match("^")) value = Apply(OperationKind.Power, value, ParseUnary());
        return value;
    }
    private double ParsePrimary()
    {
        if (Match("(")) { var nested = ParseExpression(); Expect(")"); return nested; }
        if (Match("π")) return Math.PI;
        if (Match("e")) return Math.E;
        if (_index >= _tokens.Count) throw new FormatException("Ожидалось число.");
        var token = _tokens[_index++];
        if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)) return number;
        if (token is "sqrt" or "sin" or "cos" or "tan" or "ln" or "log10")
        {
            Expect("("); var value = ParseExpression(); Expect(")");
            var kind = token switch { "sqrt" => OperationKind.Sqrt, "sin" => OperationKind.Sin, "cos" => OperationKind.Cos, "tan" => OperationKind.Tan, "ln" => OperationKind.Ln, _ => OperationKind.Log10 };
            return Apply(kind, value);
        }
        throw new FormatException($"Неизвестный элемент: {token}");
    }
    private double Apply(OperationKind kind, params double[] values) => EnsureFinite(factory.Create(kind, _mode).Execute(values));
    private static double EnsureFinite(double value) => double.IsFinite(value) ? value : throw new ArithmeticException("Результат выходит за допустимый диапазон.");
    private bool Match(string token) { if (_index >= _tokens.Count || _tokens[_index] != token) return false; _index++; return true; }
    private string Previous => _tokens[_index - 1];
    private void Expect(string token) { if (!Match(token)) throw new FormatException($"Ожидался символ «{token}»."); }
    private static List<string> Tokenize(string expression)
    {
        var result = new List<string>();
        for (var i = 0; i < expression.Length;)
        {
            if (char.IsWhiteSpace(expression[i])) { i++; continue; }
            if (char.IsDigit(expression[i]) || expression[i] is '.' or ',')
            {
                var start = i++;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] is '.' or ',')) i++;
                if (i < expression.Length && expression[i] is 'e' or 'E')
                {
                    i++;
                    if (i < expression.Length && expression[i] is '+' or '-') i++;
                    var exponentStart = i;
                    while (i < expression.Length && char.IsDigit(expression[i])) i++;
                    if (exponentStart == i) throw new FormatException("Некорректная степень числа.");
                }
                var number = expression[start..i].Replace(',', '.');
                if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out _)) throw new FormatException("Некорректное число.");
                result.Add(number); continue;
            }
            if (char.IsLetter(expression[i])) { var start = i++; while (i < expression.Length && char.IsLetterOrDigit(expression[i])) i++; result.Add(expression[start..i].ToLowerInvariant()); continue; }
            if (expression[i] is '+' or '-' or '*' or '/' or '^' or '(' or ')' or 'π') { result.Add(expression[i++].ToString()); continue; }
            throw new FormatException("Выражение содержит недопустимый символ.");
        }
        return result;
    }
}
