using Godot;
using System;

public partial class PotBackground : Node2D
{
    internal int HighlightPositionId { get; private set; } = -1;

    private float _playerPointerDirection_current;
    private float _playerPointerDirection_goal;
    private float _lerpSpeed = 1f;

    public Vector2 CurrentDirectionVector
    {
        get
        {
            float x = (float) Math.Cos(_playerPointerDirection_current);
            float y = (float) Math.Sin(_playerPointerDirection_current);
            return new Vector2(x, y);
        }
    }
    private static float GetDirectionFromVector(Vector2 direction)
    {
        return (float)Math.Atan2(direction.Y, direction.X);
    }
    public float RadiansPointerNeedsToMove
    {
        get
        {
            if (_playerPointerDirection_current != _playerPointerDirection_goal)
            {
                float delta = _playerPointerDirection_goal - _playerPointerDirection_current;
                while (delta < -Math.PI)
                {
                    delta += 2f * (float)Math.PI;
                }
                while (delta > Math.PI)
                {
                    delta -= 2f * (float)Math.PI;
                }
                return delta;
            }
            else
            {
                return 0f;
            }
        }
    }

    internal void SetHighlight(int positionID, Vector2 direction, float durationInSeconds)
    {
        if (positionID == HighlightPositionId)
            return;

        if (HighlightPositionId == -1)
        {
            // Who knows where we were pointed, let's just rabbit to the new position right now
            _playerPointerDirection_current = GetDirectionFromVector(direction);
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
            _playerPointerDirection_goal = GetDirectionFromVector(direction);
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_playerPointerDirection_current != _playerPointerDirection_goal)
        {
            float amountWeWantToGo = RadiansPointerNeedsToMove;
            float deltaChange = (float)delta * _lerpSpeed;
            if (deltaChange > Math.Abs(amountWeWantToGo))
            {
                _playerPointerDirection_current = _playerPointerDirection_goal;
            }
            else
            {
                if (amountWeWantToGo > 0)
                    _playerPointerDirection_current += deltaChange;
                else
                    _playerPointerDirection_current -= deltaChange;
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
        Vector2 vec = CurrentDirectionVector;
        float len = flerp(width, height, Math.Abs(vec.Y));
        float x = origin.X + vec.X * len * 1.15f;
        float y = origin.Y + vec.Y * len * 1.15f;

        DrawLine(origin, new Vector2(x, y), color, width: 5, antialiased: true);
        
        Vector2[] coords = new Vector2[3];
        float dx = vec.X * 10f;
        float dy = vec.Y * 10f;
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
