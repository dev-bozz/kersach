namespace CalculatorPatterns.Core.Operations;

public enum OperationKind { Add, Subtract, Multiply, Divide, Power, Square, Sqrt, Reciprocal, Sin, Cos, Tan, Ln, Log10 }
public enum AngleMode { Degrees, Radians }
public interface IOperation { double Execute(params double[] operands); }
public interface IBinaryOperation : IOperation { }
public interface IUnaryOperation : IOperation { }

public abstract class BinaryOperation : IBinaryOperation
{
    public double Execute(params double[] operands) => operands.Length == 2 ? Calculate(operands[0], operands[1]) : throw new ArgumentException("Операция требует два аргумента.");
    protected abstract double Calculate(double left, double right);
}
public abstract class UnaryOperation : IUnaryOperation
{
    public double Execute(params double[] operands) => operands.Length == 1 ? Calculate(operands[0]) : throw new ArgumentException("Операция требует один аргумент.");
    protected abstract double Calculate(double value);
}
public sealed class AddOperation : BinaryOperation { protected override double Calculate(double left, double right) => left + right; }
public sealed class SubtractOperation : BinaryOperation { protected override double Calculate(double left, double right) => left - right; }
public sealed class MultiplyOperation : BinaryOperation { protected override double Calculate(double left, double right) => left * right; }
public sealed class DivideOperation : BinaryOperation { protected override double Calculate(double left, double right) => right == 0 ? throw new DivideByZeroException("Деление на ноль невозможно.") : left / right; }
public sealed class PowerOperation : BinaryOperation { protected override double Calculate(double left, double right) => Math.Pow(left, right); }
public sealed class SquareOperation : UnaryOperation { protected override double Calculate(double value) => value * value; }
public sealed class SqrtOperation : UnaryOperation { protected override double Calculate(double value) => value < 0 ? throw new ArithmeticException("Корень из отрицательного числа не определен в действительных числах.") : Math.Sqrt(value); }
public sealed class ReciprocalOperation : UnaryOperation { protected override double Calculate(double value) => value == 0 ? throw new DivideByZeroException("Обратное число для нуля не определено.") : 1d / value; }
public abstract class TrigonometricOperation(AngleMode mode) : UnaryOperation
{
    protected double ToRadians(double value) => mode == AngleMode.Degrees ? value * Math.PI / 180d : value;
}
public sealed class SinOperation(AngleMode mode) : TrigonometricOperation(mode) { protected override double Calculate(double value) => Math.Sin(ToRadians(value)); }
public sealed class CosOperation(AngleMode mode) : TrigonometricOperation(mode) { protected override double Calculate(double value) => Math.Cos(ToRadians(value)); }
public sealed class TanOperation(AngleMode mode) : TrigonometricOperation(mode)
{
    protected override double Calculate(double value)
    {
        var radians = ToRadians(value);
        if (Math.Abs(Math.Cos(radians)) < 1e-12) throw new ArithmeticException("Тангенс не определен для этого угла.");
        return Math.Tan(radians);
    }
}
public sealed class LnOperation : UnaryOperation { protected override double Calculate(double value) => value <= 0 ? throw new ArithmeticException("ln определен только для положительных чисел.") : Math.Log(value); }
public sealed class Log10Operation : UnaryOperation { protected override double Calculate(double value) => value <= 0 ? throw new ArithmeticException("log10 определен только для положительных чисел.") : Math.Log10(value); }

public interface IOperationFactory { IOperation Create(OperationKind kind, AngleMode angleMode = AngleMode.Degrees); }
public sealed class OperationFactory : IOperationFactory
{
    public IOperation Create(OperationKind kind, AngleMode angleMode = AngleMode.Degrees) => kind switch
    {
        OperationKind.Add => new AddOperation(), OperationKind.Subtract => new SubtractOperation(), OperationKind.Multiply => new MultiplyOperation(),
        OperationKind.Divide => new DivideOperation(), OperationKind.Power => new PowerOperation(), OperationKind.Square => new SquareOperation(),
        OperationKind.Sqrt => new SqrtOperation(), OperationKind.Reciprocal => new ReciprocalOperation(), OperationKind.Sin => new SinOperation(angleMode),
        OperationKind.Cos => new CosOperation(angleMode), OperationKind.Tan => new TanOperation(angleMode), OperationKind.Ln => new LnOperation(),
        OperationKind.Log10 => new Log10Operation(), _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
