# Публикация Windows x64

Для Windows runner в `.github/workflows/windows-release.yml` закреплена .NET 8 установка, restore, Release build, xUnit и self-contained single-file publish для `win-x64`. Avalonia Desktop использует платформенные нативные assets через runtime-specific publish; `IncludeNativeLibrariesForSelfExtract` упаковывает нативные библиотеки для извлечения при запуске.

Локальная команда (Windows или macOS с .NET SDK и доступными NuGet пакетами):

```sh
dotnet publish src/CalculatorPatterns.Desktop/CalculatorPatterns.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts/win-x64
find artifacts/win-x64 -maxdepth 1 -type f -print
```

На Windows скопируйте полученную папку целиком и запустите `CalculatorPatterns.Desktop.exe`. Приложение self-contained, поэтому отдельный .NET runtime на целевой машине не нужен. Windows GUI должен быть проверен на Windows: macOS не запускает PE executable.
