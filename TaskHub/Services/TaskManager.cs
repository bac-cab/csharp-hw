using TaskHub.Infrastructure;
using TaskHub.Models;

namespace TaskHub.Services;

public class TaskManager
{
    private readonly InMemoryRepository<TaskItem> _repository = new();

    public IReadOnlyList<TaskItem> GetAll() => _repository.GetAll();
    public List<TaskItem> GetDone() => _repository.FindAll(t => t.Status == TaskHub.Models.TaskStatus.Done);
    public List<TaskItem> GetNotDone() => _repository.FindAll(t => t.Status != TaskHub.Models.TaskStatus.Done);
    public List<TaskItem> GetHighPriority() => _repository.FindAll(t => t.Priority == TaskPriority.High);

    public void Create(TaskItem task) => _repository.Add(task);

    public bool Delete(Guid id) => _repository.Remove(t => t.Id == id);

    public TaskItem? GetById(Guid id) => _repository.Find(t => t.Id == id);

    public List<TaskItem> SearchByTitle(string title) =>
        _repository.FindAll(t => t.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

    public List<TaskItem> SearchByStatus(TaskHub.Models.TaskStatus status) =>
        _repository.FindAll(t => t.Status == status);

    public List<TaskItem> SearchByPriority(TaskPriority priority) =>
        _repository.FindAll(t => t.Priority == priority);

    public Dictionary<TaskPriority, int> PriorityStats()
    {
        var result = Enum.GetValues<TaskPriority>().ToDictionary(p => p, _ => 0);
        foreach (var task in _repository.GetAll())
        {
            result[task.Priority]++;
        }

        return result;
    }

    public (int total, int done, int overdue) GetStats()
    {
        var tasks = _repository.GetAll();
        var total = tasks.Count;
        var done = tasks.Count(t => t.Status == TaskHub.Models.TaskStatus.Done);
        var overdue = tasks.Count(t => t.IsOverdue);
        return (total, done, overdue);
    }

    public async Task SaveAsync(string path)
    {
        await FileStorage.SaveAsync(path, _repository.GetAll());
    }

    public async Task LoadAsync(string path)
    {
        var loaded = await FileStorage.LoadAsync(path);
        _repository.ReplaceAll(loaded);
    }
}
