
namespace Lab1Todo.Models;

public class TodoItem
{
    private string _title = string.Empty;
    private bool _isDone;
    private DateTime? _dueDate;

    public TodoItem(string title, DateTime? dueDate = null)
    {
        Rename(title);
        _isDone = false;
        _dueDate = dueDate;
    }

    public string Title => _title;

    public bool IsDone => _isDone;

    public DateTime? DueDate => _dueDate;

    public void Complete()
    {
        _isDone = true;
    }

    public void ToggleDone()
    {
        _isDone = !_isDone;
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Название задачи не может быть пустым.");
        }

        _title = title.Trim();
    }

    public void SetDueDate(DateTime? dueDate)
    {
        _dueDate = dueDate;
    }
}
