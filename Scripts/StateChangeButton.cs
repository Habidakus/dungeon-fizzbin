using Godot;
using System;

#nullable enable

public partial class StateChangeButton : NinePatchRect
{
    [Export]
    public string? State = null;

    private state_machine? _state_machine = null;
    private Object? _additional_info = null;
    private ColorRect? _color_rect = null;

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

    public void Initialize(state_machine sm, string bbCode, Object? additionalInfo = null)
    {
        _state_machine = sm;
        _additional_info = additionalInfo;
        _color_rect = FindChild("ColorRect") as ColorRect;
        if (_color_rect != null)
        {
            _color_rect.Color = Main.Color_ButtonDefault;
        }

        if (FindChild("Text") is RichTextLabel rtl)
        {
            rtl.Text = bbCode;
            //rtl.UpdateMinimumSize();
            //rtl.CustomMinimumSize = rtl.Size;
            //this.CustomMinimumSize = rtl.Size + new Vector2(PatchMarginLeft /*+ PatchMarginRight*/, PatchMarginBottom /*+ PatchMarginTop*/);
            //this.ResetSize();
            //GD.Print($"{Name}.Text.MinSize = {rtl.CustomMinimumSize} {Name}.MinSize = {CustomMinimumSize} {Name}.Text.Size = {rtl.Size} {Name}.Size = {Size}");
        }
    }

    public void OnMouseEnter()
    {
        if (_color_rect != null)
        {
            _color_rect.Color = Main.Color_ButtonHover;
        }
    }

    public void OnMouseExit()
    {
        if (_color_rect != null)
        {
            _color_rect.Color = Main.Color_ButtonDefault;
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
                        _state_machine.SwitchState(State, _additional_info);
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
