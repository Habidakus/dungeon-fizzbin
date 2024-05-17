using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

public partial class HUD : CanvasLayer
{
    [Export]
    public Texture2D? NineGridButton_Default = null;
    [Export]
    public Texture2D? NineGridButton_Hover = null;
    [Export]
    public Texture2D? SpeciesUnlockAchievement = null;
    [Export]
    public Texture2D? GoldAchievement = null;
    [Export]
    public Texture2D? SilverAchievement = null;
    [Export]
    public Texture2D? BronzeAchievement = null;
    [Export]
    public PackedScene? VisibleAchievement = null;
    [Export]
    public PackedScene? SpeciesSelectButton = null;

    private Control TitlePage { get { return GetChildControl("TitlePage"); } }
    private Control MenuPage { get { return GetChildControl("MenuPage"); } }
    private Control PlayPage { get { return GetChildControl("PlayPage"); } }
    private Control NextHandMenu { get { return GetChildControl(PlayPage, "NextHandMenu"); } }
    private Control AchievementsPage { get { return GetChildControl("AchievementsPage"); } }
    private Control PlayAsNewSpeciesPage { get { return GetChildControl("PlayAsNewSpecies"); } }
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
        InitializeStateChangeButton(MenuPage, "Achievements");
        InitializeStateChangeButton(MenuPage, "NewPlayer");
        InitializeStateChangeButton(PlayPage, "PlayAnotherHand");
        InitializeStateChangeButton(PlayPage, "LeaveTable");
        InitializeStateChangeButton(AchievementsPage, "BackButton");
        InitializeStateChangeButton(PlayAsNewSpeciesPage, "BackButton");

        TitlePage.Hide();
        MenuPage.Hide();
        PlayPage.Hide();
        AchievementsPage.Hide();
        PlayAsNewSpeciesPage.Hide();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        lock (_popUpUnlocks)
        {
            if (_popUpUnlocks.Count > 0)
            {
                if (_popUpWait > 0)
                {
                    _popUpWait -= delta;
                    if (_popUpWait <= 0)
                    {
                        _popUpUnlocks.RemoveAt(0);
                        _popUpWait = 0;
                    }
                }
                else
                {
                    _popUpWait = 5;
                    ShowNextPopUpAchievement();
                }

            }
        }
    }

    private void ShowMenuPage(Main mainNode)
    {
        if (MenuPage.FindChild("Achievements") is StateChangeButton achievementsButton)
        {
            if (mainNode.Achievments.AchievementsUnlocked.Any())
            {
                achievementsButton.Show();
            }
            else
            {
                achievementsButton.Hide();
            }
        }

        if (MenuPage.FindChild("NewPlayer") is StateChangeButton newPlayerButton)
        {
            if (Species.GetUnlockedSpecies(mainNode.Achievments).Where(a => a != Species.Human).Any())
            {
                newPlayerButton.Show();
            }
            else
            {
                newPlayerButton.Hide();
            }
        }

        MenuPage.Show();
    }

    public void StartState(string state, Main mainNode)
	{
		switch (state)
        {
            case "Quit":
                break;
            case "StartUp":
                TitlePage.Show();
				break;
            case "Menu":
                ShowMenuPage(mainNode);
                break;
            case "Achievements":
                AchievementsPage.Show();
                break;
            case "PlayAsNewSpecies":
                PlayAsNewSpeciesPage.Show();
                break;
            case "ChangeSpecies":
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
            case "Achievements":
                AchievementsPage.Hide();
                break;
            case "PlayAsNewSpecies":
                PlayAsNewSpeciesPage.Hide();
                break;
            case "ChangeSpecies":
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
            river.HideRiver();
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

    internal void ClearAllCardIcons(Hand hand)
    {
        if (FindChild($"Hand{hand.PositionID}") is VisibleHand visibleHand)
        {
            visibleHand.ClearEyeIcons();
        }
    }

    internal void MoveCardToDiscard(int positionID, Card card, List<int> playersWhoCanSeeThisDiscard, int nonNPCPositionID)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            visibleHand.AddDiscard(card, playersWhoCanSeeThisDiscard, positionID, nonNPCPositionID);
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

    internal void ExposeCardToOtherPlayer(int positionID, Card card, Player viewingPlayer, int[] playersWhoCanSeeOtherThanOwnerAndNonNPC)
    {
        if (FindChild($"Hand{positionID}") is VisibleHand visibleHand)
        {
            if (!viewingPlayer.IsNPC)
                visibleHand.ExposeCardToNonNPC(card);
            
            //else visibleHand.ExposeNonNPCCardToNPC(card, viewingPlayer.PositionID);

            visibleHand.FineTuneVisibility(card, playersWhoCanSeeOtherThanOwnerAndNonNPC);
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
        //GD.Print($"Starting sleep for card selection on {hand}");
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
                //GD.Print($"Ending sleep for card selection on {hand}");
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
                        throw new Exception($"Why is _selectedCardsAsText null when {Name}.Text={label.Text} is selected? (note that _potentialBetValues={_potentialBetValues})");
                    }

                    CanvasItem ci = VisibleHand.GetCardSelectionMark(visibleCard);
                    if (ci.Visible)
                    {
                        _selectedCardsAsText.RemoveAll(a => a.CompareTo(label.Text) == 0);
                        ci.Hide();
                    }
                    else
                    {
                        _selectedCardsAsText.Add(label.Text);
                        ci.Show();
                        if (ci is Node2D scaleableNode)
                        {
                            Tween tween = scaleableNode.GetTree().CreateTween();
                            tween.TweenProperty(scaleableNode, "scale", new Vector2(3, 3), 0);
                            tween.TweenProperty(scaleableNode, "modulate:a", 0f, 0);
                            tween.TweenProperty(scaleableNode, "scale", new Vector2(1, 1), 0.333).SetEase(Tween.EaseType.Out);
                            tween.Parallel();
                            tween.TweenProperty(scaleableNode, "modulate:a", 1f, 0.2).SetEase(Tween.EaseType.Out);
                        }
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
                instructions.Text = $"[center]Pass these {_selectedCardsGoalCount} to {_selectedCardsDestination}[/center]";
            }
            else
            {
                int remainingToSelect = _selectedCardsGoalCount - _selectedCardsAsText.Count;
                if (remainingToSelect > 1)
                {
                    if (_selectedCardsAsText.Count > 0)
                    {
                        instructions.Text = $"[center]Select {remainingToSelect} more cards to pass to {_selectedCardsDestination}[/center]";
                    }
                    else
                    {
                        instructions.Text = $"[center]Please select {remainingToSelect} cards to pass to {_selectedCardsDestination}[/center]";
                    }
                }
                else if (remainingToSelect == 1)
                {
                    if (_selectedCardsAsText.Count > 0)
                    {
                        instructions.Text = $"[center]Select one more card to pass to {_selectedCardsDestination}[/center]";
                    }
                    else
                    {
                        instructions.Text = $"[center]Please select one card to pass to {_selectedCardsDestination}[/center]";
                    }
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
                    instructions.Text = $"[center]Confirm you wish to discard these {_selectedCardsAsText.Count} cards[/center]";
                }
                else 
                { 
                    instructions.Text = $"[center]Confirm you wish to discard these {_selectedCardsAsText.Count} cards for ${_selectedCardsCostPerDiscard * _selectedCardsAsText.Count:F2}[/center]";
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
            else if (_potentialBetValues != null)
            {
                npr.Texture = NineGridButton_Hover;
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
        //GD.Print($"Starting sleep for bet selection");
        while (true)
        {
            if (_betValue < 0)
            {
                await Task.Run(() => Thread.Sleep(10));
            }
            else
            {
                //GD.Print($"Ending sleep for bet selection");
                return _betValue;
            }
        }
    }

    internal void EnableBetSlider(double betFloor)
    {
        const int initValue = 1;

        _betValue = -1;
        _potentialBetValues = new List<double>() { 0, betFloor };
        _potentialBetValues.AddRange(Player.GetNextBets(betFloor, 8));

        if (PlayPage.FindChild("BetSlider") is Slider range)
        {
            range.Value = initValue;
            range.Show();
        }

        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
        {
            UpdateConfirmationButtonText_Bet(confirmationButton, initValue);
            confirmationButton.Show();
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

        // Don't bother to actually remove the check, it can remain until we get rid of the card.
        //if (FindChild($"Hand{positionID}") is VisibleHand hand)
        //{
        //    hand.DisableCardSelection(CardSelectionGuiHandler);
        //}

        if (PlayPage.FindChild("ConfirmationButton") is NinePatchRect confirmationButton)
        {
            confirmationButton.Hide();
        }
    }

    internal void SetAchievmentsAndUnlocks(AchievementUnlock[] achievementUnlocks, Tuple<Species, float>[] speciesUnlocks)
    {
        if (VisibleAchievement == null)
            throw new Exception($"VisibleAchievement value not set for {Name}");

        if (AchievementsPage.FindChild("AchievementBox") is Control achievementBox)
        {
            foreach (var child in achievementBox.GetChildren())
                achievementBox.RemoveChild(child);

            foreach (AchievementUnlock achUnlock in achievementUnlocks)
            {
                Node vAch = VisibleAchievement.Instantiate();
                ConfigureAchievementBox(achUnlock, vAch);

                achievementBox.AddChild(vAch);
            }

            achievementBox.UpdateMinimumSize();
            achievementBox.ResetSize();
        }

        if (AchievementsPage.FindChild("UnlocksBox") is Control unlocksBox)
        {
            foreach (var child in unlocksBox.GetChildren())
                unlocksBox.RemoveChild(child);

            Label unlockedLabel = new Label();
            unlockedLabel.Theme = unlocksBox.Theme;
            Label inProgressLabel = new Label();
            inProgressLabel.Theme = unlocksBox.Theme;
            int unknownCount = 0;

            foreach (var speciesUnlock in speciesUnlocks)
            {
                if (speciesUnlock.Item2 == 0)
                    unknownCount += 1;
                else if (speciesUnlock.Item2 >= 1)
                {
                    if (string.IsNullOrEmpty(unlockedLabel.Text))
                        unlockedLabel.Text = $"Unlocked: {speciesUnlock.Item1.Name}";
                    else
                        unlockedLabel.Text += $", {speciesUnlock.Item1.Name}";
                }
                else
                {
                    if (string.IsNullOrEmpty(inProgressLabel.Text))
                        inProgressLabel.Text = $"In Progress: {speciesUnlock.Item1.Name} {Math.Round(speciesUnlock.Item2 * 100)}%";
                    else
                        inProgressLabel.Text += $", {speciesUnlock.Item1.Name} {Math.Round(speciesUnlock.Item2 * 100)}%";
                }
            }

            if (!string.IsNullOrEmpty(unlockedLabel.Text))
                unlocksBox.AddChild(unlockedLabel);
            if (!string.IsNullOrEmpty(inProgressLabel.Text))
                unlocksBox.AddChild(inProgressLabel);
            if (unknownCount > 0)
            {
                Label unknownLabel = new Label();
                unknownLabel.Text = $"{unknownCount} x Unknown";
                unknownLabel.Theme = unlocksBox.Theme;
                unlocksBox.AddChild(unknownLabel);
            }
        }
    }

    private void ConfigureAchievementBox(AchievementUnlock unlock, Node visibileAchievementNode)
    {
        if (visibileAchievementNode.FindChild("Image") is TextureRect imageRect)
        {
            if (unlock.IsBronze)
                imageRect.Texture = BronzeAchievement;
            else if (unlock.IsSilver)
                imageRect.Texture = SilverAchievement;
            else if (unlock.IsGold)
                imageRect.Texture = GoldAchievement;
            else if (unlock.IsSpeciesUnlock)
                imageRect.Texture = SpeciesUnlockAchievement;
            else
                throw new Exception($"Bad achievement level for {unlock}");
        }

        if (visibileAchievementNode.FindChild("Text") is RichTextLabel richText)
        {
            richText.Text = unlock.Text;
        }
    }

    private double _popUpWait = 0;
    private List<AchievementUnlock> _popUpUnlocks = new List<AchievementUnlock>();
    internal void ShowAchievementPopUp(AchievementUnlock postUnlock)
    {
        lock (_popUpUnlocks)
        {
            _popUpUnlocks.Add(postUnlock);
        }
    }

    private void ShowNextPopUpAchievement()
    {
        // 868, -59
        // 868, 8
        if (FindChild("PopUpAchievement") is Control visiblePopUpAchievement)
        {
            visiblePopUpAchievement.Show();
            visiblePopUpAchievement.Position = new Vector2(868, -59);
            ConfigureAchievementBox(_popUpUnlocks[0], visiblePopUpAchievement);
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(visiblePopUpAchievement, "position", new Vector2(868, 8), 1);
            tween.TweenInterval(3);
            tween.TweenProperty(visiblePopUpAchievement, "position", new Vector2(868, -59), 1);
        }
    }

    internal void SetSelectSpecies(Species[] species)
    {
        if (SpeciesSelectButton == null)
            throw new Exception("Species Select Button not defined for HUD");

        if (PlayAsNewSpeciesPage.FindChild("ButtonContainer") is GridContainer buttonContainer)
        {
            foreach (Node? child in buttonContainer.GetChildren())
            {
                if (child != null)
                    buttonContainer.RemoveChild(child);
            }

            foreach (Species sp in species)
            {
                if (SpeciesSelectButton.Instantiate() is StateChangeButton newButton)
                {
                    Label label = new Label();
                    label.SetAnchorsPreset(Control.LayoutPreset.Center);
                    label.Theme = buttonContainer.Theme;
                    label.Text = sp.Name;
                    label.ResetSize();
                    label.CustomMinimumSize = label.Size;
                    newButton.CustomMinimumSize = label.Size + new Vector2(80, 20);
                    newButton.State = "ChangeSpecies";
                    label.Position = Vector2.Zero - (new Vector2(8, 7) + (label.Size / 2));
                    newButton.Initialize(StateMachine, sp);
                    newButton.AddChild(label);
                    buttonContainer.AddChild(newButton);
                }
            }

            buttonContainer.ResetSize();
        }
    }
}
