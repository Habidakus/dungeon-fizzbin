using Godot;
using System;

public partial class PotBackground : Node2D
{
    internal int HighlightPositionId { get; private set; } = -1;
    private Vector2 _highlightVectorDirection_current;
    private Vector2 _highlightVectorDirection_final;
    private float _lerpSpeed = 1f;
    internal void SetHighlight(int positionID, Vector2 direction, float durationInSeconds)
    {
        if (positionID == HighlightPositionId)
            return;

        //if (positionID == -1)
        //    GD.Print("Stopping positionID highlighting");
        //else
        //    GD.Print($"Switching highlight from #{HighlightPositionId} to #{positionID}");

        if (HighlightPositionId == -1)
        {
            // Who knows where we were pointed, let's just rabbit to the new position right now
            _highlightVectorDirection_current = direction;
            QueueRedraw();
        }

        HighlightPositionId = positionID;
        
        // Swing the pot arrow around to point to whoever is next up, but swing it quickly.
        _lerpSpeed = 1f / durationInSeconds;
        if (_lerpSpeed < 3f)
        {
            _lerpSpeed = 3f;
        }

        if (direction != Vector2.Zero)
        {
            _highlightVectorDirection_final = direction;
        }

        //QueueRedraw();
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_highlightVectorDirection_current != _highlightVectorDirection_final)
        {
            var old = _highlightVectorDirection_current;
            _highlightVectorDirection_current = _highlightVectorDirection_current.MoveToward(_highlightVectorDirection_final, (float) delta * _lerpSpeed).Normalized();
            if (old == _highlightVectorDirection_current)
            {
                // We didn't move anywhere, most likely because we're switching our direction by 180 degrees and that
                // won't work with lerp & then normalize. So instead lets just move any direction for now and then it
                // should resolve next pass.
                _highlightVectorDirection_current = _highlightVectorDirection_current.MoveToward(new Vector2(_highlightVectorDirection_final.Y, _highlightVectorDirection_final.X), (float)delta * _lerpSpeed).Normalized();
            }

            QueueRedraw();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Draw()
    {
        Color feltGreen = Main.Color_HandActive;
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
        float len = flerp(width, height, Math.Abs(_highlightVectorDirection_current.Y));
        float x = origin.X + _highlightVectorDirection_current.X * len * 1.15f;
        float y = origin.Y + _highlightVectorDirection_current.Y * len * 1.15f;

        DrawLine(origin, new Vector2(x, y), color, width: 5, antialiased: true);
        
        Vector2[] coords = new Vector2[3];
        float dx = _highlightVectorDirection_current.X * 10f;
        float dy = _highlightVectorDirection_current.Y * 10f;
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
