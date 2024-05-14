using Godot;
using System;

#nullable enable

public partial class StateChangeButton : NinePatchRect
{
    [Export]
    public string? State = null;
    [Export]
    public Texture2D? Hover = null;

    public Texture2D? Default = null;
    private state_machine? _state_machine = null;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
        GuiInput += OnGuiInput;
        MouseFilter = MouseFilterEnum.Stop;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void Initialize(state_machine sm)
    {
        Default = Texture;
        _state_machine = sm;
    }

    public void OnMouseEnter()
    {
        if (Hover != null)
        {
            Texture = Hover;
        }
    }

    public void OnMouseExit()
    {
        if (Default != null)
        {
            Texture = Default;
        }
        else
        {
            throw new Exception($"{Name}.Initialize() has not been called to set the state machine.");
        }
    }

    public void OnGuiInput(InputEvent inputEvent)
    {
        if (!string.IsNullOrEmpty(State))
        {
            if (inputEvent is InputEventMouseButton iemb)
            {
                if (iemb.Pressed)
                {
                    if (_state_machine != null)
                    {
                        _state_machine.SwitchState(State);
                    }
                    else
                    {
                        throw new Exception($"{Name}.Initialize() has not been called to set the state machine.");
                    }
                }
            }
        }
        else
        {
            throw new Exception($"{Name}.State has not been set in the Godot editor.");
        }
    }
}
