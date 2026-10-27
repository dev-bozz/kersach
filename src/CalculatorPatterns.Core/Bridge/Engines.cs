using CalculatorPatterns.Core.Expressions;
using CalculatorPatterns.Core.Operations;

namespace CalculatorPatterns.Core.Bridge;

public enum CalculatorMode { Standard, Scientific }
public interface ICalculatorEngine { CalculatorMode Mode { get; } double Evaluate(string expression, AngleMode angleMode); }
public sealed class StandardEngine(IOperationFactory factory) : ICalculatorEngine
{
    private readonly ExpressionParser _parser = new(factory);
    public CalculatorMode Mode => CalculatorMode.Standard;
    public double Evaluate(string expression, AngleMode angleMode) => _parser.Evaluate(expression, angleMode);
}
public sealed class ScientificEngine(IOperationFactory factory) : ICalculatorEngine
{
    private readonly ExpressionParser _parser = new(factory);
    public CalculatorMode Mode => CalculatorMode.Scientific;
    public double Evaluate(string expression, AngleMode angleMode) => _parser.Evaluate(expression, angleMode);
}
public sealed class CalculatorController(ICalculatorEngine engine)
{
    public ICalculatorEngine Engine { get; private set; } = engine;
    public double Calculate(string expression, AngleMode angleMode) => Engine.Evaluate(expression, angleMode);
    public void SwitchEngine(ICalculatorEngine engine) => Engine = engine;
}
