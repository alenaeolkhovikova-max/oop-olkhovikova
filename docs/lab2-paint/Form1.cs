using System.Drawing;
using System.Windows.Forms;
using Lab2Paint.Controls;
using Lab2Paint.Models;

namespace Lab2Paint;

public partial class Form1 : Form
{
    private readonly Drawing _drawing;
    private readonly DrawingCanvas _canvas;
    private readonly FlowLayoutPanel _toolbar;
    private Button? _selectedToolButton;

    public Form1()
    {
        InitializeComponent();

        Text = "Графический редактор";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        _drawing = new Drawing();
        _canvas = new DrawingCanvas(_drawing);

        KeyPreview = true;

        KeyDown += (sender, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                _drawing.Undo();
                _canvas.Invalidate();

                e.Handled = true;
            }
        };

        _toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(5),
            BackColor = Color.LightGray
        };

        AddToolButton("Карандаш", "Pencil");
        AddToolButton("Линия", "Line");
        AddToolButton("Прямоугольник", "Rectangle");
        AddToolButton("Эллипс", "Ellipse");

        Button colorButton = new Button
        {
            Text = "Цвет",
            Width = 80,
            Height = 32,
            Margin = new Padding(3)
        };

        colorButton.Click += (sender, e) =>
        {
            using ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _canvas.CurrentColor = colorDialog.Color;
            }
        };

        _toolbar.Controls.Add(colorButton);

        Label widthLabel = new Label
        {
            Text = "Толщина:",
            AutoSize = true,
            Margin = new Padding(8, 10, 3, 3)
        };

        NumericUpDown widthSelector = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 20,
            Value = 2,
            Width = 60,
            Margin = new Padding(3)
        };

        widthSelector.ValueChanged += (sender, e) =>
        {
            _canvas.CurrentWidth = (int)widthSelector.Value;
        };

        _toolbar.Controls.Add(widthLabel);
        _toolbar.Controls.Add(widthSelector);

        Button undoButton = new Button
        {
            Text = "Отменить",
            Width = 90,
            Height = 32,
            Margin = new Padding(3)
        };

        undoButton.Click += (sender, e) =>
        {
            _drawing.Undo();
            _canvas.Invalidate();
        };

        _toolbar.Controls.Add(undoButton);

        Button clearButton = new Button
        {
            Text = "Очистить",
            Width = 90,
            Height = 32,
            Margin = new Padding(3)
        };

        clearButton.Click += (sender, e) =>
        {
            DialogResult result = MessageBox.Show(
                "Вы действительно хотите очистить весь рисунок?",
                "Очистка холста",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _drawing.Clear();
                _canvas.Invalidate();
            }
        };

        _toolbar.Controls.Add(clearButton);

        Button saveButton = new Button
        {
            Text = "Сохранить",
            Width = 100,
            Height = 32,
            Margin = new Padding(3)
        };

        saveButton.Click += (sender, e) =>
        {
            using SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Файлы рисунков (*.json)|*.json",
                Title = "Сохранить рисунок",
                DefaultExt = "json",
                AddExtension = true
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _drawing.SaveToFile(saveDialog.FileName);

                    MessageBox.Show(
                        "Рисунок успешно сохранён!",
                        "Сохранение",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось сохранить рисунок: " + ex.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        };

        _toolbar.Controls.Add(saveButton);

        Button openButton = new Button
        {
            Text = "Открыть",
            Width = 100,
            Height = 32,
            Margin = new Padding(3)
        };

        openButton.Click += (sender, e) =>
        {
            using OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Файлы рисунков (*.json)|*.json",
                Title = "Открыть рисунок"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _drawing.LoadFromFile(openDialog.FileName);
                    _canvas.Invalidate();

                    MessageBox.Show(
                        "Рисунок успешно загружен!",
                        "Открытие",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось открыть рисунок: " + ex.Message,
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        };

        _toolbar.Controls.Add(openButton);

        Controls.Add(_canvas);
        Controls.Add(_toolbar);
    }

    private void AddToolButton(string text, string toolName)
    {
        Button button = new Button
        {
            Text = text,
            Width = 120,
            Height = 32,
            Margin = new Padding(3)
        };

        if (toolName == "Pencil")
        {
            _selectedToolButton = button;
            button.BackColor = Color.LightBlue;
            _canvas.CurrentTool = toolName;
        }

        button.Click += (sender, e) =>
        {
            if (_selectedToolButton != null)
            {
                _selectedToolButton.BackColor = SystemColors.Control;
            }

            _selectedToolButton = button;

            button.BackColor = Color.LightBlue;

            _canvas.CurrentTool = toolName;
        };

        _toolbar.Controls.Add(button);
    }
}