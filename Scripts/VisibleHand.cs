using Godot;
using System;
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
}
