using Godot;
using System;
using System.Collections.Generic;

public partial class PotBackground : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        // TODO: Is this needed? Maybe only if we switch who's to be highlighted
        //QueueRedraw();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Draw()
    {
        DrawOval(Vector2.Zero, width: 69 * 2 + 16, height: 169/2f, Color.FromHtml("147754"));
    }

    private void DrawOval(Vector2 origin, float width, float height, Color color)
    {
        Vector2[] coords = new Vector2[36];
        for (int i = 0; i < coords.Length; ++i)
        {
            double x = origin.X + Math.Sin(Math.PI * i / 18.0) * width;
            double y = origin.Y + Math.Cos(Math.PI * i / 18.0) * height;
            coords[i] = new Vector2((float)x, (float)y);
        }

        DrawPolygon(coords, new Color[] { color });
    }
}
