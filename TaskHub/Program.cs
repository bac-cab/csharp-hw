using TaskHub.Models;
using TaskHub.Services;
using StatusEnum = TaskHub.Models.TaskStatus;

var manager = new TaskManager();
var filePath = Path.Combine(AppContext.BaseDirectory, "tasks.json");

using var monitor = new DeadlineMonitor(manager, TimeSpan.FromSeconds(7));
monitor.OverdueTasksFound += overdue =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n[NOTIFY] Overdue tasks:");
    foreach (var task in overdue)
    {
        Console.WriteLine($"- {task.Title} (Deadline: {task.Deadline:g})");
    }
    Console.ResetColor();
};
monitor.Start();

while (true)
{
    PrintMenu();
    Console.Write("Select action: ");
    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                CreateTask(manager);
                break;
            case "2":
                ViewTasksMenu(manager);
                break;
            case "3":
                EditTask(manager);
                break;
            case "4":
                DeleteTask(manager);
                break;
            case "5":
                SearchMenu(manager);
                break;
            case "6":
                PrintStatistics(manager);
                break;
            case "7":
                await manager.SaveAsync(filePath);
                Console.WriteLine($"Saved to {filePath}");
                break;
            case "8":
                await manager.LoadAsync(filePath);
                Console.WriteLine($"Loaded from {filePath}");
                break;
            case "0":
                monitor.Dispose();
                return;
            default:
                Console.WriteLine("Unknown command");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}

static void PrintMenu()
{
    Console.WriteLine("\n=== TaskHub ===");
    Console.WriteLine("1. Create task");
    Console.WriteLine("2. View tasks");
    Console.WriteLine("3. Edit task");
    Console.WriteLine("4. Delete task");
    Console.WriteLine("5. Search tasks");
    Console.WriteLine("6. Statistics");
    Console.WriteLine("7. Save to file (async)");
    Console.WriteLine("8. Load from file (async)");
    Console.WriteLine("0. Exit");
}

static void CreateTask(TaskManager manager)
{
    var task = new TaskItem();

    task.Title = ReadString("Title: ");

    task.Description = ReadString("Description: ");

    task.Priority = ReadEnum<TaskPriority>("Priority (Low, Medium, High): ");

    task.Deadline = DateTime.Parse(ReadString("Deadline (yyyy-MM-dd HH:mm): "));

    task.Status = ReadEnum<StatusEnum>("Status (New, InProgress, Done): ");

    manager.Create(task);
    Console.WriteLine("Task created.");
}

static void ViewTasksMenu(TaskManager manager)
{
    Console.WriteLine("1. All");
    Console.WriteLine("2. Done");
    Console.WriteLine("3. Not Done");
    Console.WriteLine("4. High Priority");
    Console.Write("Select: ");

    var choice = Console.ReadLine();
    List<TaskItem> tasks = choice switch
    {
        "2" => manager.GetDone(),
        "3" => manager.GetNotDone(),
        "4" => manager.GetHighPriority(),
        _ => manager.GetAll().ToList()
    };

    PrintTasks(tasks);
}

static void EditTask(TaskManager manager)
{
    PrintTasks(manager.GetAll());
    Console.Write("Enter task ID: ");

    if (!Guid.TryParse(Console.ReadLine(), out var id))
    {
        throw new FormatException("Invalid GUID format.");
    }

    var task = manager.GetById(id) ?? throw new InvalidOperationException("Task not found.");

    Console.Write($"New title ({task.Title}): ");
    var title = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(title)) task.Title = title;

    Console.Write($"New description ({task.Description}): ");
    var description = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(description)) task.Description = description;

    Console.WriteLine("Change priority? (y/n)");
    if (Console.ReadLine()?.Equals("y", StringComparison.OrdinalIgnoreCase) == true)
    {
        task.Priority = ReadEnum<TaskPriority>("Priority (Low, Medium, High): ");
    }

    Console.WriteLine("Change status? (y/n)");
    if (Console.ReadLine()?.Equals("y", StringComparison.OrdinalIgnoreCase) == true)
    {
        task.Status = ReadEnum<StatusEnum>("Status (New, InProgress, Done): ");
    }

    Console.WriteLine("Task updated.");
}

static void DeleteTask(TaskManager manager)
{
    PrintTasks(manager.GetAll());
    Console.Write("Enter task ID: ");

    if (!Guid.TryParse(Console.ReadLine(), out var id))
    {
        throw new FormatException("Invalid GUID format.");
    }

    Console.WriteLine(manager.Delete(id) ? "Task deleted." : "Task not found.");
}

static void SearchMenu(TaskManager manager)
{
    Console.WriteLine("1. By title");
    Console.WriteLine("2. By status");
    Console.WriteLine("3. By priority");
    Console.Write("Select: ");
    var choice = Console.ReadLine();

    List<TaskItem> result = choice switch
    {
        "1" => manager.SearchByTitle(ReadString("Title contains: ")),
        "2" => manager.SearchByStatus(ReadEnum<StatusEnum>("Status (New, InProgress, Done): ")),
        "3" => manager.SearchByPriority(ReadEnum<TaskPriority>("Priority (Low, Medium, High): ")),
        _ => new List<TaskItem>()
    };

    PrintTasks(result);
}

static void PrintStatistics(TaskManager manager)
{
    var (total, done, overdue) = manager.GetStats();
    var priorityStats = manager.PriorityStats();

    Console.WriteLine($"Total tasks: {total}");
    Console.WriteLine($"Done tasks: {done}");
    Console.WriteLine($"Overdue tasks: {overdue}");
    Console.WriteLine("Priority stats:");
    foreach (var entry in priorityStats)
    {
        Console.WriteLine($"- {entry.Key}: {entry.Value}");
    }
}

static void PrintTasks(IEnumerable<TaskItem> tasks)
{
    var list = tasks.ToList();
    if (list.Count == 0)
    {
        Console.WriteLine("No tasks found.");
        return;
    }

    foreach (var task in list)
    {
        Console.WriteLine($"ID: {task.Id}");
        Console.WriteLine($"Title: {task.Title}");
        Console.WriteLine($"Description: {task.Description}");
        Console.WriteLine($"Priority: {task.Priority}");
        Console.WriteLine($"Deadline: {task.Deadline:g}");
        Console.WriteLine($"Status: {task.Status}");
        Console.WriteLine($"Overdue: {(task.IsOverdue ? "Yes" : "No")}");
        Console.WriteLine(new string('-', 30));
    }
}

static string ReadString(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine() ?? string.Empty;
}

static T ReadEnum<T>(string prompt) where T : struct, Enum
{
    Console.Write(prompt);
    var input = Console.ReadLine();

    if (Enum.TryParse<T>(input, true, out var value))
    {
        return value;
    }

    throw new FormatException($"Invalid value for {typeof(T).Name}.");
}
