using System.Drawing;
using System.Collections.Generic;

namespace Lab2Paint.Shapes;

public class PencilShape : Shape
{
    private readonly List<Point> _points = new();

    public IReadOnlyList<Point> Points => _points.AsReadOnly();

    public PencilShape(Point start, Color color, int width)
        : base(start, color, width)
    {
        _points.Add(start);
    }

    public PencilShape(
        Point start,
        Color color,
        int width,
        IEnumerable<Point> points)
        : base(start, color, width)
    {
        _points.AddRange(points);

        if (_points.Count == 0)
        {
            _points.Add(start);
        }

        MoveEnd(_points[^1]);
    }

    public override void MoveEnd(Point p)
    {
        base.MoveEnd(p);
        _points.Add(p);
    }

    public override void Draw(Graphics g)
    {
        using Pen pen = new Pen(Color, Width);

        if (_points.Count == 1)
        {
            g.DrawLine(pen, _points[0], _points[0]);
            return;
        }

        g.DrawLines(pen, _points.ToArray());
    }
}