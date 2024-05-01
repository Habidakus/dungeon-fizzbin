using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

#nullable enable

public partial class HUD : CanvasLayer
{
	private Control TitlePage { get { return GetChildControl("TitlePage"); } }
    private Control MenuPage { get { return GetChildControl("MenuPage"); } }
    private Control PlayPage { get { return GetChildControl("PlayPage"); } }
    private int CurrentHighlightPositionId { get; set; }

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

        CurrentHighlightPositionId = -1;
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
            case "Play_Someone_Passes":
            case "Play_Resolve_PassAndRiver":
            case "Play_Someone_Discards":
            case "Play_Animate_Discards":
            case "Play_Someone_Bets":
            case "Play_Someone_Reveals":
            case "Play_Declare_Winner":
            case "Play_Post_Discard":
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
            case "Play_Someone_Passes":
            case "Play_Resolve_PassAndRiver":
            case "Play_Someone_Discards":
            case "Play_Animate_Discards":
            case "Play_Someone_Bets":
            case "Play_Someone_Reveals":
            case "Play_Declare_Winner":
            case "Play_Post_Discard":
                PlayPage.Hide();
                break;
            default:
                throw new Exception($"{Name} has no conception of state \"{state}\"");
        }
    }

    internal void HideRiver()
    {
        if (PlayPage.FindChild("Pot") is VisibleHand river)
        {
            river.Hide();
        }
    }

    internal void ShowRiver(List<Card> cards)
    {
        if (PlayPage.FindChild("Pot") is VisibleHand river)
        {
            river.UpdateRiver(cards);
            river.Show();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Pot");
        }
    }

    internal void SetPot(double amount)
    {
        if (PlayPage.FindChild("Pot") is VisibleHand river)
        {
            river.SetPot(amount);
            river.Show();
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

    internal void SetPlayerInfo(Player player)
    {
        if (FindChild($"Hand{player.PositionID}") is VisibleHand visibleHand)
        {
            visibleHand.ResetDisplay();
            visibleHand.SetPlayerInfo(player.Name, player.Species);
        }
    }

    internal void SetVisibleHand(Hand hand, Player nonNPCPlayer)
    {
        if (FindChild($"Hand{hand.PositionID}") is VisibleHand visibleHand)
        {
            visibleHand.Update(hand, nonNPCPlayer);
        }
    }

    internal void MoveCardToDiscard(int positionID, Card card, bool isVisibileToNonNPC)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.AddDiscard(card, isVisibileToNonNPC);
        }
    }

    public void Ante(int positionID, double amountBet)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.Ante(amountBet);
        }
    }

    public void FoldHand(int positionID, double amountBet)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.FoldHand(amountBet);

            // TODO: When we get a better highlight, remove this. We're just clearing out the reset to standard green here.
            if (CurrentHighlightPositionId == positionID)
            {
                CurrentHighlightPositionId = -1;
            }
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

    internal void ExposeCardToOtherPlayer(int positionID, Card card, Player viewingPlayer)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            if (viewingPlayer.IsNPC)
                visibleHand.ExposeNonNPCCardToNPC(card);
            else
                visibleHand.ExposeCardToNonNPC(card);
        }
    }

    public void HighlightPosition(int positionId)
    {
        if (CurrentHighlightPositionId == positionId)
        {
            return;
        }

        if (CurrentHighlightPositionId != -1)
        {
            if (FindChild($"Hand{CurrentHighlightPositionId}") is VisibleHand visibleHand)
            {
                visibleHand.ClearHighlight(positionId != -1 ? $"Switching to #{positionId}" : "Turning off");
            }
        }

        CurrentHighlightPositionId = positionId;

        if (CurrentHighlightPositionId != -1)
        {
            if (FindChild($"Hand{CurrentHighlightPositionId}") is VisibleHand visibleHand)
            {
                visibleHand.SetHighlight();
            }
        }
    }

}
