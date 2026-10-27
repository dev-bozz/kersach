namespace CalculatorPatterns.Core.Commands;

public interface ICalculatorCommand { string Name { get; } void Execute(); }
public sealed class CalculatorInvoker
{
    public void Execute(ICalculatorCommand command) => command.Execute();
}
public sealed class DelegateCalculatorCommand(string name, Action action) : ICalculatorCommand
{
    public string Name { get; } = name;
    public void Execute() => action();
}
