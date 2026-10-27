# Калькулятор с паттернами проектирования (вариант 2)

Учебное настольное приложение на C# 12 / .NET 8 / Avalonia UI 11. Решение разделено на доменную библиотеку, кроссплатформенный GUI и xUnit тесты.

## Состав

- `src/CalculatorPatterns.Core` — операции, парсер, Bridge, Factory Method, Observer, Command, Memento и JSON хранилище.
- `src/CalculatorPatterns.Desktop` — интерфейс Avalonia, ViewModel и файловые диалоги платформы.
- `tests/CalculatorPatterns.Tests` — сценарии проверки домена и хранения.
- `docs` — архитектура, паттерны, запуск и фактический отчёт проверок.

## Запуск и проверки

Требуется .NET 8 SDK. На macOS Apple Silicon установите официальный SDK командой `brew install --cask dotnet-sdk` (при работе под обычной пользовательской учётной записью) либо загрузите установщик macOS Arm64 с [официальной страницы .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0).

```sh
dotnet --info
dotnet restore CalculatorPatterns.sln
dotnet run --project src/CalculatorPatterns.Desktop
dotnet build CalculatorPatterns.sln -c Release
dotnet test CalculatorPatterns.sln -c Release
```

Ввод поддерживает десятичную точку и запятую. В выражениях применяются скобки, приоритет арифметики и функции `sqrt`, `sin`, `cos`, `tan`, `ln`, `log10`; для строкового ввода допустимы `π` и `e`. Горячие клавиши: цифры, `+ - * /`, Enter, Escape, Backspace и Ctrl/Cmd+Z/Y.

История автоматически хранится в `%APPDATA%/CalculatorPatterns/history.json` на Windows и в пользовательской папке Application Support на macOS (точный путь задаётся платформой .NET). Команды в нижней части сетки открывают файловые диалоги импорта/экспорта.

## Windows x64

На Windows можно собрать автономный пакет командой:

```powershell
dotnet publish src/CalculatorPatterns.Desktop/CalculatorPatterns.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts/win-x64
```

Перенесите всю папку `artifacts/win-x64` на Windows x64 и запустите `CalculatorPatterns.Desktop.exe`. Self-contained публикация включает .NET runtime. Репозиторий также содержит GitHub Actions workflow, который публикует эту папку как artifact после build/test на Windows runner.

На macOS невозможно запустить Windows `.exe`; локальный publish не подтверждает исполнимость GUI на Windows. Состояние фактических проверок в этой среде записано в [docs/REPORT_FACTS.md](docs/REPORT_FACTS.md).
