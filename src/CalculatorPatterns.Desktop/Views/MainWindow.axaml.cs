using Avalonia.Controls;
using Avalonia.Input;
using CalculatorPatterns.Core.Observer;
using CalculatorPatterns.Desktop.ViewModels;

namespace CalculatorPatterns.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow() { InitializeComponent(); Opened += (_, _) => (DataContext as CalculatorViewModel)?.Attach(this); }
    private void InitializeComponent() => Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    private void OnButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Tag: string command } && DataContext is CalculatorViewModel vm) vm.Invoke(command);
    }
    private void OnHistorySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: CalculationRecord record } && DataContext is CalculatorViewModel vm)
            vm.SelectHistory(record);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        var key = e.Key switch { Key.Enter => "Enter", Key.Escape => "Escape", Key.Back => "Backspace", Key.Add => "+", Key.Subtract => "-", Key.Multiply => "*", Key.Divide => "/", Key.D0 or Key.NumPad0 => "0", Key.D1 or Key.NumPad1 => "1", Key.D2 or Key.NumPad2 => "2", Key.D3 or Key.NumPad3 => "3", Key.D4 or Key.NumPad4 => "4", Key.D5 or Key.NumPad5 => "5", Key.D6 or Key.NumPad6 => "6", Key.D7 or Key.NumPad7 => "7", Key.D8 or Key.NumPad8 => "8", Key.D9 or Key.NumPad9 => "9", Key.OemPlus => "+", Key.OemMinus => "-", Key.OemPeriod or Key.Decimal => ",", Key.Z => "z", Key.Y => "y", _ => "" };
        if (key.Length > 0 && DataContext is CalculatorViewModel vm) { vm.HandleKey(key, e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta), e.KeyModifiers.HasFlag(KeyModifiers.Shift)); e.Handled = true; }
    }
}
