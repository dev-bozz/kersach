# Фактический отчёт о реализации

## Реализация

- Solution: `CalculatorPatterns.sln`; домен: `src/CalculatorPatterns.Core`; Avalonia desktop: `src/CalculatorPatterns.Desktop`; xUnit: `tests/CalculatorPatterns.Tests`.
- Вычисление выражений и математические алгоритмы: `Expressions/ExpressionParser.cs`, `Operations/Operations.cs`.
- Bridge: `Bridge/Engines.cs`; Observer: `Observer/CalculatorModel.cs`; Command: `Commands/Commands.cs`; Memento: `State/Memento.cs`; JSON история: `Persistence/HistoryStore.cs`.
- UI композиция и экран: `Desktop/App.axaml.cs`, `Desktop/ViewModels/CalculatorViewModel.cs`, `Desktop/Views/MainWindow.axaml`.
- Автоматизированная Windows публикация: `.github/workflows/windows-release.yml`.

## Проверки в исходной среде

Дата запуска работ: 2026-10-27. В доступной среде `dotnet --info` завершился сообщением `dotnet: command not found`. `brew install dotnet@8` не выполнен: Homebrew отказался работать из-под root. Поэтому restore/build/test, GUI запуск и Windows publish **не запускались**, количество реально пройденных тестов — **не установлено**. Папка `artifacts/win-x64` и exe локально не созданы. Не считать workflow успешным, пока его фактический Windows run не завершён.

Исходная папка не является Git-репозиторием (`git status` сообщил `not a git repository`), поэтому commit hashes отсутствуют. Ручная сверка исходников выявила незавершённую интеграцию выраженческого ввода: кнопки скобок и констант не собирают нажатия цифр в общую строку выражения. Expression mode и его UI сценарии нельзя считать подтверждёнными до исправления и интеграционной проверки.

Установка на обычной macOS учётной записи: `brew install --cask dotnet-sdk`; альтернативно установить macOS Arm64 SDK с https://dotnet.microsoft.com/download/dotnet/8.0. Затем выполнить команды из `docs/TESTING.md` и `docs/WINDOWS_RELEASE.md`.

## Скриншоты для курсовой

Добавить реальные скриншоты из запущенного приложения после установки SDK и GUI проверки:

- `[ВСТАВИТЬ СКРИНШОТ: основной калькулятор, стандартный режим, DEG]`
- `[ВСТАВИТЬ СКРИНШОТ: научный режим и вычисление sin(30)]`
- `[ВСТАВИТЬ СКРИНШОТ: история вычислений после перезапуска]`
- `[ВСТАВИТЬ СКРИНШОТ: сообщение об ошибке при делении на ноль]`
