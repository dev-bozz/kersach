# Сборка и тестирование

Нужен .NET 8 SDK. Проверка версии: `dotnet --info`. Основной цикл:

```sh
dotnet restore CalculatorPatterns.sln
dotnet build CalculatorPatterns.sln -c Release
dotnet test CalculatorPatterns.sln -c Release
dotnet run --project src/CalculatorPatterns.Desktop
```

Тесты покрывают арифметику и приоритеты parser, математические границы, фабрику, оба Bridge implementor, invoker/command, typed Observer event, Memento undo/redo/capacity и JSON round-trip/валидацию. GUI проверяется запуском приложения в интерактивном macOS desktop session; успешный compile сам по себе не является GUI-тестом.
