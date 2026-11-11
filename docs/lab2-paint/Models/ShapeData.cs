using System;
using System.Collections.Generic;

namespace Lab2Paint.Models;

public class ShapeData
{
    public string Type { get; set; } = "";
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int EndX { get; set; }
    public int EndY { get; set; }
    public string Color { get; set; } = "";
    public int Width { get; set; }

    public List<PointData> Points { get; set; } = new();
}

public class PointData
{
    public int X { get; set; }
    public int Y { get; set; }
}