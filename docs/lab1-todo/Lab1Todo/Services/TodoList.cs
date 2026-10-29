
using System.Text.Json;
using Lab1Todo.Models;

namespace Lab1Todo.Services;

public enum TodoFilter
{
    All,
    Active,
    Completed
}

public class TodoList
{
    private readonly List<TodoItem> _items = new();

    public IReadOnlyList<TodoItem> Items => _items.AsReadOnly();

    public int Count => _items.Count;

    public void Add(string title, DateTime? dueDate = null)
    {
        _items.Add(new TodoItem(title, dueDate));
    }

    public void Remove(int index)
    {
        CheckIndex(index);
        _items.RemoveAt(index);
    }

    public void Rename(int index, string newTitle)
    {
        CheckIndex(index);
        _items[index].Rename(newTitle);
    }

    public void ToggleDone(int index)
    {
        CheckIndex(index);
        _items[index].ToggleDone();
    }

    public void SetDueDate(int index, DateTime? dueDate)
    {
        CheckIndex(index);
        _items[index].SetDueDate(dueDate);
    }

    public TodoItem GetItem(int index)
    {
        CheckIndex(index);
        return _items[index];
    }

    public IEnumerable<TodoItem> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _items;

        return _items.Where(item =>
            item.Title.Contains(
                query.Trim(),
                StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<TodoItem> Filter(TodoFilter filter)
    {
        return filter switch
        {
            TodoFilter.Active => _items.Where(item => !item.IsDone),
            TodoFilter.Completed => _items.Where(item => item.IsDone),
            _ => _items
        };
    }

    public IEnumerable<TodoItem> SortByDueDate()
    {
        return _items
            .OrderBy(item => item.DueDate is null)
            .ThenBy(item => item.DueDate);
    }

    public void SaveToFile(string filePath)
    {
        var data = _items.Select(item => new TodoData
        {
            Title = item.Title,
            IsDone = item.IsDone,
            DueDate = item.DueDate
        }).ToList();

        string json = JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(filePath, json);
    }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        string json = File.ReadAllText(filePath);

        var data = JsonSerializer.Deserialize<List<TodoData>>(json)
                   ?? new List<TodoData>();

        _items.Clear();

        foreach (var item in data)
        {
            var task = new TodoItem(item.Title, item.DueDate);

            if (item.IsDone)
                task.Complete();

            _items.Add(task);
        }
    }

    private void CheckIndex(int index)
    {
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                "Задача с таким номером не существует.");
        }
    }

    private class TodoData
    {
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
