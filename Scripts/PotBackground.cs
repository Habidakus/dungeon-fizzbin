using Godot;
using System;

public partial class PotBackground : Node2D
{
    internal int HighlightPositionId { get; private set; } = -1;
    private Vector2 _highlightVectorDirection;
    internal void SetHighlight(int positionID, Vector2 direction)
    {
        if (positionID == HighlightPositionId)
            return;

        if (positionID == -1)
            GD.Print("Stopping positionID highlighting");
        else
            GD.Print($"Switching highlight from #{HighlightPositionId} to #{positionID}");

        HighlightPositionId = positionID;
        _highlightVectorDirection = direction;

        QueueRedraw();
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Draw()
    {
        Color feltGreen = Color.FromHtml("147754");
        const float width = 69 * 2 + 16;
        const float height = 169 / 2f;
        DrawOval(Vector2.Zero, width, height, feltGreen);
        if (HighlightPositionId != -1)
        {
            DrawArrow(Vector2.Zero, width, height, feltGreen); 
        }
    }

    private static float flerp(float start, float end, float lerp)
    {
        return start + (end - start) * Math.Clamp(lerp, 0f, 1f);
    }

    private void DrawArrow(Vector2 origin, float width, float height, Color color)
    {
        float len = flerp(width, height, Math.Abs(_highlightVectorDirection.Y));
        float x = origin.X + _highlightVectorDirection.X * len * 1.15f;
        float y = origin.Y + _highlightVectorDirection.Y * len * 1.15f;

        DrawLine(origin, new Vector2(x, y), color, width: 5, antialiased: true);
        
        Vector2[] coords = new Vector2[3];
        float dx = _highlightVectorDirection.X * 10f;
        float dy = _highlightVectorDirection.Y * 10f;
        coords[0] = new Vector2(x + dx, y + dy);
        coords[1] = new Vector2(x + dy, y - dx);
        coords[2] = new Vector2(x - dy, y + dx);
        DrawPolygon(coords, new Color[] { color });
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
