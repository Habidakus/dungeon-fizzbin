using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class VisibleHand : Node2D
{
    [Export]
    public PackedScene _visibleCard = null;

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

    internal void Update(Hand hand)
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
                    Control visibleCard = _visibleCard.Instantiate() as Control;
                    if (visibleCard != null)
                    {
                        cards.AddChild(visibleCard);

                        if (visibleCard.GetChild(0) is Label cardLabel)
                        {
                            cardLabel.Visible = hand.IsVisible(card);
                            cardLabel.Text = card.ToString();
                            cardLabel.ResetSize();
                            //cardLabel.Position = Vector2.Zero;
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
                            cardBack.Visible = !hand.IsVisible(card);
                            if (cardBack.Visible)
                                visibleCard.CustomMinimumSize = cardBack.Size + new Vector2(4, 4);
                            //GD.Print($"9Rect={visibleCard.Size} Label={cardLabel.Size}");
                        }
                        else
                        {
                            throw new Exception($"VisibleCard {visibleCard} has no child label");
                        }

                        visibleCard.UpdateMinimumSize();
                        visibleCard.ResetSize();
                    }
                }
            }
            else
            {
                throw new Exception("No visible card defined for visible hand");
            }
            //cardsLabel.Text = hand.CardsAsString();
		}
		else
		{
			throw new Exception($"{Name} does not have a child Cards");
        }

        if (FindChild("Score") is Label scoreLabel)
        {
            if (hand.IsEveryCardVisible)
            {
                scoreLabel.Text = hand.ScoreAsString();
                scoreLabel.Show();
            }
            else
            {
                scoreLabel.Hide();
            }
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }

        //if (FindChild("Discards") is Label discardLabel)
        //{
        //    discardLabel.Text = "";
        //}
        //else
        //{
        //    throw new Exception($"{Name} does not have a child Score");
        //}
    }

    internal void AddDiscard(Card discard)
    {
        if (FindChild("Discards") is Label discardLabel)
        {
            StringBuilder sb = new StringBuilder();

            if (discardLabel.Text.Length == 0)
                sb.Append("Discard:");
            else
                sb.Append(discardLabel.Text);

            sb.Append(' ');
            sb.Append(discard);
            discardLabel.Text = sb.ToString();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }
    }
}
