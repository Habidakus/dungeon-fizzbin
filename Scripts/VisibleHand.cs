using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class VisibleHand : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    internal void Update(Hand hand)
    {
		if (FindChild("Cards") is Label cardsLabel)
		{
            cardsLabel.Text = hand.CardsAsString();
		}
		else
		{
			throw new Exception($"{Name} does not have a child Cards");
        }

        if (FindChild("Score") is Label scoreLabel)
        {
            scoreLabel.Text = hand.ScoreAsString();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }

        if (FindChild("Discards") is Label discardLabel)
        {
            discardLabel.Text = "";
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }
    }

    internal void SetDiscards(List<Card> discards)
    {
        if (FindChild("Discards") is Label discardLabel)
        {
            StringBuilder sb = new StringBuilder("Discard: ");
            for (int i = 0; i < discards.Count; ++i)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(discards[i].ToString());
            }

            discardLabel.Text = sb.ToString();
        }
        else
        {
            throw new Exception($"{Name} does not have a child Score");
        }
    }
}
