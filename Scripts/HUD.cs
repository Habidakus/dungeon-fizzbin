using Godot;
using System;

public partial class HUD : CanvasLayer
{
	private Control TitlePage { get { return GetChildControl("TitlePage"); } }
    private Control MenuPage { get { return GetChildControl("MenuPage"); } }

    private Control GetChildControl(string name)
	{
		if (GetNode(name) is Control retVal)
			return retVal;

		throw new Exception($"No child of {Name} called {name}");
	}

    public void OnQuitButtonToggled(bool isOn)
    {
        GD.Print($"{Name}.OnQuitButtonToggled={isOn}");
    }

    public void _on_quit_button_focus_entered()
    {
        GD.Print($"{Name}._on_quit_button_focus_entered()");
    }

    public void _on_quit_button_pressed()
    {
        GD.Print("Pressed");
        GetTree().Quit();
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        TitlePage.Hide();
        MenuPage.Hide();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
    }

	public void StartState(string state)
	{
		switch (state)
		{
			case "StartUp":
                TitlePage.Show();
				break;
            case "Menu":
                MenuPage.Show();
                break;
            default:
                throw new Exception($"{Name} has no conception of state \"{state}\"");
        }
	}

    public void EndState(string state)
	{
        switch (state)
        {
            case "StartUp":
                TitlePage.Hide();
                break;
            case "Menu":
                MenuPage.Hide();
                break;
            default:
                throw new Exception($"{Name} has no conception of state \"{state}\"");
        }
    }
}
