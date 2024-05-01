using Godot;
using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

public partial class VisibleHand : Node2D
{
    [Export]
    public PackedScene? _visibleCard = null;

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

    private void AddCardToControl(Control visibleCard, bool exposed, Card card)
    {
        if (visibleCard == null)
            return;

        if (visibleCard.GetChild(0) is Label cardLabel)
        {
            cardLabel.Visible = exposed;
            cardLabel.Text = card.ToString();
            cardLabel.ResetSize();
            if (cardLabel.Visible)
            {
                visibleCard.CustomMinimumSize = cardLabel.Size + new Vector2(24, 12);
                //GD.Print($"9Rect.size={visibleCard.Size} 9Rect.pos={visibleCard.Position} Label.size={cardLabel.Size} Label.pos={cardLabel.Position}");
            }
        }
        else
        {
            throw new Exception($"VisibleCard {visibleCard} has no child label");
        }

        if (visibleCard.GetChild(1) is TextureRect cardBack)
        {
            cardBack.Visible = !exposed;
            if (cardBack.Visible)
                visibleCard.CustomMinimumSize = cardBack.Size + new Vector2(4, 4);
        }
        else
        {
            throw new Exception($"VisibleCard {visibleCard} has no child label");
        }

        visibleCard.UpdateMinimumSize();
        visibleCard.ResetSize();
    }

    internal void ExposeNonNPCCardToNPC(Card card)
    {
        if (FindChild("Cards") is Control cards)
        {
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
            {
                if (child is Control visibleCard)
                {
                    if (visibleCard.GetChild(0) is Label cardLabel)
                    {
                        if (cardLabel.Text == card.ToString() && cardLabel.Visible == true)
                        {
                            if (visibleCard.GetChild(2) is Control seenByOthers)
                            {
                                //seenByOthers.Position = visibleCard.Position;
                                seenByOthers.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                                seenByOthers.SizeFlagsVertical = Control.SizeFlags.Fill;
                                seenByOthers.UpdateMinimumSize();
                                seenByOthers.Show();
                            }
                            //cardLabel.TextDirection = Control.TextDirection.Rtl;
                        }
                    }
                }
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child Cards");
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
                    if (visibleCard.GetChild(0) is Label cardLabel)
                    {
                        if (cardLabel.Text == card.ToString() && cardLabel.Visible == false)
                        {
                            if (visibleCard.GetChild(1) is TextureRect cardBack)
                            {
                                //cardLabel.Visible = true;
                                //cardBack.Visible = false;
                                cardBack.Hide();
                                cardLabel.Show();
                                cardLabel.ResetSize();
                                if (cardLabel.Visible)
                                {
                                    visibleCard.CustomMinimumSize = cardLabel.Size + new Vector2(24, 12);
                                    //GD.Print($"9Rect.size={visibleCard.Size} 9Rect.pos={visibleCard.Position} Label.size={cardLabel.Size} Label.pos={cardLabel.Position}");
                                }
                            }
                        }
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

        if (FindChild("ColorRect") is ColorRect cr)
        {
            cr.Color = Color.FromHtml("147754");
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

            if (_visibleCard != null)
            {
                foreach (Card card in river)
                {
                    if (_visibleCard.Instantiate() is Control visibleCard)
                    {
                        visibleCard.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                        cards.AddChild(visibleCard);
                        AddCardToControl(visibleCard, exposed: true, card);
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

        if (FindChild("PlayerInfo") is BoxContainer infoBox)
        {
            if (infoBox.GetChildCount() == 0)
            {
                Label nameText = new Label();
                nameText.Text = "River";
                nameText.HorizontalAlignment = HorizontalAlignment.Center;
                nameText.VerticalAlignment = VerticalAlignment.Center;
                nameText.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                infoBox.AddChild(nameText);
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child PlayerInfo");
        }
    }

    internal void Update(Hand hand, Player nonNPCPlayer)
    {
		if (FindChild("Cards") is Control cards)
		{
            Godot.Collections.Array<Node> children = cards.GetChildren();
            foreach (Node child in children)
                cards.RemoveChild(child);
            
            if (_visibleCard != null)
            {
                foreach (Card card in hand._cards)
                {
                    if (_visibleCard.Instantiate() is Control visibleCard)
                    {
                        visibleCard.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                        cards.AddChild(visibleCard);
                        AddCardToControl(visibleCard, hand.IsVisible(card, nonNPCPlayer), card);
                    }
                }

                if (hand.PositionID == nonNPCPlayer.PositionID)
                {
                    foreach (Card card in hand._cards)
                    {
                        if (hand.IsVisibleToAnyoneElse(card, nonNPCPlayer.PositionID))
                        {
                            ExposeNonNPCCardToNPC(card);
                        }
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
    }

    internal void AddDiscard(Card discard, bool isVisibleToNonNPC)
    {
        if (FindChild("Discards") is BoxContainer discardBox)
        {
            if (_visibleCard != null)
            {
                if (discardBox.GetChildCount() == 0)
                {
                    Label discardtext = new Label();
                    discardtext.Text = "Discard: ";
                    discardtext.HorizontalAlignment = HorizontalAlignment.Right;
                    discardtext.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                    discardBox.AddChild(discardtext);
                }
                
                if (_visibleCard.Instantiate() is Control visibleCard)
                {
                    visibleCard.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                    discardBox.AddChild(visibleCard);
                    AddCardToControl(visibleCard, isVisibleToNonNPC, discard);
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
        if (FindChild("Score") is Label scoreLabel)
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

    internal void ClearHighlight(string why)
    {
        if (FindChild("ColorRect") is ColorRect cr)
        {
            //GD.Print($"{DateTime.Now.Second}.{DateTime.Now.Millisecond} {Name} removing highlight. Why={why}");
            cr.Color = Color.FromHtml("147754");
        }
        else
        {
            throw new Exception($"{Name} does not have a child ColorRect");
        }
    }

    internal void SetHighlight()
    {
        if (FindChild("ColorRect") is ColorRect cr)
        {
            //GD.Print($"{DateTime.Now.Second}.{DateTime.Now.Millisecond} {Name} adding highlight");
            cr.Color = Color.FromHtml("277714");
        }
        else
        {
            throw new Exception($"{Name} does not have a child ColorRect");
        }
    }
}
