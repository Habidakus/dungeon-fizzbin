using Godot;
using System;
using System.Collections.Generic;

#nullable enable

public partial class HUD : CanvasLayer
{
    [Export]
    public Texture2D? NineGridButton_Default = null;
    [Export]
    public Texture2D? NineGridButton_Hover = null;

    private Control TitlePage { get { return GetChildControl("TitlePage"); } }
    private Control MenuPage { get { return GetChildControl("MenuPage"); } }
    private Control PlayPage { get { return GetChildControl("PlayPage"); } }
    private Control NextHandMenu { get { return GetChildControl(PlayPage, "NextHandMenu"); } }
    private PotBackground PotBackground
    {
        get
        {
            if (FindChild("Pot") is VisibleHand pot)
            {
                if (pot.FindChild("Background") is PotBackground potBackground)
                {
                    return potBackground;
                }
                else
                {
                    throw new Exception($"{Name}.Pot has no child Background");
                }
            }
            else
            {
                throw new Exception($"{Name} has no child Pot");
            }
        }
    }
    private state_machine StateMachine
    {
        get
        {
            if (GetParent() is Main mainNode)
            {
                return mainNode.GetStateMachine();
            }
            else
            {
                throw new Exception($"Parent of {Name} is not main node");
            }
        }
    }

    private Control GetChildControl(Control control, string name)
    {
        if (control.GetNode(name) is Control retVal)
            return retVal;

        throw new Exception($"No child of {Name}.{control.Name} called {name}");
    }

    private Control GetChildControl(string name)
	{
		if (GetNode(name) is Control retVal)
			return retVal;

		throw new Exception($"No child of {Name} called {name}");
	}

    private void InitializeStateChangeButton(Control page, string buttonName)
    {
        if (page.FindChild(buttonName) is StateChangeButton quitButton)
        {
            quitButton.Initialize(StateMachine);
        }
        else
        {
            throw new Exception($"{page.Name} has no child named {buttonName}");
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeStateChangeButton(MenuPage, "PlayButton2");
        InitializeStateChangeButton(MenuPage, "QuitButton2");
        InitializeStateChangeButton(PlayPage, "PlayAnotherHand");
        InitializeStateChangeButton(PlayPage, "LeaveTable");

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
            case "Quit":
                break;
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
            case "Play_Player_Leaves_Game":
                PlayPage.Show();
                break;
            case "Play_Offer_Another_Hand":
                PlayPage.Show();
                NextHandMenu.Show();
                break;
            default:
                throw new Exception($"{Name} has no conception of state \"{state}\"");
        }
	}

    public void EndState(string state)
	{
        switch (state)
        {
            case "Quit":
                break;
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
            case "Play_Player_Leaves_Game":
                PlayPage.Hide();
                break;
            case "Play_Offer_Another_Hand":
                PlayPage.Hide();
                NextHandMenu.Hide();
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

    internal void SetStake(double amount)
    {
        if (PlayPage.FindChild("PlayersCash") is Label label)
        {
            label.Text = $"Your stake: ${amount:F2}";
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
        }

        PotBackground potBackground = PotBackground;
        if (potBackground.HighlightPositionId == positionID)
        {
            potBackground.SetHighlight(-1, Vector2.Zero);
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

    public void HighlightPosition(int positionID)
    {
        Vector2 direction = Vector2.Zero;
        if (positionID != -1)
        {
            if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
            {
                direction = (visibleHand.GlobalPosition - PotBackground.GlobalPosition).Normalized();
            }
        }

        PotBackground.SetHighlight(positionID, direction);
    }

    internal void PlayerLeaves(int positionID, string leavingText)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.PlayerLeaves(leavingText);
        }
    }

    internal void FlingCard(int fromPositionID, int toPositionID, double delay, Random rnd, Card card, int cardIndex, bool isVisible)
    {
        if (FindChild($"Hand{fromPositionID}") is VisibleHand fromHand)
        {
            if (FindChild($"Hand{toPositionID}") is VisibleHand toHand)
            {
                fromHand.FlingCard(card, cardIndex, rnd, isVisible, delay, toHand);
            }
        }
    }
}
