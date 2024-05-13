using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable

public partial class VisibleHand : Node2D
{
    [Export]
    public PackedScene? _visibleCardScene = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void OnContainerResized()
    {
        if (FindChild("ColorRect") is ColorRect cr)
        {
            if (FindChild("Container") is Control container)
            {
                cr.Position = container.Position;
                cr.Size = container.Size;
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child ColorRect");
        }
    }

    private static Label GetCardLabel(Control visibleCard)
    {
        if (visibleCard.FindChild("Label", recursive: false) is Label retVal)
            return retVal;

        throw new Exception($"Visible Card {visibleCard.Name} has no child Label");
    }

    private static TextureRect GetCardBackside(Control visibleCard)
    {
        if (visibleCard.FindChild("Backside", recursive: false) is TextureRect retVal)
            return retVal;

        throw new Exception($"Visible Card {visibleCard.Name} has no child Backside");
    }

    private static TextureRect GetCardVisibiltityPerPosition(Control visibleCard, int positionID)
    {
        string nodeName = $"SeenByPlayer{positionID}";
        if (visibleCard.FindChild(nodeName, recursive: false) is TextureRect retVal)
            return retVal;

        throw new Exception($"Visible Card {visibleCard.Name} has no child {nodeName}");
    }

    private static TextureRect GetCardVisibiltityAll(Control visibleCard)
    {
        string nodeName = $"SeenByPlayerBig";
        if (visibleCard.FindChild(nodeName, recursive: false) is TextureRect retVal)
            return retVal;

        throw new Exception($"Visible Card {visibleCard.Name} has no child {nodeName}");
    }

    internal static CanvasItem GetCardSelectionMark(Control visibleCard)
    {
        if (visibleCard.FindChild("SelectionMark", recursive: false) is CanvasItem retVal)
            return retVal;

        throw new Exception($"Visible Card {visibleCard.Name} has no child SelectionMark");
    }

    private static void ConfigureCardExposure(Control visibleCard, bool exposed, Card card)
    {
        if (visibleCard == null)
            return;

        Label cardLabel = GetCardLabel(visibleCard);
        cardLabel.Visible = exposed;
        cardLabel.Text = card.ToString();

        TextureRect cardBack = GetCardBackside(visibleCard);
        cardBack.Visible = !exposed;

        //visibleCard.UpdateMinimumSize();
        //visibleCard.ResetSize();
    }

    //internal void ExposeNonNPCCardToNPC(Card card, int[] positionIDs)
    //{
    //    if (FindChild("Cards") is Control cards)
    //    {
    //        Godot.Collections.Array<Node> children = cards.GetChildren();
    //        foreach (Node child in children)
    //        {
    //            if (child is Control visibleCard)
    //            {
    //                Label cardLabel = GetCardLabel(visibleCard);
    //                if (cardLabel.Text == card.ToString() && cardLabel.Visible == true)
    //                {
    //                    TextureRect seenByNPC = GetCardSeenByPlayer(visibleCard, positionID);
    //                    //seenByOthers.SizeFlagsHorizontal = Control.SizeFlags.Fill;
    //                    //seenByOthers.SizeFlagsVertical = Control.SizeFlags.Fill;
    //                    //seenByOthers.UpdateMinimumSize();
    //                    seenByNPC.Show();
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        throw new Exception($"{Name} does not have a child Cards");
    //    }
    //}

    internal void FineTuneVisibility(Card card, int[] playersWhoCanSeeOtherThanOwnerAndNonNPC)
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                if (child is Control visibleCard)
                {
                    Label cardLabel = GetCardLabel(visibleCard);
                    if (cardLabel.Text == card.ToString() && cardLabel.Visible == true)
                    {
                        FineTuneVisibility(visibleCard, playersWhoCanSeeOtherThanOwnerAndNonNPC);
                    }
                }
            }
        }
    }

    private static void BlinkCard(CanvasItem ci)
    {
        Tween tween = ci.GetTree().CreateTween().SetLoops();
        tween.TweenProperty(ci, "modulate:a", 0f, 1.0f);
        tween.TweenProperty(ci, "modulate:a", 1f, 1.0f);
    }

    private static void FineTuneVisibility(Control visibleCard, int[] playersWhoCanSeeOtherThanOwnerAndNonNPC)
    {
        if (playersWhoCanSeeOtherThanOwnerAndNonNPC.IsEmpty())
        {
            ; // Do nothing, just leave this card without any observation marks
        }
        else if (playersWhoCanSeeOtherThanOwnerAndNonNPC.Count() >= 3)
        {
            TextureRect bigEyeIcon = GetCardVisibiltityAll(visibleCard);
            bigEyeIcon.Show();
            BlinkCard(bigEyeIcon);

            for (int j = 1; j< 5; ++j)
            {
                GetCardVisibiltityPerPosition(visibleCard, j).Hide();
            }
        }
        else
        {
            if (GetCardVisibiltityAll(visibleCard).Visible == false)
            {
                foreach (int positionID in playersWhoCanSeeOtherThanOwnerAndNonNPC)
                {
                    TextureRect smallEyeIcon = GetCardVisibiltityPerPosition(visibleCard, positionID);
                    smallEyeIcon.Show();
                    BlinkCard(smallEyeIcon);
                }
            }
        }
    }

    internal void ExposeCardToNonNPC(Card card)
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                if (child is Control visibleCard)
                {
                    Label cardLabel = GetCardLabel(visibleCard);
                    if (cardLabel.Text == card.ToString() && cardLabel.Visible == false)
                    {
                        TextureRect cardBack = GetCardBackside(visibleCard);
                        cardBack.Hide();
                        cardLabel.Show();
                        cardLabel.ResetSize();
                    }
                }
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child Cards");
        }
    }

    internal void ResetDisplay()
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                cards.RemoveChild(child);
            }
        }

        if (FindChild("Discards") is BoxContainer discardBox)
        {
            Godot.Collections.Array<Node> children = discardBox.GetChildren();
            foreach (Node child in children)
            {
                discardBox.RemoveChild(child);
            }
        }

        if (FindChild("Score") is Label scoreLabel)
        {
            scoreLabel.Hide();
        }

        if (FindChild("PlayerInfo") is BoxContainer infoBox)
        {
            Godot.Collections.Array<Node> children = infoBox.GetChildren();
            foreach (Node child in children)
            {
                infoBox.RemoveChild(child);
            }
        }

        if (FindChild("Text") is Label text)
        {
            text.Text = "";
        }

        if (FindChild("ColorRect") is ColorRect cr)
        {
            cr.Color = Color.FromHtml("277714");
        }
    }

    internal void HideRiver()
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                cards.RemoveChild(child);
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child Cards");
        }

        if (FindChild("Text") is Label infoBox)
        {
            infoBox.Text = "";
        }
        else
        {
            throw new Exception($"{Name} does not have a child Text");
        }
    }

    internal void UpdateRiver(List<Card> river)
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                cards.RemoveChild(child);
            }

            if (_visibleCardScene != null)
            {
                foreach (Card card in river)
                {
                    if (_visibleCardScene.Instantiate() is Control visibleCard)
                    {
                        //visibleCard.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                        cards.AddChild(visibleCard);
                        ConfigureCardExposure(visibleCard, exposed: true, card);
                    }
                }
            }
            else
            {
                throw new Exception("No visible card defined for visible hand");
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child Cards");
        }

        if (FindChild("Text") is Label infoBox)
        {
            infoBox.Text = "River";
        }
        else
        {
            throw new Exception($"{Name} does not have a child Text");
        }
    }

    internal void Update(Hand hand, Player nonNPCPlayer)
    {
		if (FindChild("Cards") is Control cards)
		{
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
                cards.RemoveChild(child);
            
            if (_visibleCardScene != null)
            {
                foreach (Card card in hand._cards)
                {
                    if (_visibleCardScene.Instantiate() is Control visibleCard)
                    {
                        //visibleCard.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                        cards.AddChild(visibleCard);
                        bool exposedToNonNPC = hand.IsVisible(card, nonNPCPlayer);
                        ConfigureCardExposure(visibleCard, exposedToNonNPC, card);
                        if (exposedToNonNPC)
                        {
                            int[] playersWhoCanSeeOtherThanOwnerAndNonNPC =
                                hand.ObserversOtherThanOwnerAndNonNPC(card, nonNPCPlayer.PositionID).ToArray();
                            FineTuneVisibility(visibleCard, playersWhoCanSeeOtherThanOwnerAndNonNPC);
                        }
                    }
                }

                //if (hand.PositionID == nonNPCPlayer.PositionID)
                //{
                //    foreach (Card card in hand._cards)
                //    {
                //        if (hand.IsCardVisibleToEntireTable(card))
                //        {
                //            // Do nothing, the card should be visible to everyone
                //            GD.Print($"MAKE SURE {card} in hand #{hand.PositionID} hand is visible");
                //        }
                //        else if (hand.IsVisible(card, nonNPCPlayer) && hand.Player.PositionID != nonNPCPlayer.PositionID)
                //        {
                //            ExposeNonNPCCardToNPC(card, 0);
                //        }
                //        else
                //        {
                //            hand.ForEachNPCThatCanViewCard(card, npcPositionId =>
                //            {
                //                ExposeNonNPCCardToNPC(card, npcPositionId);
                //            });
                //        }
                //    }
                //}
            }
            else
            {
                throw new Exception("No visible card defined for visible hand");
            }
		}
		else
		{
			throw new Exception($"{Name} does not have a child Cards");
        }
    }

    internal void AddDiscard(Card discard, List<int> playersWhoCanSeeThisDiscard, int handPositionId, int nonNPCPositionID)
    {
        if (FindChild("Discards") is BoxContainer discardBox)
        {
            if (_visibleCardScene != null)
            {
                if (discardBox.GetChildCount() == 0)
                {
                    Label discardtext = new Label();
                    discardtext.Text = "Discard: ";
                    discardtext.HorizontalAlignment = HorizontalAlignment.Right;
                    discardtext.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                    discardBox.AddChild(discardtext);
                }
                
                if (_visibleCardScene.Instantiate() is Control visibleCard)
                {
                    //visibleCard.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                    discardBox.AddChild(visibleCard);
                    bool isVisibleToNonNPC = playersWhoCanSeeThisDiscard.Contains(nonNPCPositionID);
                    ConfigureCardExposure(visibleCard, isVisibleToNonNPC, discard);
                    if (isVisibleToNonNPC)
                    {
                        int[] playersWhoCanSeeOtherThanOwnerAndNonNPC = playersWhoCanSeeThisDiscard.Where(a => a != handPositionId && a != nonNPCPositionID).ToArray();
                        FineTuneVisibility(visibleCard, playersWhoCanSeeOtherThanOwnerAndNonNPC);
                    }
                }
            }
            else
            {
                throw new Exception("No visible card defined for visible hand");
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child Discards");
        }
    }

    internal void SetPlayerInfo(string name, Species species)
    {
        if (FindChild("PlayerInfo") is BoxContainer infoBox)
        {
            if (infoBox.GetChildCount() == 0)
            {
                Label nameText = new Label();
                nameText.Text = name;
                nameText.HorizontalAlignment = HorizontalAlignment.Left;
                nameText.VerticalAlignment = VerticalAlignment.Bottom;
                nameText.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                infoBox.AddChild(nameText);

                Label speciesText = new Label();
                speciesText.Text = species.Name;
                speciesText.HorizontalAlignment = HorizontalAlignment.Right;
                speciesText.VerticalAlignment = VerticalAlignment.Bottom;
                speciesText.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                infoBox.AddChild(speciesText);
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child PlayerInfo");
        }
    }

    internal void SetPot(double amount)
    {
        if (FindChild("Text") is Label scoreLabel)
        {
            scoreLabel.Text = (amount > 0) ? $"Pot: ${amount:F2}" : "";
            scoreLabel.Show();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }
    }

    internal void Ante(double amountBet)
    {
        if (FindChild("Score") is Label scoreLabel)
        {
            scoreLabel.Text = (amountBet > 0) ? $"Ante: ${amountBet:F2}" : "Fold";
            scoreLabel.Show();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }
    }

    internal void FoldHand(double amountBet)
    {
        if (FindChild("Score") is Label scoreLabel)
        {
            scoreLabel.Text = (amountBet > 0) ? $"Fold: ${amountBet:F2}": "Fold";
            scoreLabel.Show();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }

        if (FindChild("ColorRect") is ColorRect cr)
        {
            cr.Color = Color.FromString("#770b21", Color.Color8(128,128,128));
        }
        else
        {
            throw new Exception($"{Name} does not have a child ColorRect");
        }
    }

    internal void SetFeltToLost()
    {
        if (FindChild("ColorRect") is ColorRect cr)
        {
            cr.Color = Color.FromString("#770b21", Color.Color8(128, 128, 128));
        }
        else
        {
            throw new Exception($"{Name} does not have a child ColorRect");
        }
    }

    internal void SetBetAmount(double amountBet, string? description)
    {
        if (FindChild("Score") is Label scoreLabel)
        {
            if (string.IsNullOrEmpty(description))
                scoreLabel.Text = $"${amountBet:F2}";
            else
                scoreLabel.Text = $"${amountBet:F2} {description}";

            scoreLabel.Show();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }
    }

    internal void PlayerLeaves(string leavingText)
    {
        if (FindChild("ColorRect") is ColorRect cr)
        {
            cr.Color = Color.FromHtml("147754");
            //cr.Color = Color.FromHtml("277714");
        }
        else
        {
            throw new Exception($"{Name} does not have a child ColorRect");
        }

        if (FindChild("Score") is Label scoreLabel)
        {
            scoreLabel.Text = $"\"{leavingText}\"";
            scoreLabel.Show();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }

        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
                cards.RemoveChild(child);
        }

        if (FindChild("Discards") is BoxContainer discardBox)
        {
            Godot.Collections.Array<Node> children = discardBox.GetChildren();
            foreach (Node child in children)
            {
                discardBox.RemoveChild(child);
            }
        }
    }

    internal void FlingCard(Card card, int cardIndex, Random rnd, bool isVisible, double delay, VisibleHand toHand)
    {
        if (_visibleCardScene == null)
        {
            throw new Exception("No visible card defined for visible hand");
        }

        if (_visibleCardScene.Instantiate() is Control visibleCard)
        {
            Vector2 launchPos = GlobalPosition;
            if (FindChild("Cards") is Control cards)
            {
                if (cards.GetChild(cardIndex) is Control fc)
                {
                    launchPos = fc.GlobalPosition;
                }
                else
                {
                    GD.Print($"{Name}.Cards[0] is not a Control");
                }
            }
            else
            {
                GD.Print($"{Name} has no child Cards");
            }

            visibleCard.Position = Vector2.Zero;
            ConfigureCardExposure(visibleCard, isVisible, card);
            visibleCard.Show();
            visibleCard.TopLevel = true;
            this.AddChild(visibleCard);
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(visibleCard, "position", launchPos, 0);
            tween.TweenProperty(visibleCard, "position", toHand.GlobalPosition, delay);
            tween.Parallel().TweenProperty(visibleCard, "rotation", (rnd.NextDouble() * 0.5 + 0.75) * Math.Tau, delay);
            tween.TweenCallback(Callable.From(visibleCard.QueueFree));
        }
    }

    internal void EnableCardSelection(Action<InputEvent, Control> cardSelectionGuiHandler)
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                if (child is Control fc)
                {
                    GetCardSelectionMark(fc).Hide();
                    fc.GuiInput += (a) => cardSelectionGuiHandler(a, fc);
                }
            }
        }
    }

    internal void DisableCardSelection(Action<InputEvent, Control> cardSelectionGuiHandler)
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                if (child is Control fc)
                {
                    GetCardSelectionMark(fc).Hide();
                }
            }
        }
    }

    internal void ClearEyeIcons()
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                if (child is Control visibleCard)
                {
                    GetCardVisibiltityAll(visibleCard).Hide();
                    for (int j = 1; j < 5; ++j)
                    {
                        GetCardVisibiltityPerPosition(visibleCard, j).Hide();
                    }
                }
            }
        }
    }
}
