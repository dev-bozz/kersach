using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CalculatorPatterns.Core.Bridge;
using CalculatorPatterns.Core.Operations;
using CalculatorPatterns.Core.Observer;
using CalculatorPatterns.Core.Persistence;
using CalculatorPatterns.Desktop.ViewModels;
using CalculatorPatterns.Desktop.Views;

namespace CalculatorPatterns.Desktop;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var factory = new OperationFactory();
            var model = new CalculatorModel();
            var controller = new CalculatorController(new StandardEngine(factory));
            var store = new JsonHistoryStore();
            desktop.MainWindow = new MainWindow { DataContext = new CalculatorViewModel(model, controller, factory, store) };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
