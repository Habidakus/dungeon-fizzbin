using Godot;
using System;

public partial class MenuButton : Button
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print($"{Name}._Ready()");
        //Pressed += WePressed;
        Connect("pressed", Callable.From(WePressed));
    }

    public void WePressed()
    {
        GD.Print($"{Name}.WePressed()");
    }
}
