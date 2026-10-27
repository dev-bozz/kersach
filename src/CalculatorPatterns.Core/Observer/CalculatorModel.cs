using CalculatorPatterns.Core.Bridge;
using CalculatorPatterns.Core.Operations;

namespace CalculatorPatterns.Core.Observer;

public sealed record CalculatorChangedEventArgs(string Display, string Expression, string? ErrorMessage, IReadOnlyList<CalculationRecord> History, CalculatorMode Mode, AngleMode AngleMode);
public sealed class CalculatorModel
{
    public event EventHandler<CalculatorChangedEventArgs>? Changed;
    public string Display { get; private set; } = "0";
    public string Expression { get; private set; } = "";
    public string? ErrorMessage { get; private set; }
    public CalculatorMode Mode { get; private set; } = CalculatorMode.Standard;
    public AngleMode AngleMode { get; private set; } = AngleMode.Degrees;
    private readonly List<CalculationRecord> _history = [];
    public IReadOnlyList<CalculationRecord> History => _history.AsReadOnly();
    public void Update(string display, string expression, string? error, CalculatorMode mode, AngleMode angleMode) { Display = display; Expression = expression; ErrorMessage = error; Mode = mode; AngleMode = angleMode; Publish(); }
    public void AddRecord(CalculationRecord record) { _history.Insert(0, record); if (_history.Count > 1000) _history.RemoveAt(_history.Count - 1); Publish(); }
    public void ReplaceHistory(IEnumerable<CalculationRecord> records) { _history.Clear(); _history.AddRange(records.Take(1000)); Publish(); }
    private void Publish() => Changed?.Invoke(this, new(Display, Expression, ErrorMessage, History, Mode, AngleMode));
}
public sealed record CalculationRecord(DateTimeOffset Timestamp, string Expression, string Result);
