using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        if (PlayPage.FindChild("PlayersCash") is Godot.Label label)
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

    private string _selectedCardsDestination = string.Empty;
    private double _selectedCardsCostPerDiscard = 0;
    private int _selectedCardsGoalCount = -1;
    private List<string>? _selectedCardsAsText = null;
    private bool _selectedCardsConfirmed = false;
    internal async Task<List<string>> HavePlayerSelectCardsToPassOrDiscard(Hand hand)
    {
        GD.Print($"Starting sleep for card selection on {hand}");
        while (true)
        {
            if (_selectedCardsAsText == null)
                throw new Exception($"Why are _selectedCards = null for {hand}");

            if (!_selectedCardsConfirmed)
            {
                await Task.Run(() => Thread.Sleep(10));
            }
            else
            {
                GD.Print($"Ending sleep for card selection on {hand}");
                return _selectedCardsAsText;
            }
        }
    }

    internal void CardSelectionGuiHandler(InputEvent inputEvent, Control visibleCard)
    {
        if (inputEvent is InputEventMouseButton buttonEvent)
        {
            if (buttonEvent.Pressed)
            {
                if (visibleCard.FindChild("Label") is Godot.Label label)
                {
                    if (_selectedCardsAsText == null)
                    {
                        throw new Exception($"Why is _selectedCardsAsText null when {Name}.Text={label.Text} is selected?");
                    }

                    if (visibleCard.FindChild("SelectionMark") is CanvasItem ci)
                    {
                        if (ci.Visible)
                        {
                            _selectedCardsAsText.RemoveAll(a => a.CompareTo(label.Text) == 0);
                            ci.Hide();
                        }
                        else
                        {
                            _selectedCardsAsText.Add(label.Text);
                            ci.Show();
                        }

                        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
                        {
                            if (_selectedCardsDestination.Length > 0)
                                UpdateConfirmationButtonText_Passing(confirmationButton);
                            else
                                UpdateConfirmationButtonText_Discard(confirmationButton);
                        }
                    }
                }
            }
        }
    }

    private void UpdateConfirmationButtonText_Passing(NinePatchRect confirmationButton)
    {
        if (confirmationButton.FindChild("Instructions") is RichTextLabel instructions)
        {
            if (_selectedCardsAsText == null)
            {
                throw new Exception("Why is _selectedCardsAsText null when UpdateConfirmationButtonText_Passing()?");
            }

            if (_selectedCardsAsText.Count == _selectedCardsGoalCount)
            {
                instructions.Text = $"[center]Please click here to confirm passing these {_selectedCardsGoalCount} to {_selectedCardsDestination}[/center]";
            }
            else
            {
                int remainingToSelect = _selectedCardsGoalCount - _selectedCardsAsText.Count;
                if (remainingToSelect > 1)
                {
                    instructions.Text = $"[center]Please select {remainingToSelect} more cards to pass to {_selectedCardsDestination}[/center]";
                }
                else if (remainingToSelect == 1)
                {
                    instructions.Text = $"[center]Please select one more card to pass to {_selectedCardsDestination}[/center]";
                }
                else if (remainingToSelect == -1)
                {
                    instructions.Text = "[center]Too many cards selected: unselect one card[/center]";
                }
                else
                {
                    instructions.Text = $"[center]Too many cards selected: unselect {remainingToSelect} cards[/center]";
                }
            }
        }
    }

    private void UpdateConfirmationButtonText_Discard(NinePatchRect confirmationButton)
    {
        if (confirmationButton.FindChild("Instructions") is RichTextLabel instructions)
        {
            if (_selectedCardsAsText == null)
            {
                throw new Exception("Why is _selectedCardsAsText null when UpdateConfirmationButtonText_Discard()?");
            }

            int overselectCount = _selectedCardsGoalCount - _selectedCardsAsText.Count;
            if (overselectCount == -1)
            {
                instructions.Text = "[center]Too many cards selected: unselect one card[/center]";
            }
            else if (overselectCount < -1)
            {
                instructions.Text = $"[center]Too many cards selected: Unselect {0 - overselectCount} cards[/center]";
            }
            else if (_selectedCardsAsText.Count == 0)
            {
                instructions.Text = "[center]If you wish to discard nothing then click here.[/center]";
            }
            else if (_selectedCardsAsText.Count == 1)
            {
                if (_selectedCardsCostPerDiscard == 0)
                {
                    instructions.Text = $"[center]Confirm you wish to discard just the {_selectedCardsAsText[0]}[/center]";
                }
                else
                {
                    instructions.Text = $"[center]Confirm you wish to discard {_selectedCardsAsText[0]} for ${_selectedCardsCostPerDiscard:F2}[/center]";
                }
            }
            else
            {
                if (_selectedCardsCostPerDiscard == 0)
                {
                    instructions.Text = $"[center]Confirm passing you wish to discard these {_selectedCardsAsText.Count} cards[/center]";
                }
                else 
                { 
                    instructions.Text = $"[center]Confirm passing you wish to discard these {_selectedCardsAsText.Count} cards for ${_selectedCardsCostPerDiscard * _selectedCardsAsText.Count:F2}[/center]";
                }
            }
        }
    }

    internal void OnConfirmationButtonMouseEnter()
    {
        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect npr)
        {
            if (_selectedCardsAsText != null)
            {
                if (_selectedCardsDestination.Length > 0)
                {
                    // We are passing, and must only accept the button if the count is right
                    if (0 == _selectedCardsGoalCount - _selectedCardsAsText.Count)
                    {
                        npr.Texture = NineGridButton_Hover;
                    }
                }
                else
                {
                    // We are discarding, and can accept up to _selectedCardGoalCount discards
                    if (_selectedCardsAsText.Count <= _selectedCardsGoalCount)
                    {
                        npr.Texture = NineGridButton_Hover;
                    }
                }
            }
        }
    }

    internal void OnConfirmationButtonMouseExit()
    {
        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect npr)
        {
            npr.Texture = NineGridButton_Default;
        }
    }

    internal void OnConfirmationButtonInputEvent(InputEvent inputEvent)
    {
        if (_selectedCardsAsText != null)
        {
            if (_selectedCardsDestination.Length > 0)
            {
                // We are passing, and must only accept the button if the count is right
                if (0 != _selectedCardsGoalCount - _selectedCardsAsText.Count)
                    return;
            }
            else
            {
                // We are discarding, and can accept up to _selectedCardGoalCount discards
                if (_selectedCardsAsText.Count > _selectedCardsGoalCount)
                    return;
            }

            if (inputEvent is InputEventMouseButton mouseButton)
            {
                if (!mouseButton.Pressed)
                    return;

                _selectedCardsConfirmed = true;
            }
        }
        else if (_potentialBetValues != null)
        {
            // We are selecting a potential amount to bet
            if (PlayPage.FindChild("BetSlider") is Slider range)
            {
                if (inputEvent is InputEventMouseButton mouseButton)
                {
                    if (!mouseButton.Pressed)
                        return;

                    int index = (int)Math.Round(range.Value);
                    _betValue = _potentialBetValues[index];
                }
            }
        }
    }

    private List<double>? _potentialBetValues = null;
    private double _betValue = -1;

    internal async Task<double> HaveChosenAmountToBet()
    {
        GD.Print($"Starting sleep for bet selection");
        while (true)
        {
            if (_betValue < 0)
            {
                await Task.Run(() => Thread.Sleep(10));
            }
            else
            {
                GD.Print($"Ending sleep for bet selection");
                return _betValue;
            }
        }
    }

    internal void EnableBetSlider(double betFloor)
    {
        _betValue = -1;
        _potentialBetValues = new List<double>() { 0, betFloor };
        _potentialBetValues.AddRange(Player.GetNextBets(betFloor, 8));
        if (PlayPage.FindChild("BetSlider") is Slider range)
        {
            range.Value = 1;
            range.Show();
        }
    }

    internal void DisableBetSlider()
    {
        _betValue = -1;
        _potentialBetValues = null;

        if (PlayPage.FindChild("BetSlider") is Slider range)
        {
            range.Hide();
        }
        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
        {
            confirmationButton.Hide();
        }
    }

    private void UpdateConfirmationButtonText_Bet(NinePatchRect confirmationButton, int value)
    {
        if (confirmationButton.FindChild("Instructions") is RichTextLabel instructions)
        {
            if (_potentialBetValues == null)
            {
                throw new Exception("Why is _potentialBetValues null when UpdateConfirmationButtonText_Bet()?");
            }

            if (value == 0)
                instructions.Text = "[center]Fold[/center]";
            else if (value == 1)
                instructions.Text = $"[center]Hold at ${_potentialBetValues[1]:F2} - or adjust slider[/center]";
            else
                instructions.Text = $"[center]Raise to ${_potentialBetValues[value]:F2}[/center]";
        }
    }

    internal void OnBetSliderChanged(float value)
    {
        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
        {
            UpdateConfirmationButtonText_Bet(confirmationButton, (int)Math.Round(value));
            confirmationButton.Show();
        }
    }

    private void EnableCardSelection(int positionID, int goalCount)
    {
        _selectedCardsGoalCount = goalCount;
        _selectedCardsAsText = new List<string>();
        _selectedCardsConfirmed = false;
        if (FindChild($"Hand{positionID}") is VisibleHand hand)
        {
            hand.EnableCardSelection(CardSelectionGuiHandler);
        }
    }

    internal void EnableCardSelection_Passing(int positionID, int goalCount, string destination)
    {
        EnableCardSelection(positionID, goalCount);
        _selectedCardsDestination = destination;
        _selectedCardsCostPerDiscard = 0;

        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
        {
            UpdateConfirmationButtonText_Passing(confirmationButton);
            confirmationButton.Show();
        }
    }

    internal void EnableCardSelection_Discard(int positionID, int goalCount, double costPerDiscard)
    {
        EnableCardSelection(positionID, goalCount);
        _selectedCardsDestination = string.Empty;
        _selectedCardsCostPerDiscard = costPerDiscard;

        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
        {
            UpdateConfirmationButtonText_Discard(confirmationButton);
            confirmationButton.Show();
        }
    }

    internal void DisableCardSelection(int positionID)
    {
        _selectedCardsGoalCount = -1;
        _selectedCardsAsText = null;
        _selectedCardsDestination = string.Empty;
        _selectedCardsConfirmed = false;
        if (FindChild($"Hand{positionID}") is VisibleHand hand)
        {
            hand.DisableCardSelection(CardSelectionGuiHandler);
        }

        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
        {
            confirmationButton.Hide();
        }
    }
}
