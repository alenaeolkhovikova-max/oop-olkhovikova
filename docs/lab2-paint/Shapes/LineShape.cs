using System.Drawing;

namespace Lab2Paint.Shapes;

public class LineShape : Shape
{
    public LineShape(Point start, Color color, int width)
        : base(start, color, width)
    {
    }

    public override void Draw(Graphics g)
    {
        using Pen pen = new Pen(Color, Width);
        g.DrawLine(pen, Start, End);
    }
}