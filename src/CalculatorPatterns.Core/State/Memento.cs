using CalculatorPatterns.Core.Bridge;
using CalculatorPatterns.Core.Operations;

namespace CalculatorPatterns.Core.State;

public sealed record CalculatorMemento(string Display, string Input, string Expression, string? PendingOperator, double? LeftOperand,
    bool IsNewEntry, CalculatorMode CalculatorMode, AngleMode AngleMode, string? ErrorMessage);
public sealed class CalculatorOriginator
{
    public string Display { get; set; } = "0";
    public string Input { get; set; } = "0";
    public string Expression { get; set; } = "";
    public string? PendingOperator { get; set; }
    public double? LeftOperand { get; set; }
    public bool IsNewEntry { get; set; } = true;
    public CalculatorMode CalculatorMode { get; set; } = CalculatorMode.Standard;
    public AngleMode AngleMode { get; set; } = AngleMode.Degrees;
    public string? ErrorMessage { get; set; }
    public CalculatorMemento Save() => new(Display, Input, Expression, PendingOperator, LeftOperand, IsNewEntry, CalculatorMode, AngleMode, ErrorMessage);
    public void Restore(CalculatorMemento state) { Display = state.Display; Input = state.Input; Expression = state.Expression; PendingOperator = state.PendingOperator; LeftOperand = state.LeftOperand; IsNewEntry = state.IsNewEntry; CalculatorMode = state.CalculatorMode; AngleMode = state.AngleMode; ErrorMessage = state.ErrorMessage; }
}
public sealed class HistoryCaretaker(int capacity = 100)
{
    private readonly Stack<CalculatorMemento> _undo = new();
    private readonly Stack<CalculatorMemento> _redo = new();
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public int UndoCount => _undo.Count;
    public void Capture(CalculatorMemento state) { _undo.Push(state); if (_undo.Count > capacity) { var keep = _undo.Reverse().Take(capacity).ToArray(); _undo.Clear(); foreach (var item in keep) _undo.Push(item); } _redo.Clear(); }
    public CalculatorMemento? Undo(CalculatorMemento current) { if (_undo.Count == 0) return null; _redo.Push(current); return _undo.Pop(); }
    public CalculatorMemento? Redo(CalculatorMemento current) { if (_redo.Count == 0) return null; _undo.Push(current); return _redo.Pop(); }
}
