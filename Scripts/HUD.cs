using Godot;
using System;
using System.Collections.Generic;

public partial class HUD : CanvasLayer
{
	private Control TitlePage { get { return GetChildControl("TitlePage"); } }
    private Control MenuPage { get { return GetChildControl("MenuPage"); } }
    private Control PlayPage { get { return GetChildControl("PlayPage"); } }

    private Control GetChildControl(string name)
	{
		if (GetNode(name) is Control retVal)
			return retVal;

		throw new Exception($"No child of {Name} called {name}");
	}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TitlePage.Hide();
        MenuPage.Hide();
        PlayPage.Hide();
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
            case "Play":
                PlayPage.Show();
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
            case "Play":
                PlayPage.Hide();
                break;
            default:
                throw new Exception($"{Name} has no conception of state \"{state}\"");
        }
    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    public void OnPlayPressed()
    {
        if (GetParent() is Main mainNode)
        {
            mainNode.GetStateMachine().SwitchState("Play");
        }
    }

    internal void SetVisibleHand(Hand hand)
    {
        if (FindChild($"Hand{hand.PositionID}") is VisibleHand visibleHand)
        {
            visibleHand.Update(hand);
        }
    }

    internal void SetHandDiscards(Hand hand, List<Card> discards)
    {
        if (FindChild($"Hand{hand.PositionID}") is VisibleHand visibleHand)
        {
            visibleHand.SetDiscards(discards);
        }
    }

}
