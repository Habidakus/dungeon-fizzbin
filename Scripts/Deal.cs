using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

class DiscardCards
{
    internal Card Card { get; private set; }
    internal List<int> PlayersWhoCanSeeThis { get; private set; } = new List<int>();
    internal int PlayerWhoDiscardedThis { get; private set; }
    internal DiscardCards(Card card, List<int> playersWhoCanSeeThis, int playerWhoDiscardedThis)
    {
        Card = card;
        PlayersWhoCanSeeThis = playersWhoCanSeeThis;
        PlayerWhoDiscardedThis = playerWhoDiscardedThis;
    }
};

class Deal
{
    internal List<Suit> _suits = new List<Suit>();
    internal List<Rank> _ranks = new List<Rank>();
    internal List<Card> _drawPile = new List<Card>();
    internal List<Hand> _hands = new List<Hand>();
    internal List<Card> _river = new List<Card> ();
    internal List<DiscardCards> _discards = new List<DiscardCards> ();
    internal int DiscardsToReveal { get; private set; }
    internal int RevealRightNeighborsHighestCards { get; set; }
    internal int PassCardsToLeftNeighbor { get; set; }
    internal int RiverSize { get; private set; }
    internal int HandSize { get; private set; }
    internal int MaxDiscard { get; private set; }
    internal bool PixieCompare { get; private set; }
    internal double PendingCostPerDiscard { get; private set; }
    internal double CostPerDiscard { get; private set; }
    internal int NumberOfHighestRankingCardsToExpose { get; private set; }
    internal double Pot { get; private set; }
    internal HandValue.HandRanking MinimumHandToWinPot { get; private set; } = HandValue.HandRanking.HighCard;

    public Player NonNPCPlayer {
        get
        {
            return _hands[0].Player;
        }
    }

    internal Deal(double carryoverPot)
    {
        DiscardsToReveal = 0;
        RevealRightNeighborsHighestCards = 0;
        PassCardsToLeftNeighbor = 0;
        HandSize = 5;
        RiverSize = 0;
        MaxDiscard = 3;
        PixieCompare = false;
        CostPerDiscard = 0;
        MinimumHandToWinPot = HandValue.HandRanking.HighCard;
        PendingCostPerDiscard = 0;
        NumberOfHighestRankingCardsToExpose = 0;
        Pot = carryoverPot;

        _suits.AddRange(Suit.DefaultSuits);
        _ranks.AddRange(Rank.DefaultRanks);
    }

    internal void AddPlayer(HUD hud, Player player)
    {
        player.Species.ApplyDealComponent(this);
        player.PrepForDeal(this);
        player.InitHud(hud);
    }

    internal void Shuffle(List<Player> players, Random rnd)
    {
        ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);

        foreach (Suit suit in _suits)
        {
            foreach (Rank rank in _ranks)
            {
                Card card = new Card(suit, rank);
                _drawPile.Add(card);
            }
        }

        _drawPile = _drawPile.Shuffle(rnd).ToList();

        for (int i = 0; i < RiverSize; ++i)
        {
            _river.Add(_drawPile.First());
            _drawPile.RemoveAt(0);
        }

        //Test(rnd, players[0]);
        foreach (Player player in players)
        {
            Hand hand = new Hand(player);
            for (int i = 0; i < HandSize; ++i)
            {
                hand.AddCard(_drawPile.First());
                _drawPile.RemoveAt(0);
            }

            hand.ComputeBestScore(minRank, maxRank, suitsCount, _river);

            _hands.Add(hand);
        }
    }

    public void ForceBetOrFold(HUD hud, Player player, List<Player> allPlayers, Random rnd, double currentRaise, int bettingRound, Action<int, double> confirmBetPlaced)
    {
        if (player.IsNPC)
            ForceBetOrFold_NPC(player, allPlayers, currentRaise, rnd, bettingRound, confirmBetPlaced);
        else
            ForceBetOrFold_NonNPC(hud, player.PositionID, currentRaise, confirmBetPlaced);
    }

    private void ForceBetOrFold_NonNPC(HUD hud, int positionID, double currentRaise, Action<int, double> confirmBetPlaced)
    {
        hud.EnableBetSlider(currentRaise);
        Task.Run(() =>
        {
            double amountToBet = hud.HaveChosenAmountToBet().Result;
            confirmBetPlaced(positionID, amountToBet);
        });
    }

    public void ForceBetOrFold_NPC(Player player, List<Player> allPlayers, double currentRaise, Random rnd, int bettingRound, Action<int, double> confirmBetPlaced)
    {
        DateTime start = DateTime.Now;
        ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);

        Hand hand = GetPlayerHand(player);
        hand.ComputeBestScore(minRank, maxRank, suitsCount, _river);

        bool canStopTheRoundByMatching = true;
        double maxPercent = double.MinValue;
        foreach (Player otherPlayer in allPlayers)
        {
            if (otherPlayer.PositionID != hand.PositionID)
            {
                if (!otherPlayer.HasFolded)
                {
                    if (otherPlayer.AmountBet < currentRaise)
                    {
                        canStopTheRoundByMatching = false;
                    }

                    Hand otherHand = GetPlayerHand(otherPlayer);
                    if (otherHand._handValue == null)
                    {
                        otherHand.ComputeBestScore(minRank, maxRank, suitsCount, _river);
                    }

                    List<Card> unseenCards = AvailableCardsFromHandsView(hand);
                    double percent = WhatIsThePercentChanceOtherPlayerIsBetterThanOurHand(otherPlayer, hand, unseenCards, rnd);
                    if (percent > maxPercent)
                    {
                        maxPercent = percent;
                    }
                }
            }
        }

        double ourChance = 100.0 - maxPercent;
        player.ForceBetOrFold(hand, ourChance, currentRaise, canStopTheRoundByMatching, bettingRound, confirmBetPlaced);

        GD.Print($"Bet computation time: {(DateTime.Now - start).TotalSeconds:F2}");
    }

    public void Dump()
    {
        GD.Print($"Sorted hands (pixie={PixieCompare}):");
        foreach(Hand hand in _hands.OrderBy(a => a).Reverse())
        {
            GD.Print(hand);
        }
    }

    internal void UpdateHUD(HUD hud)
    {
        foreach (Hand hand in _hands)
        {
            hud.SetVisibleHand(hand, NonNPCPlayer);
        }

        if (RiverSize == 0)
        {
            hud.HideRiver();
        }
        else
        {

            hud.ShowRiver(_river);
        }
    }

    internal void Reveal(Player player, HUD hud, string description)
    {
        player.HasRevealed = true;
        hud.SetVisibleHand(GetPlayerHand(player), NonNPCPlayer);
        hud.SetBetAmount(player.PositionID, player.AmountBet, description);
    }

    internal List<Card> AvailableCardsFromHandsView(Hand viewHand)
    {
        List<Card> retVal = new List<Card>();
        retVal.AddRange(_drawPile);
        foreach (Hand hand in _hands)
        {
            if (hand == viewHand)
            {
                continue;
            }

            foreach (Card card in hand._cards)
            {
                if (!hand.IsVisible(card, viewHand.Player))
                {
                    retVal.Add(card);
                }
            }
        }

        foreach (DiscardCards discard in _discards)
        {
            if (!discard.PlayersWhoCanSeeThis.Contains(viewHand.PositionID))
            {
                retVal.Add(discard.Card);
            }
        }

        return retVal;
    }

    internal void ApplyDiscardCost()
    {
        CostPerDiscard = PendingCostPerDiscard;
    }

    internal void ReleaseDiscardCost()
    {
        CostPerDiscard = 0;
    }

    internal void HavePlayerDiscard(Player player, Random rnd, HUD hud, Action<int> confirmDiscardEvent)
    {
        ApplyDiscardCost();
        if (player.IsNPC)
        {
            Task.Run(() =>
            {
                Hand hand = GetPlayerHand(player);
                player.Discards = hand.SelectDiscards(0, MaxDiscard, this, rnd);
                confirmDiscardEvent(player.PositionID);
            });
        }
        else
        {
            Hand hand = GetPlayerHand(player);
            hud.EnableCardSelection_Discard(player.PositionID, MaxDiscard, CostPerDiscard);
            Task.Run(() => {
                List<string> cardsAsText = hud.HavePlayerSelectCardsToPassOrDiscard(hand).Result;
                player.Discards = new List<Card>();
                foreach (string cardText in cardsAsText)
                {
                    Card[] matchingCards = hand._cards.Where(a => a.ToString().CompareTo(cardText) == 0).ToArray();
                    if (matchingCards.Count() == 1)
                    {
                        player.Discards.Add(matchingCards[0]);
                    }
                    else if (matchingCards.Count() > 1)
                    {
                        throw new Exception($"Expected 1 card match to hand ({this}) but got {string.Join(',', matchingCards.Select(a => a.ToString()))}");
                    }
                    else
                    {
                        throw new Exception($"Expected 1 card match to hand ({this}) but got zero");
                    }
                }
                confirmDiscardEvent(player.PositionID);
            });
        }
    }

    internal void HavePlayerDiscard_Post(Player player, HUD hud)
    {
        if (!player.IsNPC)
        {
            hud.DisableCardSelection(player.PositionID);
        }

        if (player.Discards != null)
        {
            double penaltyForDiscard = player.Discards!.Count * CostPerDiscard;
            MoveMoneyToPot(hud, penaltyForDiscard, player);
            UpdatePot(hud);
        }

        ReleaseDiscardCost();
        player.HasDiscarded = true;
    }

    internal Hand GetPlayerHand(Player player)
    {
        return _hands.First(a => a._player == player);
    }

    internal bool NeedsToProcessPassedCards(out int positionID)
    {
        foreach (Hand hand in _hands)
        {
            if (HasPassingCards(hand))
            {
                GD.Print($"We need to process passed cards because {hand._player.Name} still has cards that need to be handed out.");
                positionID = hand.PositionID;
                return true;
            }
        }

        positionID = -1;
        return false;
    }

    internal bool HasPassingCards(Hand hand)
    {
        return hand.HasPassingCards;
    }

    internal void DeterminePassCards(HUD hud, Player sourcePlayer, Random rng, Action<int> confirmingPassCardsDetermined)
    {
        Hand hand = GetPlayerHand(sourcePlayer);
        int destinationPositionId = (hand.PositionID + 1) % _hands.Count;
        Player destPlayer = _hands[destinationPositionId].Player;
        hand.SetAsidePassCards(PassCardsToLeftNeighbor, this, rng, hud, destPlayer, confirmingPassCardsDetermined);
    }

    internal void DeterminePassCards_Post(HUD hud, Player sourcePlayer, Random rng, double delay)
    {
        Hand hand = GetPlayerHand(sourcePlayer);
        int destinationPositionId = (hand.PositionID + 1) % _hands.Count;
        Player destPlayer = _hands[destinationPositionId].Player;
        hand.SetAsidePassCards_Post(rng, hud, delay, destPlayer, _hands[0]._player);
        hud.SetVisibleHand(hand, NonNPCPlayer);
    }

    internal void ResolvePassAndRiver(HUD hud)
    {
        // TODO: We should animate all this passing

        ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);
        foreach (Hand hand in _hands)
        {
            int getFromPositionID = (hand.PositionID + _hands.Count - 1) % _hands.Count;
            Hand passingHand = _hands[getFromPositionID];
            if (passingHand._passingCards != null)
            {
                foreach (Card card in passingHand._passingCards)
                {
                    hand.AddCard(card);
                    hand._exposedCards.Add(card: card, seer: getFromPositionID, canDiscard: true);
                }

                hand.ComputeBestScore(minRank, maxRank, suitsCount, _river);

                GD.Print($"{passingHand._player.Name} passed {string.Join(',', passingHand._passingCards)} to {hand._player.Name}");

                passingHand._passingCards = null;
                hud.SetVisibleHand(hand, NonNPCPlayer);
            }
            else
            {
                GD.Print($"{passingHand._player.Name}'s hand hasn't any passed cards");
                throw new Exception($"{passingHand._player.Name}'s hand hasn't any passed cards");
            }
        }

        PassCardsToLeftNeighbor = 0;
    }

    internal void MoveCardToDiscard(HUD hud, Player player, List<int> playersWhoCanSeeThisDiscard, Card card)
    {
        Hand hand = GetPlayerHand(player);
        hand._cards.Remove(card);
        hand._handValue = null;
        _discards.Add(new DiscardCards(card, playersWhoCanSeeThisDiscard, player.PositionID));
        hud.SetVisibleHand(hand, NonNPCPlayer);
        hud.MoveCardToDiscard(player.PositionID, card, playersWhoCanSeeThisDiscard.Contains(NonNPCPlayer.PositionID));
    }

    internal bool ProgressReplaceDiscard(HUD hud)
    {
        ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);

        foreach (Hand hand in _hands)
        {
            if (hand._cards.Count < HandSize)
            {
                Card card = _drawPile.First();
                _drawPile.RemoveAt(0);
                hand._cards.Add(card);
                if (hand._cards.Count < HandSize)
                {
                    hand._handValue = null;
                }
                else
                {
                    hand.ComputeBestScore(minRank, maxRank, suitsCount, _river);
                }

                hud.SetVisibleHand(hand, NonNPCPlayer);
                return true;
            }
        }

        return false;
    }

    internal void ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount)
    {
        Rank.ExtractMinAndMax(_ranks, _suits, out minRank, out maxRank, out suitsCount);
    }

    internal double WhatIsThePercentChanceOtherPlayerIsBetterThanOurHand(Player otherPlayer, Hand ourHand, List<Card> unseenCards, Random rnd)
    {
        ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);
        Hand otherHand = GetPlayerHand(otherPlayer);
        if (otherHand._handValue == null)
        {
            throw new Exception("Can't call WhatIsThePercentChanceOtherPlayerIsBetterThanOurHand() with un-evaluated hand");
        }

        int theyWin = 0;
        const int numHandsToCreate = 1000;
        int potentialDiscards = Math.Min(otherPlayer.DiscardCount, otherHand._cards.Count - otherHand.CardsVisibleToSeer(ourHand.PositionID));
        for (int i = 0; i < numHandsToCreate; ++i)
        {
            Hand potentialHand = otherHand.GeneratePotentialHand(unseenCards, rnd, ourHand._player);
            potentialHand.ComputeBestScore(minRank, maxRank, suitsCount, _river);
            HandValue av = potentialHand.ApplyRandomDiscard(potentialDiscards, unseenCards, minRank, maxRank, suitsCount, rnd, ourHand._player);

            if (ourHand._handValue!.PixieCompareTo(av, PixieCompare) < 0)
            {
                theyWin += 1;
            }
        }

        return 100.0 * theyWin / numHandsToCreate;
    }

    internal void ExposeHighestRankingCards(HUD hud)
    {
        SortedSet<Card> allHandCards = new SortedSet<Card>(Comparer<Card>.Create((a,b) => a.PixieCompareTo(b, pixieCompare: false)));
        foreach (Hand hand in _hands)
        {
            foreach (Card card in hand._cards)
            {
                allHandCards.Add(card);
            }
        }

        var highestRankCardsInHands = allHandCards.TakeLast(NumberOfHighestRankingCardsToExpose).ToArray().AsSpan();
        foreach (Hand hand in _hands)
        {
            foreach (Card card in highestRankCardsInHands)
            {
                if (hand._cards.Contains(card))
                {
                    foreach (Player seer in _hands.Select(a => a._player))
                    {
                        if (hand.PositionID != seer.PositionID)
                        {
                            hand._exposedCards.Add(card, seer.PositionID, canDiscard: false);
                            hud.ExposeCardToOtherPlayer(hand.PositionID, card, seer);
                        }
                    }
                }
            }
        }

        NumberOfHighestRankingCardsToExpose = 0;
    }

    public bool MeetsMinCards(int deltaRank, int deltaSuit)
    {
        const int maxPlayerCount = 5;

        int cardCount = (_ranks.Count + deltaRank) * (_suits.Count + deltaSuit);
        int cardsNeeded = RiverSize + maxPlayerCount * (HandSize + MaxDiscard);
        bool canAdd = cardCount >= cardsNeeded;
        return canAdd;
    }

    internal void AddSuit()
    {
        //_suits.RemoveAll(suit => suit._color != Suit.SuitColor.Red);

        if (!_suits.Contains(Suit.Skull))
        {
            _suits.Add(Suit.Skull);
            return;
        }

        if (!_suits.Contains(Suit.Swords))
        {
            _suits.Add(Suit.Swords);
            return;
        }

        throw new Exception("Asked to add third suit to deal");
    }

    internal void RemoveSuit(Suit.SuitColor color)
    {
        List<Suit> suitsThatCanBeRemoved = _suits.Where(a => a._color == color).ToList();
        if (suitsThatCanBeRemoved.Count > 0)
        {
            Suit suitToRemove = suitsThatCanBeRemoved.First();
            _suits.Remove(suitToRemove);
            return;
        }

        throw new Exception($"No more suits to remove of color {color}");
    }

    internal void AddRank(bool addToLowEnd)
    {
        if (addToLowEnd)
        {
            if (!_ranks.Contains(Rank.Lead))
            {
                _ranks.Add(Rank.Lead);
                return;
            }

            if (!_ranks.Contains(Rank.Anhk))
            {
                _ranks.Add(Rank.Anhk);
                return;
            }
        }
        else
        {
            if (!_ranks.Contains(Rank.Empress))
            {
                _ranks.Add(Rank.Empress);
                return;
            }

            if (!_ranks.Contains(Rank.Jupiter))
            {
                _ranks.Add(Rank.Jupiter);
                return;
            }
        }

        throw new Exception($"Failed to add rank to lowEnd={addToLowEnd} end");
    }
    
    internal void RemoveRank(int first, int second)
    {
        if (_ranks.Where(a => a._strength == first).Count() > 0)
        {
            _ranks.RemoveAll(a => a._strength == first);
            return;
        }

        if (_ranks.Where(a => a._strength == second).Count() > 0)
        {
            _ranks.RemoveAll(a => a._strength == second);
            return;
        }

        throw new Exception($"Both ranks ({first}, {second}) already removed");
    }

    internal void IncreaseDiscardReveal()
    {
        DiscardsToReveal += 1;
    }

    internal void IncreaseObserveNeighborHighCard()
    {
        RevealRightNeighborsHighestCards += 1;
    }

    internal void AddPassToNeighbor()
    {
        PassCardsToLeftNeighbor += 1;
    }

    internal void IncreaseRiver()
    {
        RiverSize += 2;
        HandSize -= 1;
        MaxDiscard -= 1;
    }

    internal void SetPixieCompare()
    {
        PixieCompare = true;
    }

    internal void IncreaseCostPerDiscard()
    {
        PendingCostPerDiscard += 0.5;
    }

    internal void ShowHighestRankCards()
    {
        NumberOfHighestRankingCardsToExpose += 3;
    }

    internal void MoveMoneyToPot(HUD hud, double amount, Player player)
    {
        Pot += amount;
        player.RemoveMoney(hud, amount);
    }

    internal void UpdatePot(HUD hud)
    {
        hud.SetPot(Pot);
    }

    internal double CarryoverPot()
    {
        double retVal = Pot;
        Pot = 0;
        return retVal;
    }

    internal void MovePotToPlayer(HUD hud, Player player)
    {
        player.AddMoney(hud, Pot);
        Pot = 0;
    }

    internal void SetMinimumHandToWinPot(HandValue.HandRanking first, HandValue.HandRanking second)
    {
        if (MinimumHandToWinPot == HandValue.HandRanking.HighCard)
            MinimumHandToWinPot = first;
        else if (MinimumHandToWinPot == first)
            MinimumHandToWinPot = second;
    }
}

public static class ExtensionMethods
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
    {
        return source.Select(a => Tuple.Create(rnd.Next(), a)).ToList().OrderBy(a => a.Item1).Select(a => a.Item2);
    }
}



//private void Test(Random rnd, Player player)
//{
//    ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);
//    const int flushIndex = 1;
//    const int straightIndex = 2;
//    const int minorHouseIndex = 4;
//    Dictionary<Tuple<int, int, int>, int> kindCount = new Dictionary<Tuple<int, int, int>, int>();
//    for (int i = 0; i < 2500000; ++i)
//    {
//        var newPile = _drawPile.Shuffle(rnd).ToList();
//        Hand hand = new Hand(player);
//        for (int j = 0; j < HandSize; ++j)
//        {
//            hand.AddCard(newPile[j]);
//        }

//        hand.CountTypes(minRank, maxRank, out bool isFlush, out bool isStraight, out bool isMinorHouse, out int primaryOfAKind, out int secondaryOfAKind);
//        int index = 0;
//        if (isFlush)
//            index += flushIndex;
//        if (isStraight)
//            index += straightIndex;
//        if (isMinorHouse)
//            index += minorHouseIndex;
//        var key = Tuple.Create(index, primaryOfAKind, secondaryOfAKind);
//        if (kindCount.TryGetValue(key, out int count))
//        {
//            kindCount[key] = count + 1;
//        }
//        else
//        {
//            kindCount[key] = 1;
//        }
//    }

//    foreach (var entry in kindCount)
//    {
//        StringBuilder sb = new StringBuilder();
//        int key = entry.Key.Item1;
//        if ((key & flushIndex) == flushIndex)
//        {
//            sb.Append(" Flush");
//            key -= flushIndex;
//        }
//        if ((key & straightIndex) == straightIndex)
//        {
//            sb.Append(" Straight");
//            key -= straightIndex;
//        }
//        if ((key & minorHouseIndex) == minorHouseIndex)
//        {
//            sb.Append(" MinorHouse");
//            key -= minorHouseIndex;
//        }
//        if (key != 0)
//        {
//            if (sb.Length == 0)
//                sb.Append(" Highcard");
//            else
//                sb.Append($" 0x{key}");
//        }
//        if (entry.Key.Item3 == 0)
//        {
//            switch (entry.Key.Item2)
//            {
//                case 5: sb.Append(" FiveOfAKind"); break;
//                case 4: sb.Append(" FourOfAKind"); break;
//                case 3: sb.Append(" ThreeOfAKind"); break;
//                case 2: sb.Append(" Pair"); break;
//                case 0: sb.Append(" Highcard"); break;
//                default:
//                    sb.Append($" ?{entry.Key.Item2}0");
//                    break;
//            }
//        }
//        else if (entry.Key.Item3 == 2)
//        {
//            if (entry.Key.Item2 == 3)
//                sb.Append(" FullHouse");
//            else if (entry.Key.Item2 == 2)
//                sb.Append(" TwoPair");
//            else
//                sb.Append($" ?{entry.Key.Item2}2");
//        }
//        else
//        {
//            sb.Append($" ?{entry.Key.Item2}{entry.Key.Item3}");
//        }

//        GD.Print($"{entry.Value} {sb}");
//    }
//}

//private void Test2(Random rnd, Player player)
//{
//    ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);
//    Hand? bestHand = null;
//    Hand? worstHand = null;
//    for (int i = 0; i < 25000; ++i)
//    {
//        var newPile = _drawPile.Shuffle(rnd).ToList();
//        Hand hand = new Hand(player);
//        for (int j = 0; j < 5; ++j)
//        {
//            hand.AddCard(newPile[j]);
//        }

//        hand.ComputeBestScore(minRank, maxRank, suitsCount);
//        if (bestHand == null || hand.CompareTo(bestHand) > 0)
//        {
//            bestHand = hand;
//        }
//        if (worstHand == null || hand.CompareTo(worstHand) < 0)
//        {
//            worstHand = hand;
//        }
//    }

//    GD.Print($"Best Hand: {bestHand}");
//    GD.Print($"Worst Hand: {worstHand}");
//}