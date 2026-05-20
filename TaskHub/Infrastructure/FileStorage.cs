using System.Text.Json;
using TaskHub.Models;

namespace TaskHub.Infrastructure;

public static class FileStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task SaveAsync(string path, IEnumerable<TaskItem> tasks)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, tasks, JsonOptions);
    }

    public static async Task<List<TaskItem>> LoadAsync(string path)
    {
        if (!File.Exists(path))
        {
            return new List<TaskItem>();
        }

        await using var stream = File.OpenRead(path);
        var tasks = await JsonSerializer.DeserializeAsync<List<TaskItem>>(stream, JsonOptions);
        return tasks ?? new List<TaskItem>();
    }
}
