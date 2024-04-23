using System;
using System.Collections.Generic;

class Player
{
    internal int PositionID { get; private set; }
    internal bool IsNPC { get; private set; }
    internal bool HasDiscarded { get; set; }
    internal bool HasFolded { get; private set; }
    internal double AmountBet { get; private set; }
    internal bool HasRevealed { get; set; }
    internal List<Card>? Discards { get; set; }

    internal Player(int positionID)
    {
        PositionID = positionID;
        IsNPC = positionID > 0;
        HasDiscarded = false;
        HasFolded = false;
        HasRevealed = false;
        AmountBet = 0;
        Discards = null;
    }

    internal void Fold(HUD hud)
    {
        HasFolded = true;
        hud.FoldHand(PositionID);
    }

    internal void Bet(HUD hud, double amount, double currentAmount)
    {
        // TODO: This should be governed by race or personality
        const double paddingAmount = 1;

        double amountWeAreWillingToGoTo = Math.Round(paddingAmount + amount * 10.0) / 10.0;
        if (amountWeAreWillingToGoTo < currentAmount)
        {
            Fold(hud);
        }
        else
        {
            AmountBet = amountWeAreWillingToGoTo;
            hud.SetBetAmount(PositionID, AmountBet, null);
        }
    }
}
