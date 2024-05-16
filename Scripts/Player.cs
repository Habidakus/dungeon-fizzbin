using Godot;
using System;
using System.Collections.Generic;

#nullable enable

class Player
{
    internal static int InitialWallet { get { return 200; } }

    internal int PositionID { get; private set; }
    internal bool IsNPC { get; private set; }
    internal bool HasDiscarded { get; set; }
    internal bool HasFolded { get; private set; }
    internal double AmountBet { get; private set; }
    internal bool HasRevealed { get; set; }
    internal List<Card>? Discards { get; set; }
    internal int DiscardCount { get; set; }
    internal Species Species { get; private set; }
    internal string Name { get; private set; }
    internal int ExposedDiscardCount { get; set; }
    internal Deal Deal { get; private set; }
    internal double Wallet { get; private set; }

    public override string ToString()
    {
        return $"{PositionID}: {Name} {Species.Name} ${Wallet:F2}";
    }

    internal Player(Deal deal, Species species) // Used for setting the Non NPC Player
    {
        Deal = deal;
        PositionID = 0;
        IsNPC = false;
        Wallet = InitialWallet;
        Species = species;

        if (OS.HasEnvironment("USERNAME"))
            Name = OS.GetEnvironment("USERNAME");
        else
            Name = "Player";
    }

    internal Player(Deal deal, PlayerSaveElement playerElement) // Used for setting the Non NPC Player
    {
        Deal = deal;
        PositionID = 0;
        IsNPC = false;
        Wallet = playerElement.Wallet;
        Species = Species.Get(playerElement.SpeciesEl.Name);

        if (OS.HasEnvironment("USERNAME"))
            Name = OS.GetEnvironment("USERNAME");
        else
            Name = "Player";
    }

    internal Player(int positionID, Random rng, List<Species> speciesAlreadyAtTable, Deal deal)
    {
        Deal = deal;
        PositionID = positionID;
        Wallet = InitialWallet;
        IsNPC = true;
        Species = Species.PickSpecies(rng, speciesAlreadyAtTable, deal);
        Name = Species.GenerateRandomName(rng, PositionID);
    }

    internal Player(int positionID, Random rng, Species species, Deal deal)
    {
        Deal = deal;
        PositionID = positionID;
        Wallet = InitialWallet;
        IsNPC = (PositionID != 0);
        Species = species;

        if (PositionID == 0)
        {
            if (OS.HasEnvironment("USERNAME"))
                Name = OS.GetEnvironment("USERNAME");
            else
                Name = "Player";
        }
        else
        {
            Name = Species.GenerateRandomName(rng, PositionID);
        }
    }

    internal void PrepForDeal(Deal deal)
    {
        Deal = deal;
        AmountBet = 0;
        HasDiscarded = false;
        HasFolded = false;
        HasRevealed = false;
        Discards = null;
        DiscardCount = 0;
        ExposedDiscardCount = 0;
    }

    internal void InitHud(HUD hud)
    {
        hud.SetPlayerInfo(this);
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

    internal void ForceBetOrFold(Hand hand, double percent, double betFloor, bool canStopTheRoundByMatching, int bettingRound, Action<int, double> confirmBetPlaced)
    {
        const double costToDiscard = 0.0; // We've already paid our discard price and we're just now
                                          // evaluating what we are going to do with the current hand.
        AggregateValue agValue = new AggregateValue(this, hand, costToDiscard, (double)Deal.MinimumHandToWinPot);
        if (agValue._normalizedWealth <= 0)
        {
            throw new Exception($"Why is player #{hand.PositionID}'s aggregate value {agValue.GetDesc()}");
        }

        double willingToGoAsFarAs = 1.75 * Math.Sqrt(agValue._normalizedWealth);
        double comfortZoneStartsAt = willingToGoAsFarAs * percent / 100.0;
        double amountWedHaveToAdd = betFloor - AmountBet;

        if (hand._handValue == null)
        {
            throw new Exception($"Why does {hand} not have a value in ForceBetOrFold?");
        }

        if (hand._handValue.Worth < (double) Deal.MinimumHandToWinPot)
        {
            confirmBetPlaced(PositionID, 0);
            return;
        }

        //string handTxt = hand._handValue?.ToString() ?? "??";
        if (comfortZoneStartsAt < betFloor)
        {
            if (canStopTheRoundByMatching && amountWedHaveToAdd <= AmountBet)
            {
                // if we've put in a lot, and we can stop the betting round with us if we just don't raise, then we should match instead of folding.
                //GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt} ... and would quite but they can stop the bidding here.");
                confirmBetPlaced(PositionID, betFloor);
            }
            else
            {
                //GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, Folds as ${comfortZoneStartsAt:F2} < ${betFloor:F2} and ${amountWedHaveToAdd:F2} is too much");
                confirmBetPlaced(PositionID, 0);
            }

            return;
        }

        if (comfortZoneStartsAt - amountWedHaveToAdd < betFloor)
        {
            //GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (not willing to risk it) ${comfortZoneStartsAt - amountWedHaveToAdd:F2} < ${betFloor:F2}");
            confirmBetPlaced(PositionID, betFloor);
            return;
        }

        double raiseAmount = DetermineRaiseAmount(betFloor, willingToGoAsFarAs, bettingRound);

        if (willingToGoAsFarAs < betFloor + raiseAmount)
        {
            //GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, stands as (would be a step too far) ${willingToGoAsFarAs:F2} < ${betFloor + raiseAmount:F2}");
            confirmBetPlaced(PositionID, betFloor);
        }
        else
        {
            // We have a decent hand, and we think we can win
            //GD.Print($"Player #{PositionID}: {percent:F2}% chance of winning with {handTxt}, raises from ${betFloor:F2} to ${betFloor + raiseAmount:F2}: as ${comfortZoneStartsAt:F2} => ${betFloor:F2} <= ${willingToGoAsFarAs:F2}");
            confirmBetPlaced(PositionID, betFloor + raiseAmount);
        }
    }

    internal static List<double> GetNextBets(double betFloor, int numberOfBets)
    {
        if (numberOfBets <= 0)
        {
            return new List<double>();
        }
        else
        {
            double advance = GetNextBetPast(betFloor);

            if (numberOfBets == 1)
            {
                return new List<double>() { advance };
            }
            else
            {
                List<double> retVal = GetNextBets(advance, numberOfBets - 1);
                retVal.Insert(0, advance);
                return retVal;
            }
        }
    }

    private static double GetNextBetPast(double previousBet)
    {
        if (previousBet < 5)
        {
            return Math.Round(1 + previousBet * 4) / 4;
        }
        else if (previousBet < 20)
        {
            return Math.Round(previousBet + 1);
        }
        else if (previousBet < 100)
        {
            return Math.Round(1 + (previousBet / 5)) * 5;
        }
        else if (previousBet < 250)
        {
            return Math.Round(1 + (previousBet / 10)) * 10;
        }
        else if (previousBet < 500)
        {
            return Math.Round(1 + (previousBet / 25)) * 25;
        }
        else if (previousBet < 1000)
        {
            return Math.Round(1 + (previousBet / 50)) * 50;
        }
        else
        {
            return Math.Round(1 + (previousBet / 100)) * 100;
        }
    }

    private static double DetermineRaiseAmount(double betFloor, double willingToGoAsFarAs, int bettingRound)
    {
        int roundsLeft = Math.Max(1, 10 - bettingRound);
        double raiseAmount = (willingToGoAsFarAs - betFloor) / (roundsLeft * roundsLeft);
        if (raiseAmount + betFloor < 5)
        {
            raiseAmount = Math.Round(raiseAmount * 4) / 4;
            raiseAmount = Math.Clamp(raiseAmount, 0, 1);
        }
        else if (raiseAmount + betFloor < 20)
        {
            raiseAmount = Math.Round(betFloor + raiseAmount) - (betFloor);
            raiseAmount = Math.Clamp(raiseAmount, 1, 5);
        }
        else if (raiseAmount + betFloor < 100)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 5) * 5 - (betFloor);
            raiseAmount = Math.Clamp(raiseAmount, 5, 25);
        }
        else if (raiseAmount + betFloor < 250)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 10) * 10 - (betFloor);
            raiseAmount = Math.Clamp(raiseAmount, 10, 50);
        }
        else if (raiseAmount + betFloor < 500)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 25) * 25 - (betFloor);
            raiseAmount = Math.Clamp(raiseAmount, 25, 100);
        }
        else if (raiseAmount + betFloor < 1000)
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 50) * 50 - (betFloor);
            raiseAmount = Math.Clamp(raiseAmount, 50, 250);
        }
        else
        {
            raiseAmount = Math.Round((betFloor + raiseAmount) / 100) * 100 - (betFloor);
            raiseAmount = Math.Max(raiseAmount, 100);
        }

        return raiseAmount;
    }

    internal void RemoveMoney(HUD hud, double amount)
    {
        Wallet -= amount;
        if (!IsNPC)
        {
            hud.SetStake(Wallet);
        }
    }

    internal void AddMoney(HUD hud, double amount)
    {
        Wallet += amount;
        if (!IsNPC)
        {
            hud.SetStake(Wallet);
        }
    }
}

public class PlayerSaveElement : SaveElement
{
    internal SpeciesSaveElement SpeciesEl { get; private set; }
    internal double Wallet { get; private set; }
    public PlayerSaveElement()
    {
        SaveVersion = 1;
        Wallet = Player.InitialWallet;
        SpeciesEl = new SpeciesSaveElement();
    }
    internal PlayerSaveElement(Player player)
    {
        SaveVersion = 1;
        Wallet = player.Wallet;
        SpeciesEl = new SpeciesSaveElement(player.Species);
    }

    protected override void LoadData(uint loadVersion, FileAccess access)
    {
        if (loadVersion != SaveVersion)
            throw new Exception($"No upgrade path from version {loadVersion} to {SaveVersion} for PlayerElement");

        if (loadVersion >= 1)
        {
            Wallet = access.GetDouble();
            SpeciesEl.Load(access);
        }
    }

    protected override void SaveData(FileAccess access)
    {
        access.StoreDouble(Wallet);
        SpeciesEl.Save(access);
    }
}