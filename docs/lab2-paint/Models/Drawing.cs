using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using Lab2Paint.Shapes;
using System.Linq;

namespace Lab2Paint.Models;

public class Drawing
{
    private readonly List<Shape> _shapes = new();

    public IReadOnlyList<Shape> Shapes => _shapes.AsReadOnly();

    public void AddShape(Shape shape)
    {
        _shapes.Add(shape);
    }

    public void Undo()
    {
        if (_shapes.Count > 0)
        {
            _shapes.RemoveAt(_shapes.Count - 1);
        }
    }

    public void Clear()
    {
        _shapes.Clear();
    }

    public void DrawAll(Graphics g)
    {
        foreach (Shape shape in _shapes)
        {
            shape.Draw(g);
        }
    }

    public void SaveToFile(string filePath)
    {
        List<ShapeData> data = new();

        foreach (Shape shape in _shapes)
        {
            ShapeData item = new()
            {
                StartX = shape.Start.X,
                StartY = shape.Start.Y,
                EndX = shape.End.X,
                EndY = shape.End.Y,
                Color = ColorTranslator.ToHtml(shape.Color),
                Width = shape.Width
            };

            switch (shape)
            {
                case LineShape:
                    item.Type = "Line";
                    break;

                case RectShape:
                    item.Type = "Rectangle";
                    break;

                case EllipseShape:
                    item.Type = "Ellipse";
                    break;

                case PencilShape pencil:
                    item.Type = "Pencil";

                    foreach (Point point in pencil.Points)
                    {
                        item.Points.Add(new PointData
                        {
                            X = point.X,
                            Y = point.Y
                        });
                    }

                    break;
            }

            data.Add(item);
        }

        string json = JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions { WriteIndented = true }
        );

        File.WriteAllText(filePath, json);
    }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        string json = File.ReadAllText(filePath);

        List<ShapeData> data =
            JsonSerializer.Deserialize<List<ShapeData>>(json) ?? new();

        _shapes.Clear();

        foreach (ShapeData item in data)
        {
            Color color = ColorTranslator.FromHtml(item.Color);
            Point start = new(item.StartX, item.StartY);
            Point end = new(item.EndX, item.EndY);

            Shape? shape = item.Type switch
            {
                "Line" => new LineShape(start, color, item.Width),

                "Rectangle" => new RectShape(start, color, item.Width),

                "Ellipse" => new EllipseShape(start, color, item.Width),

                "Pencil" => new PencilShape(
                    start,
                    color,
                    item.Width,
                    item.Points.Select(p => new Point(p.X, p.Y))
                ),

                _ => null
            };

            if (shape != null)
            {
                shape.MoveEnd(end);
                _shapes.Add(shape);
            }
        }
    }
}