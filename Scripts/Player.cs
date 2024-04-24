using Godot;
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
        hud.FoldHand(PositionID, AmountBet);
    }

    internal void Bet(HUD hud, double amountWeAreWillingToGoTo)
    {
        AmountBet = amountWeAreWillingToGoTo;
        hud.SetBetAmount(PositionID, AmountBet, null);
    }

    internal void ForceBetOrFold(HUD hud, Hand hand, double percent, double betFloor)
    {
        AggregateValue agValue = new AggregateValue(this, hand);
        if (agValue._normalizedWealth <= 0)
        {
            throw new Exception($"Why is player #{hand.PositionID}'s aggregate value {agValue.GetDesc()}");
        }

        double willingToGoAsFarAs = Math.Sqrt(agValue._normalizedWealth);

        string handTxt = hand._handValue.ToString() ?? "??";
        if (willingToGoAsFarAs < betFloor)
        {
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, Folds as ${willingToGoAsFarAs:F2} < ${betFloor:F2}");
            Fold(hud);
            return;
        }

        const double raiseAmount = 1;
        double amountWedHaveToAdd = AmountBet - betFloor;
        double comfortZoneStartsAt = willingToGoAsFarAs * percent / 100.0;
        if (comfortZoneStartsAt < betFloor)
        {
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (past our comfort zone) ${comfortZoneStartsAt:F2} < ${betFloor:F2}");
            Bet(hud, betFloor);
        }
        else if (comfortZoneStartsAt - amountWedHaveToAdd < betFloor)
        {
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (not willing to risk it) ${comfortZoneStartsAt:F2} < ${betFloor:F2}");
            Bet(hud, betFloor);
        }
        else if (willingToGoAsFarAs < betFloor + raiseAmount)
        {
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (would be a step too far) ${willingToGoAsFarAs:F2} < ${betFloor + raiseAmount:F2}");
            Bet(hud, betFloor);
        }
        else
        {
            // We have a decent hand, and we think we can win
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, raises as ${comfortZoneStartsAt:F2} => ${betFloor:F2} <= ${willingToGoAsFarAs:F2}");
            Bet(hud, betFloor + raiseAmount);
        }

        //// TODO: This should be governed by race or personality
        //double howMuchToBet = 4.0 - ((100 - percent) / 25.0);
        //howMuchToBet = 10.0 * howMuchToBet * howMuchToBet / 16.0;

        //const double paddingAmount = 1;

        //double amountWeAreWillingToGoTo = Math.Round(paddingAmount + howMuchToBet * 10.0) / 10.0;
        //if (amountWeAreWillingToGoTo < betFloor)
        //{
        //    Fold(hud);
        //}
        //else
        //{
        //    Bet(hud, amountWeAreWillingToGoTo);
        //}
    }
}
