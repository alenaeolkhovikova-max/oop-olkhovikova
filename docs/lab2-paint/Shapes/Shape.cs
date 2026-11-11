using System.Drawing;

namespace Lab2Paint.Shapes;

public abstract class Shape
{
    public Point Start { get; protected set; }
    public Point End { get; protected set; }

    public Color Color { get; }
    public int Width { get; }

    protected Shape(Point start, Color color, int width)
    {
        Start = End = start;
        Color = color;
        Width = width;
    }

    public virtual void MoveEnd(Point p)
    {
        End = p;
    }

    protected Rectangle Bounds()
    {
        return new Rectangle(
            Math.Min(Start.X, End.X),
            Math.Min(Start.Y, End.Y),
            Math.Abs(End.X - Start.X),
            Math.Abs(End.Y - Start.Y)
        );
    }

    public abstract void Draw(Graphics g);
}