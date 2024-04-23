using Godot;
using System;

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
            case "Play_Deal":
            case "Play_Loop":
            case "Play_Someone_Discards":
            case "Play_Animate_Discards":
            case "Play_Someone_Bets":
            case "Play_Someone_Reveals":
            case "Play_Declare_Winner":
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
            case "Play_Deal":
            case "Play_Loop":
            case "Play_Someone_Discards":
            case "Play_Animate_Discards":
            case "Play_Someone_Bets":
            case "Play_Someone_Reveals":
            case "Play_Declare_Winner":
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
            mainNode.GetStateMachine().SwitchState("Play_Deal");
        }
    }

    internal void SetVisibleHand(Hand hand, Player nonNPCPlayer)
    {
        if (FindChild($"Hand{hand.PositionID}") is VisibleHand visibleHand)
        {
            visibleHand.Update(hand, nonNPCPlayer);
        }
    }

    internal void MoveCardToDiscard(int positionID, Card card)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.AddDiscard(card);
        }
    }

    public void FoldHand(int positionID)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.FoldHand();
        }
    }

    internal void SetBetAmount(int positionID, double amountBet, string? description)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.SetBetAmount(amountBet, description);
        }
    }

    internal void SetFeltToLost(int positionID)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.SetFeltToLost();
        }
    }
}
