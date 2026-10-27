using System.Text.Json;
using CalculatorPatterns.Core.Observer;

namespace CalculatorPatterns.Core.Persistence;

public interface IHistoryStore { Task SaveAsync(string path, IReadOnlyCollection<CalculationRecord> records, CancellationToken cancellationToken = default); Task<IReadOnlyList<CalculationRecord>> LoadAsync(string path, CancellationToken cancellationToken = default); }
public sealed class JsonHistoryStore : IHistoryStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public async Task SaveAsync(string path, IReadOnlyCollection<CalculationRecord> records, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(records, Options);
        await File.WriteAllTextAsync(path, json, cancellationToken);
    }
    public async Task<IReadOnlyList<CalculationRecord>> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var records = JsonSerializer.Deserialize<List<CalculationRecord>>(json, Options) ?? throw new JsonException("Файл истории не содержит списка записей.");
        if (records.Count > 10000 || records.Any(r => r is null || r.Timestamp == default || string.IsNullOrWhiteSpace(r.Expression) || string.IsNullOrWhiteSpace(r.Result))) throw new JsonException("Файл истории содержит некорректные записи.");
        return records.AsReadOnly();
    }
}
