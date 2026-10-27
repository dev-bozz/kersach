using CalculatorPatterns.Core.Bridge;
using CalculatorPatterns.Core.Commands;
using CalculatorPatterns.Core.Expressions;
using CalculatorPatterns.Core.Observer;
using CalculatorPatterns.Core.Operations;
using CalculatorPatterns.Core.Persistence;
using CalculatorPatterns.Core.State;
using System.Text.Json;

namespace CalculatorPatterns.Tests;

public sealed class CoreTests
{
    private readonly OperationFactory _factory = new();
    private readonly ExpressionParser Parser => new(_factory);
    [Theory] [InlineData("2+3", 5)] [InlineData("8/2", 4)] [InlineData("2^10", 1024)] [InlineData("sqrt(81)", 9)] [InlineData("cos(0)", 1)] [InlineData("1/4", .25)] [InlineData("2+3*4", 14)] [InlineData("(2+3)*4", 20)] [InlineData("-2^2", -4)] [InlineData("(-2)^2", 4)] [InlineData("ln(e)", 1)] [InlineData("log10(1000)", 3)] public void ParserCalculates(string expression, double expected) => Assert.Equal(expected, Parser.Evaluate(expression, AngleMode.Degrees), 10);
    [Fact] public void SinThirtyDegreesIsHalf() => Assert.Equal(.5, Parser.Evaluate("sin(30)", AngleMode.Degrees), 10);
    [Fact] public void SinPiOverSixRadiansIsHalf() => Assert.Equal(.5, Parser.Evaluate("sin(0.5235987755982988)", AngleMode.Radians), 10);
    [Fact] public void FactoryCreatesDedicatedOperation() => Assert.IsType<DivideOperation>(_factory.Create(OperationKind.Divide));
    [Fact] public void DivideByZeroThrows() => Assert.Throws<DivideByZeroException>(() => Parser.Evaluate("5/0", AngleMode.Degrees));
    [Fact] public void NegativeSqrtThrows() => Assert.Throws<ArithmeticException>(() => Parser.Evaluate("sqrt(-1)", AngleMode.Degrees));
    [Theory] [InlineData("ln(0)")] [InlineData("log10(-1)")] public void InvalidLogThrows(string expression) => Assert.Throws<ArithmeticException>(() => Parser.Evaluate(expression, AngleMode.Degrees));
    [Fact] public void TangentAtNinetyDegreesThrows() => Assert.Throws<ArithmeticException>(() => _factory.Create(OperationKind.Tan).Execute(90));
    [Fact] public void OverflowThrows() => Assert.Throws<ArithmeticException>(() => Parser.Evaluate("1e308*1e308", AngleMode.Degrees));
    [Fact] public void MalformedExpressionThrows() => Assert.Throws<FormatException>(() => Parser.Evaluate("2+)", AngleMode.Degrees));
    [Fact] public void InvalidCharactersRejected() => Assert.Throws<FormatException>(() => Parser.Evaluate("2;3", AngleMode.Degrees));
    [Fact] public void StandardBridgeEngineEvaluates() => Assert.Equal(5, new CalculatorController(new StandardEngine(_factory)).Calculate("2+3", AngleMode.Degrees));
    [Fact] public void ScientificBridgeEngineEvaluates() => Assert.Equal(9, new CalculatorController(new ScientificEngine(_factory)).Calculate("sqrt(81)", AngleMode.Degrees));
    [Fact] public void ControllerCanSwitchEngine() { var controller = new CalculatorController(new StandardEngine(_factory)); controller.SwitchEngine(new ScientificEngine(_factory)); Assert.Equal(CalculatorMode.Scientific, controller.Engine.Mode); }
    [Fact] public void CommandInvokerExecutesCommand() { var called = false; new CalculatorInvoker().Execute(new DelegateCalculatorCommand("test", () => called = true)); Assert.True(called); }
    [Fact] public void CommandCarriesName() => Assert.Equal("clear", new DelegateCalculatorCommand("clear", () => { }).Name);
    [Fact] public void ModelPublishesTypedChangeEvent() { var model = new CalculatorModel(); CalculatorChangedEventArgs? received = null; model.Changed += (_, args) => received = args; model.Update("5", "2+3", null, CalculatorMode.Standard, AngleMode.Degrees); Assert.Equal("5", received?.Display); }
    [Fact] public void ModelPublishesHistoryChanges() { var model = new CalculatorModel(); model.AddRecord(new(DateTimeOffset.Now, "2+3", "5")); Assert.Single(model.History); }
    [Fact] public void MementoRestoresFields() { var origin = new CalculatorOriginator { Display = "42", Input = "42", AngleMode = AngleMode.Radians }; var m = origin.Save(); origin.Display = "0"; origin.Restore(m); Assert.Equal("42", origin.Display); Assert.Equal(AngleMode.Radians, origin.AngleMode); }
    [Fact] public void UndoRedoRoundTrip() { var caretaker = new HistoryCaretaker(); var first = new CalculatorMemento("1", "1", "", null, null, true, CalculatorMode.Standard, AngleMode.Degrees, null); var second = first with { Display = "2" }; caretaker.Capture(first); Assert.Equal(first, caretaker.Undo(second)); Assert.Equal(second, caretaker.Redo(first)); }
    [Fact] public void NewStateClearsRedo() { var caretaker = new HistoryCaretaker(); var state = new CalculatorOriginator().Save(); caretaker.Capture(state); caretaker.Undo(state); caretaker.Capture(state with { Display = "x" }); Assert.False(caretaker.CanRedo); }
    [Fact] public void CaretakerHonorsCapacity() { var caretaker = new HistoryCaretaker(30); for (var i = 0; i < 40; i++) caretaker.Capture(new CalculatorOriginator { Display = i.ToString() }.Save()); Assert.Equal(30, caretaker.UndoCount); }
    [Fact] public async Task JsonHistoryRoundTrips()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"); var store = new JsonHistoryStore(); var rows = new[] { new CalculationRecord(DateTimeOffset.UtcNow, "2+3 =", "5") };
        await store.SaveAsync(path, rows); var loaded = await store.LoadAsync(path); Assert.Equal(rows[0].Expression, loaded[0].Expression); File.Delete(path);
    }
    [Fact] public async Task InvalidJsonFailsClearly() { var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"); await File.WriteAllTextAsync(path, "{"); await Assert.ThrowsAsync<JsonException>(() => new JsonHistoryStore().LoadAsync(path)); File.Delete(path); }
    [Fact] public async Task InvalidHistoryRowsRejected() { var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"); await File.WriteAllTextAsync(path, "[{}]"); await Assert.ThrowsAsync<JsonException>(() => new JsonHistoryStore().LoadAsync(path)); File.Delete(path); }
}
