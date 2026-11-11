using System.Drawing;
using System.Windows.Forms;
using Lab2Paint.Models;
using Lab2Paint.Shapes;
using System.ComponentModel;

namespace Lab2Paint.Controls;

public class DrawingCanvas : Panel
{
    private readonly Drawing _drawing;

    private Shape? _currentShape;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CurrentTool { get; set; } = "Pencil";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color CurrentColor { get; set; } = Color.Black;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CurrentWidth { get; set; } = 2;

    public DrawingCanvas(Drawing drawing)
    {
        _drawing = drawing;

        DoubleBuffered = true;
        BackColor = Color.White;
        Dock = DockStyle.Fill;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        Point start = e.Location;

        _currentShape = CurrentTool switch
        {
            "Line" => new LineShape(start, CurrentColor, CurrentWidth),
            "Rectangle" => new RectShape(start, CurrentColor, CurrentWidth),
            "Ellipse" => new EllipseShape(start, CurrentColor, CurrentWidth),
            "Pencil" => new PencilShape(start, CurrentColor, CurrentWidth),
            _ => null
        };
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_currentShape == null)
        {
            return;
        }

        _currentShape.MoveEnd(e.Location);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (_currentShape == null)
        {
            return;
        }

        _drawing.AddShape(_currentShape);
        _currentShape = null;

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        _drawing.DrawAll(e.Graphics);

        _currentShape?.Draw(e.Graphics);
    }
}