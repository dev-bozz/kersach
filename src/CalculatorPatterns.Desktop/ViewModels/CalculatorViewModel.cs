using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CalculatorPatterns.Core.Bridge;
using CalculatorPatterns.Core.Commands;
using CalculatorPatterns.Core.Observer;
using CalculatorPatterns.Core.Operations;
using CalculatorPatterns.Core.Persistence;
using CalculatorPatterns.Core.State;

namespace CalculatorPatterns.Desktop.ViewModels;

public sealed class CalculatorViewModel : INotifyPropertyChanged
{
    private readonly CalculatorModel _model;
    private readonly CalculatorController _controller;
    private readonly IOperationFactory _factory;
    private readonly IHistoryStore _store;
    private readonly CalculatorOriginator _state = new();
    private readonly HistoryCaretaker _caretaker = new(100);
    private readonly CalculatorInvoker _invoker = new();
    private string _display = "0", _expression = "", _error = "", _modeLabel = "Стандартный", _angleLabel = "DEG";
    private bool _hasError;
    private TopLevel? _topLevel;
    private string? _lastOperator;
    private double? _lastRight;
    private string _input = "0";
    public event PropertyChangedEventHandler? PropertyChanged;
    public string Display { get => _display; private set => Set(ref _display, value); }
    public string Expression { get => _expression; private set => Set(ref _expression, value); }
    public string Error { get => _error; private set => Set(ref _error, value); }
    public bool HasError { get => _hasError; private set => Set(ref _hasError, value); }
    public string ModeLabel { get => _modeLabel; private set => Set(ref _modeLabel, value); }
    public string AngleLabel { get => _angleLabel; private set => Set(ref _angleLabel, value); }
    public ObservableCollection<CalculationRecord> History { get; } = [];
    public IReadOnlyList<string> ScientificFunctions => ["sin", "cos", "tan", "ln", "log10", "√x", "x²", "xʸ", "1/x", "(", ")", "π", "e"];
    public IReadOnlyList<string> KeypadRows => ["CE", "C", "⌫", "÷", "7", "8", "9", "×", "4", "5", "6", "−", "1", "2", "3", "+", "±", "0", ",", "="];
    public bool ShowScientific => _state.CalculatorMode == CalculatorMode.Scientific;
    public string AngleButtonLabel => _state.AngleMode == AngleMode.Degrees ? "DEG  ▾" : "RAD  ▾";
    public CalculatorViewModel(CalculatorModel model, CalculatorController controller, IOperationFactory factory, IHistoryStore store)
    {
        _model = model; _controller = controller; _factory = factory; _store = store;
        _model.Changed += OnModelChanged;
        var path = DefaultHistoryPath;
        if (File.Exists(path)) _ = LoadStartupAsync(path);
    }
    public void Attach(TopLevel topLevel) => _topLevel = topLevel;
    private static string DefaultHistoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CalculatorPatterns", "history.json");
    private async Task LoadStartupAsync(string path) { try { _model.ReplaceHistory(await _store.LoadAsync(path)); } catch (Exception ex) { SetError("Не удалось восстановить историю: " + ex.Message); } }
    public void Invoke(string text) => _invoker.Execute(new DelegateCalculatorCommand(text, () => ExecuteAction(text)));
    private void ExecuteAction(string text)
    {
        // Undo/Redo must not be recorded as new edits: Capture clears the redo stack.
        if (text is not ("Undo" or "Redo")) Capture();
        SetError("");
        try
        {
            if (text.Length == 1 && char.IsDigit(text[0])) { Digit(text); return; }
            switch (text)
            {
                case ",": case ".": Decimal(); break;
                case "+": case "−": case "-": case "×": case "*": case "÷": case "/": case "xʸ": CaseBinary(text); break;
                case "=": EqualsAction(); break;
                case "C": Clear(); break;
                case "CE": _input = "0"; _state.Display = "0"; _state.IsNewEntry = true; Publish(); break;
                case "⌫": Backspace(); break;
                case "±": _input = _input.StartsWith('-') ? _input[1..] : _input == "0" ? "0" : "-" + _input; SyncDisplay(); break;
                case "%": ApplyPercent(); break;
                case "DEG/RAD": case "DEG  ▾": case "RAD  ▾": _state.AngleMode = _state.AngleMode == AngleMode.Degrees ? AngleMode.Radians : AngleMode.Degrees; PropertyChanged?.Invoke(this, new(nameof(AngleButtonLabel))); Publish(); break;
                case "Стандартный/Научный": case "Режим": SwitchMode(); break;
                case "Undo": Undo(); break; case "Redo": Redo(); break;
                case "Сохранить историю": _ = SaveHistoryAsync(); break; case "Загрузить историю": _ = LoadHistoryAsync(); break;
                case "Очистить историю": _model.ReplaceHistory([]); _ = SaveDefaultAsync(); break;
                case "(": case ")": case "π": case "e": ExpressionInput(text); break;
                default: ApplyUnary(text); break;
            }
        }
        catch (Exception ex) { SetError(ex.Message); }
        _state.ErrorMessage = HasError ? Error : null; Publish();
    }
    private void Digit(string digit) { if (_state.IsNewEntry || _input == "0") _input = digit; else if (_input.Length < 24) _input += digit; _state.IsNewEntry = false; SyncDisplay(); }
    private void Decimal() { if (_state.IsNewEntry) { _input = "0"; _state.IsNewEntry = false; } if (!_input.Contains('.') && !_input.Contains(',')) _input += "."; SyncDisplay(); }
    private void Backspace() { if (_state.IsNewEntry) return; _input = _input.Length <= 1 || _input.Length == 2 && _input[0] == '-' ? "0" : _input[..^1]; SyncDisplay(); }
    private void CaseBinary(string op)
    {
        if (_state.PendingOperator is not null && !_state.IsNewEntry) EqualsAction(false);
        _state.LeftOperand = ParseInput(); _state.PendingOperator = op == "xʸ" ? "^" : op switch { "−" => "-", "×" => "*", "÷" => "/", _ => op };
        _state.Expression = _input + " " + _state.PendingOperator; _state.IsNewEntry = true; Publish();
    }
    private void EqualsAction(bool record = true)
    {
        if (_state.PendingOperator is null)
        {
            if (_lastOperator is null) { EvaluateExpression(record); return; }
            _state.PendingOperator = _lastOperator; _state.LeftOperand = ParseInput(); _input = Format(_lastRight ?? 0);
        }
        var right = ParseInput(); var left = _state.LeftOperand ?? 0; var operation = _state.PendingOperator;
        var expressionText = $"{Format(left)} {operation} {Format(right)} =";
        var result = _factory.Create(operation switch { "+" => OperationKind.Add, "-" => OperationKind.Subtract, "*" => OperationKind.Multiply, "/" => OperationKind.Divide, "^" => OperationKind.Power, _ => throw new FormatException("Неизвестная операция") }, _state.AngleMode).Execute(left, right);
        if (!double.IsFinite(result)) throw new ArithmeticException("Результат выходит за допустимый диапазон.");
        _lastOperator = operation; _lastRight = right; _input = Format(result); _state.Display = _input; _state.Expression = expressionText; _state.PendingOperator = null; _state.LeftOperand = null; _state.IsNewEntry = true;
        if (record) { _model.AddRecord(new(DateTimeOffset.Now, _state.Expression, _input)); _ = SaveDefaultAsync(); }
        Publish();
    }
    private void EvaluateExpression(bool record)
    {
        if (!_state.Expression.Contains('(') && !_state.Expression.Any(c => "+-*/^".Contains(c))) return;
        var expression = _state.Expression;
        var result = _controller.Calculate(expression, _state.AngleMode);
        _input = Format(result); _state.Display = _input; _state.Expression = expression + " ="; _state.IsNewEntry = true;
        if (record) { _model.AddRecord(new(DateTimeOffset.Now, _state.Expression, _input)); _ = SaveDefaultAsync(); }
    }
    private void ApplyUnary(string key)
    {
        var kind = key switch { "x²" => OperationKind.Square, "√x" => OperationKind.Sqrt, "1/x" => OperationKind.Reciprocal, "sin" => OperationKind.Sin, "cos" => OperationKind.Cos, "tan" => OperationKind.Tan, "ln" => OperationKind.Ln, "log10" => OperationKind.Log10, _ => throw new InvalidOperationException("Неизвестная команда.") };
        var value = _factory.Create(kind, _state.AngleMode).Execute(ParseInput());
        if (!double.IsFinite(value)) throw new ArithmeticException("Результат выходит за допустимый диапазон.");
        var previous = _input; _input = Format(value); _state.Display = _input; _state.Expression = $"{key}({previous}) ="; _state.IsNewEntry = true;
        _model.AddRecord(new(DateTimeOffset.Now, _state.Expression, _input)); _ = SaveDefaultAsync(); Publish();
    }
    private void ApplyPercent() { var value = ParseInput() / 100d; _input = Format(value); SyncDisplay(); }
    private void ExpressionInput(string text) { _state.Expression += text; _input = text is "π" or "e" ? Format(text == "π" ? Math.PI : Math.E) : _input; _state.IsNewEntry = true; Publish(); }
    private void Clear() { _input = "0"; _state.Display = "0"; _state.Expression = ""; _state.PendingOperator = null; _state.LeftOperand = null; _state.IsNewEntry = true; _lastOperator = null; _lastRight = null; Publish(); }
    private void SwitchMode() { _state.CalculatorMode = _state.CalculatorMode == CalculatorMode.Standard ? CalculatorMode.Scientific : CalculatorMode.Standard; _controller.SwitchEngine(_state.CalculatorMode == CalculatorMode.Standard ? new StandardEngine(_factory) : new ScientificEngine(_factory)); PropertyChanged?.Invoke(this, new(nameof(ShowScientific))); Publish(); }
    private void Undo() { var memento = _caretaker.Undo(_state.Save()); if (memento is not null) { _state.Restore(memento); _input = _state.Input; RestoreEngine(); Publish(); } }
    private void Redo() { var memento = _caretaker.Redo(_state.Save()); if (memento is not null) { _state.Restore(memento); _input = _state.Input; RestoreEngine(); Publish(); } }
    private void RestoreEngine() => _controller.SwitchEngine(_state.CalculatorMode == CalculatorMode.Standard ? new StandardEngine(_factory) : new ScientificEngine(_factory));
    private void Capture() { _state.Input = _input; _state.Display = Display; _caretaker.Capture(_state.Save()); }
    private double ParseInput() => double.Parse(_input.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture);
    private void SyncDisplay() { _state.Input = _input; _state.Display = _input; Display = _input.Replace('.', ','); Publish(); }
    private static string Format(double value) => value.ToString("G15", CultureInfo.InvariantCulture);
    private void Publish() { _state.Input = _input; _state.Display = _input; _model.Update(_state.Display, _state.Expression, HasError ? Error : null, _state.CalculatorMode, _state.AngleMode); }
    private void SetError(string message) { Error = message; HasError = message.Length > 0; }
    private void OnModelChanged(object? sender, CalculatorChangedEventArgs args)
    {
        Display = args.Display.Replace('.', ','); Expression = args.Expression;
        Error = args.ErrorMessage ?? ""; HasError = Error.Length > 0;
        ModeLabel = args.Mode == CalculatorMode.Standard ? "Стандартный" : "Научный";
        AngleLabel = args.AngleMode == AngleMode.Degrees ? "DEG" : "RAD";
        PropertyChanged?.Invoke(this, new(nameof(AngleButtonLabel)));
        if (!History.SequenceEqual(args.History))
        {
            History.Clear();
            foreach (var record in args.History) History.Add(record);
        }
    }
    public void SelectHistory(CalculationRecord? record) { if (record is null) return; Capture(); _state.Expression = record.Expression.Replace(" =", ""); _input = record.Result; _state.Display = _input; _state.IsNewEntry = true; Publish(); }
    public void HandleKey(string key, bool control = false, bool shift = false)
    {
        if (control && key.Equals("z", StringComparison.OrdinalIgnoreCase)) { Invoke(shift ? "Redo" : "Undo"); return; }
        if (control && key.Equals("y", StringComparison.OrdinalIgnoreCase)) { Invoke("Redo"); return; }
        if (key.Length == 1 && char.IsDigit(key[0])) Invoke(key); else if (key is "." or ",") Invoke(","); else if (key is "+" or "-") Invoke(key == "-" ? "−" : key); else if (key is "*" or "/") Invoke(key == "*" ? "×" : "÷"); else if (key is "Enter" or "=") Invoke("="); else if (key == "Escape") Invoke("C"); else if (key == "Backspace") Invoke("⌫");
    }
    private async Task SaveHistoryAsync()
    {
        if (_topLevel?.StorageProvider is not { } storage) return;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions { Title = "Сохранить историю", SuggestedFileName = "calculator-history.json", FileTypeChoices = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }] });
        if (file is not null) try { await _store.SaveAsync(file.Path.LocalPath, _model.History); SetError(""); } catch (Exception ex) { SetError("Не удалось сохранить историю: " + ex.Message); }
    }
    private async Task LoadHistoryAsync()
    {
        if (_topLevel?.StorageProvider is not { } storage) return;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Загрузить историю", AllowMultiple = false, FileTypeFilter = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }] });
        if (files.Count == 0) return;
        try { _model.ReplaceHistory(await _store.LoadAsync(files[0].Path.LocalPath)); await SaveDefaultAsync(); SetError(""); } catch (Exception ex) { SetError("Не удалось загрузить историю: " + ex.Message); }
    }
    private async Task SaveDefaultAsync() { try { Directory.CreateDirectory(Path.GetDirectoryName(DefaultHistoryPath)!); await _store.SaveAsync(DefaultHistoryPath, _model.History); } catch (Exception ex) { SetError("Не удалось сохранить историю: " + ex.Message); } }
    private void Set<T>(ref T field, T value, [CallerMemberName] string? property = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return; field = value; PropertyChanged?.Invoke(this, new(property)); }
}
