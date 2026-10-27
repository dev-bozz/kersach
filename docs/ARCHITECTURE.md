# Архитектура

`CalculatorPatterns.Core` не зависит от UI. `ExpressionParser` разбирает арифметические выражения рекурсивным спуском, `OperationFactory` выдаёт отдельные объекты операций, а `CalculatorController` делегирует вычисление выбранному движку. `CalculatorModel` — источник типизированных событий изменения состояния и списка вычислений. ViewModel подписывается на события и реализует `INotifyPropertyChanged` для привязок Avalonia.

`CalculatorPatterns.Desktop` собирает зависимости в `App`, отображает состояние из ViewModel и использует `StorageProvider` Avalonia для файловых диалогов. UI остаётся кроссплатформенным.

История вычислений (`CalculationRecord`) и история редактирования UI — разные данные: первая хранится в JSON, вторая хранится `HistoryCaretaker` в undo/redo стеках. Доменная модель публикует изменения; ViewModel — подписчик.
