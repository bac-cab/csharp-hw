using TaskHub.Models;

namespace TaskHub.Services;

public delegate void OverdueTasksFoundHandler(List<TaskItem> overdueTasks);

public sealed class DeadlineMonitor : IDisposable
{
    private readonly TaskManager _taskManager;
    private readonly CancellationTokenSource _cts = new();
    private readonly TimeSpan _interval;
    private Task? _monitorTask;

    public event OverdueTasksFoundHandler? OverdueTasksFound;

    public DeadlineMonitor(TaskManager taskManager, TimeSpan interval)
    {
        _taskManager = taskManager;
        _interval = interval;
    }

    public void Start()
    {
        if (_monitorTask is not null)
        {
            return;
        }

        _monitorTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var overdue = _taskManager.GetAll()
                        .Where(t => t.IsOverdue)
                        .ToList();

                    if (overdue.Count > 0)
                    {
                        OverdueTasksFound?.Invoke(overdue);
                    }

                    await Task.Delay(_interval, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();

        try
        {
            _monitorTask?.Wait();
        }
        catch (AggregateException)
        {
        }

        _cts.Dispose();
    }
}
