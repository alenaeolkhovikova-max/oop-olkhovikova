
using Lab1Todo.Models;
using Lab1Todo.Services;

namespace Lab1Todo;

public partial class Form1 : Form
{
    private readonly TodoList _list = new();
    private readonly string _filePath =
        Path.Combine(AppContext.BaseDirectory, "tasks.json");

    private readonly TextBox _titleBox = new();
    private readonly TextBox _searchBox = new();
    private readonly DateTimePicker _dueDatePicker = new();
    private readonly CheckBox _noDueDate = new();
    private readonly ListBox _taskList = new();
    private readonly ComboBox _filterBox = new();

    public Form1()
    {
        Text = "Мой список задач";
        Width = 800;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        BuildInterface();
        ConnectEvents();

        try
        {
            _list.LoadFromFile(_filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить задачи: {ex.Message}",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        RefreshTasks();
    }

    
private void BuildInterface()
{
    var titleLabel = new Label
    {
        Text = "Название задачи:",
        Left = 20,
        Top = 20,
        AutoSize = true
    };

    _titleBox.SetBounds(20, 45, 350, 30);

    var dueLabel = new Label
    {
        Text = "Срок выполнения:",
        Left = 390,
        Top = 20,
        AutoSize = true
    };

    _dueDatePicker.SetBounds(390, 45, 200, 30);
    _dueDatePicker.Format = DateTimePickerFormat.Short;

    _noDueDate.Text = "Без срока";
    _noDueDate.SetBounds(600, 45, 120, 30);
    _noDueDate.Checked = true;
    _dueDatePicker.Enabled = false;

    var addButton = new Button
    {
        Text = "Добавить",
        Left = 20,
        Top = 90,
        Width = 110,
        Height = 35
    };
    addButton.Click += (_, _) => AddTask();

    var completeButton = new Button
    {
        Text = "Выполнить",
        Left = 140,
        Top = 90,
        Width = 110,
        Height = 35
    };
    completeButton.Click += (_, _) => CompleteTask();

    var renameButton = new Button
    {
        Text = "Переименовать",
        Left = 260,
        Top = 90,
        Width = 130,
        Height = 35
    };
    renameButton.Click += (_, _) => RenameTask();

    var deleteButton = new Button
    {
        Text = "Удалить",
        Left = 400,
        Top = 90,
        Width = 100,
        Height = 35
    };
    deleteButton.Click += (_, _) => DeleteTask();

    var searchLabel = new Label
    {
        Text = "Поиск:",
        Left = 20,
        Top = 145,
        AutoSize = true
    };

    _searchBox.SetBounds(80, 140, 250, 30);

    var filterLabel = new Label
    {
        Text = "Показать:",
        Left = 350,
        Top = 145,
        AutoSize = true
    };

    _filterBox.SetBounds(430, 140, 200, 30);
    _filterBox.DropDownStyle = ComboBoxStyle.DropDownList;
    _filterBox.Items.AddRange(new object[]
    {
        "Все",
        "Активные",
        "Выполненные"
    });
    _filterBox.SelectedIndex = 0;

    _taskList.SetBounds(20, 185, 740, 330);
    _taskList.FormattingEnabled = true;
    _taskList.DrawMode = DrawMode.OwnerDrawFixed;
_taskList.ItemHeight = 24;

_taskList.DrawItem += (_, e) =>
{
    if (e.Index < 0)
        return;

    e.DrawBackground();

    if (_taskList.Items[e.Index] is TodoItem task)
    {
        bool overdue =
            !task.IsDone &&
            task.DueDate.HasValue &&
            task.DueDate.Value.Date < DateTime.Today;

        Color textColor = overdue
            ? Color.Red
            : _taskList.ForeColor;

        using var brush = new SolidBrush(textColor);

        e.Graphics.DrawString(
            _taskList.Items[e.Index].ToString() is not null
                ? GetTaskDisplayText(task)
                : "",
            e.Font ?? _taskList.Font,
            brush,
            e.Bounds);
    }

    e.DrawFocusRectangle();
};
_taskList.Format += (_, args) =>
{
    if (args.ListItem is TodoItem task)
    {
        string status = task.IsDone ? "[X]" : "[ ]";

        string due = task.DueDate.HasValue
            ? $" — до {task.DueDate.Value:dd.MM.yyyy}"
            : "";

        args.Value = $"{status} {task.Title}{due}";
    }
};
    _taskList.Anchor =
        AnchorStyles.Top | AnchorStyles.Bottom |
        AnchorStyles.Left | AnchorStyles.Right;

    Controls.AddRange(new Control[]
    {
        titleLabel,
        _titleBox,
        dueLabel,
        _dueDatePicker,
        _noDueDate,
        addButton,
        completeButton,
        renameButton,
        deleteButton,
        searchLabel,
        _searchBox,
        filterLabel,
        _filterBox,
        _taskList
    });
}


    private void ConnectEvents()
    {
        _noDueDate.CheckedChanged += (_, _) =>
            _dueDatePicker.Enabled = !_noDueDate.Checked;

        _searchBox.TextChanged += (_, _) => RefreshTasks();
        _filterBox.SelectedIndexChanged += (_, _) => RefreshTasks();
    }

    private void AddTask()
    {
        try
        {
            DateTime? dueDate = _noDueDate.Checked
                ? null
                : _dueDatePicker.Value.Date;

            _list.Add(_titleBox.Text, dueDate);
            SaveTasks();

            _titleBox.Clear();
            RefreshTasks();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void CompleteTask()
    {
        int index = GetSelectedIndex();
        if (index < 0) return;

        try
        {
            TodoItem task = _taskList.SelectedItem as TodoItem
                ?? throw new InvalidOperationException(
                    "Не удалось получить выбранную задачу.");

            int actualIndex = FindTaskIndex(task);
            _list.ToggleDone(actualIndex);

            SaveTasks();
            RefreshTasks();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void RenameTask()
    {
        int index = GetSelectedIndex();
        if (index < 0) return;

        var task = (TodoItem)_taskList.SelectedItem!;

        string newTitle = Microsoft.VisualBasic.Interaction.InputBox(
            "Введите новое название:",
            "Переименовать задачу",
            task.Title);

        if (string.IsNullOrWhiteSpace(newTitle))
            return;

        try
        {
            _list.Rename(FindTaskIndex(task), newTitle);
            SaveTasks();
            RefreshTasks();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void DeleteTask()
    {
        int index = GetSelectedIndex();
        if (index < 0) return;

        var task = (TodoItem)_taskList.SelectedItem!;

        var answer = MessageBox.Show(
            $"Удалить задачу «{task.Title}»?",
            "Подтверждение",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
            return;

        try
        {
            _list.Remove(FindTaskIndex(task));
            SaveTasks();
            RefreshTasks();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private int GetSelectedIndex()
    {
        if (_taskList.SelectedItem is not TodoItem)
        {
            MessageBox.Show(
                "Сначала выберите задачу в списке.",
                "Внимание",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return -1;
        }

        return _taskList.SelectedIndex;
    }

    private int FindTaskIndex(TodoItem task)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (ReferenceEquals(_list.GetItem(i), task))
                return i;
        }

        throw new InvalidOperationException(
            "Задача больше не существует.");
    }

    private void RefreshTasks()
    {
        IEnumerable<TodoItem> tasks = _list.Filter(
            _filterBox.SelectedIndex switch
            {
                1 => TodoFilter.Active,
                2 => TodoFilter.Completed,
                _ => TodoFilter.All
            });

        tasks = tasks.Where(task =>
            task.Title.Contains(
                _searchBox.Text.Trim(),
                StringComparison.OrdinalIgnoreCase));

        _taskList.Items.Clear();

        foreach (TodoItem task in tasks)
            _taskList.Items.Add(task);
    }

    private void SaveTasks()
    {
        _list.SaveToFile(_filePath);
    }

    private void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "Ошибка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
}
private string GetTaskDisplayText(TodoItem task)
{
    string status = task.IsDone ? "[X]" : "[ ]";

    string due = task.DueDate.HasValue
        ? $" — до {task.DueDate.Value:dd.MM.yyyy}"
        : "";

    return $"{status} {task.Title}{due}";
}
}

