# Паттерны

## Command

`ICalculatorCommand` задаёт `Execute`, `DelegateCalculatorCommand` оборачивает конкретную пользовательскую операцию, а `CalculatorInvoker` явно вызывает команду. Все нажатия кнопок и клавиатурные события проходят через ViewModel `Invoke`, создающую команду и передающую её invoker. Command отделяет инициатора действия от его запуска; он не хранит прошлые состояния.

## Factory Method

`IOperationFactory.Create` — фабричный метод, `OperationFactory` выбирает и создаёт конкретный `IOperation`. Это Factory Method в компактной форме: creator предоставляет метод создания продукта, а конкретный creator инкапсулирует сопоставление ключа продукту. Алгоритмы инкапсулированы в `AddOperation`, `DivideOperation`, `SinOperation` и других классах, а не в фабрике.

## Observer

`CalculatorModel.Changed` публикует типизированный `CalculatorChangedEventArgs` после изменений состояния и истории. `CalculatorViewModel` подписывается в конструкторе и обновляет свойства/коллекцию UI. `INotifyPropertyChanged` обеспечивает техническую работу привязок, но осмысленная доменная подписка между Model и ViewModel является отдельной связью Observer.

## Bridge

Абстракция `CalculatorController` хранит заменяемый `ICalculatorEngine` и делегирует ему вычисление. `StandardEngine` и `ScientificEngine` — две независимые реализации, обе используют парсер и фабрику операций, но могут независимо расширяться под разные наборы/политики вычислений. Переключение режима создаёт соответствующий implementor без изменения ViewModel вызовов `Calculate`.

## Memento

`CalculatorOriginator.Save/Restore` создаёт и применяет неизменяемый `CalculatorMemento` (record со значимыми строками, nullable operand и enum). `HistoryCaretaker` хранит ограниченные стеки undo/redo. Новая команда очищает redo. Memento отвечает за снимок, Command — за запуск действия; Undo не подменяет Command и не смешивает состояние интерфейса с результатами вычислений.
