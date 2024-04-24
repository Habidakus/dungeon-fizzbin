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

    internal double Ante(HUD hud, double anteAmount)
    {
        AmountBet = anteAmount;
        hud.Ante(PositionID, AmountBet);
        return anteAmount;
    }

    internal void Fold(HUD hud)
    {
        HasFolded = true;
        hud.FoldHand(PositionID, AmountBet);
    }

    internal void Bet(HUD hud, double betAmount)
    {
        AmountBet = betAmount;
        hud.SetBetAmount(PositionID, AmountBet, null);
    }

    internal double ForceBetOrFold(HUD hud, Hand hand, double percent, double betFloor, bool canStopTheRoundByMatching, int bettingRound)
    {
        AggregateValue agValue = new AggregateValue(this, hand);
        if (agValue._normalizedWealth <= 0)
        {
            throw new Exception($"Why is player #{hand.PositionID}'s aggregate value {agValue.GetDesc()}");
        }

        double willingToGoAsFarAs = 1.75 * Math.Sqrt(agValue._normalizedWealth);
        double amountWedHaveToAdd = betFloor - AmountBet;

        string handTxt = hand._handValue.ToString() ?? "??";
        if (willingToGoAsFarAs < betFloor)
        {
            if (canStopTheRoundByMatching && amountWedHaveToAdd <= AmountBet)
            {
                // if we've put in a lot, and we can stop the betting round with us if we just don't raise, then we should match instead of folding.
                GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt} ... and would quite but they can stop the bidding here.");
                Bet(hud, betFloor);
                return amountWedHaveToAdd;
            }
            else
            {
                GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, Folds as ${willingToGoAsFarAs:F2} < ${betFloor:F2} and ${amountWedHaveToAdd:F2} is too much");
                Fold(hud);
                return 0;
            }
        }

        double comfortZoneStartsAt = willingToGoAsFarAs * percent / 100.0;
        if (comfortZoneStartsAt < betFloor)
        {
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (past our comfort zone) ${comfortZoneStartsAt:F2} < ${betFloor:F2} <= ${willingToGoAsFarAs:F2}");
            Bet(hud, betFloor);
            return amountWedHaveToAdd;
        }

        if (comfortZoneStartsAt - amountWedHaveToAdd < betFloor)
        {
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (not willing to risk it) ${comfortZoneStartsAt - amountWedHaveToAdd:F2} < ${betFloor:F2}");
            Bet(hud, betFloor);
            return amountWedHaveToAdd;
        }

        double raiseAmount = DetermineRaiseAmount(betFloor, willingToGoAsFarAs, bettingRound);

        if (willingToGoAsFarAs < betFloor + raiseAmount)
        {
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (would be a step too far) ${willingToGoAsFarAs:F2} < ${betFloor + raiseAmount:F2}");
            Bet(hud, betFloor);
            return amountWedHaveToAdd;
        }
        else
        {
            // We have a decent hand, and we think we can win
            GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, raises from ${betFloor:F2} to ${betFloor + raiseAmount:F2}: as ${comfortZoneStartsAt:F2} => ${betFloor:F2} <= ${willingToGoAsFarAs:F2}");
            Bet(hud, betFloor + raiseAmount);
            return amountWedHaveToAdd + raiseAmount;
        }
    }

    private static double DetermineRaiseAmount(double betFloor, double willingToGoAsFarAs, int bettingRound)
    {
        int roundsLeft = Math.Max(1, 10 - bettingRound);
        double raiseAmount = (willingToGoAsFarAs - betFloor) / (roundsLeft * roundsLeft);
        if (raiseAmount + betFloor < 5)
        {
            raiseAmount = Math.Round(raiseAmount * 4) / 4;
            raiseAmount = Math.Max(raiseAmount, 0.25);
        }
        else if (raiseAmount + betFloor < 20)
        {
            raiseAmount = Math.Round(betFloor + raiseAmount) - (betFloor);
            raiseAmount = Math.Max(raiseAmount, 1);
        }
        else if (raiseAmount + betFloor < 100)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 5) * 5 - (betFloor);
            raiseAmount = Math.Max(raiseAmount, 5);
        }
        else if (raiseAmount + betFloor < 250)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 10) * 10 - (betFloor);
            raiseAmount = Math.Max(raiseAmount, 10);
        }
        else if (raiseAmount + betFloor < 500)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 25) * 25 - (betFloor);
            raiseAmount = Math.Max(raiseAmount, 25);
        }
        else if (raiseAmount + betFloor < 1000)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 50) * 50 - (betFloor);
            raiseAmount = Math.Max(raiseAmount, 50);
        }
        else
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 100) * 100 - (betFloor);
            raiseAmount = Math.Max(raiseAmount, 100);
        }

        return raiseAmount;
    }
}
