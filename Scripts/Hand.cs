﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Hand : IComparable<Hand>
{
    readonly internal Player _player;
    internal List<Card> _cards = new List<Card>();
    internal HandValue? _bestScore = null;

    public int PositionID { get { return _player.PositionID; } }
    public Player Player { get { return _player; } }

    internal Hand(Player player)
    {
        _player = player;
    }

    internal bool IsVisible(Card card, Player viewer)
    {
        if (_player.HasRevealed)
        {
            return true;
        }

        if (_player.PositionID == viewer.PositionID)
        {
            return true;
        }

        return false;
    }

    public bool IsEveryCardVisible(Player viewer)
    {
        foreach (Card card in _cards)
        {
            if (!IsVisible(card, viewer))
                return false;
        }

        return true;
    }

    internal void AddCard(Card card)
    {
        _cards.Add(card);
    }

    internal bool IsStraight(int minRank, int maxRank)
    {
        if (minRank >= maxRank)
        {
            throw new Exception("Min and Max rank not accurately calculated");
        }

        List<Card> cardsInOrder = _cards.OrderBy(a => a).ToList();
        bool retVal = true;
        for (int i = 1; retVal && i < cardsInOrder.Count; ++i)
        {
            if (!cardsInOrder[i - 1].Rank.IsNeighborTo(cardsInOrder[i].Rank, minRank, maxRank))
                retVal = false;
        }

        // If we didn't find a straight without considering that ACE can appear at the front of the sequence
        if (retVal == false && cardsInOrder.Last().Rank.Wraps)
        {
            if (!cardsInOrder.First().Rank.IsNeighborTo(cardsInOrder.Last().Rank, minRank, maxRank))
                return false;

            for (int i = 1; i < cardsInOrder.Count - 1; ++i)
            {
                if (!cardsInOrder[i - 1].Rank.IsNeighborTo(cardsInOrder[i].Rank, minRank, maxRank))
                    return false;
            }

            return true;
        }
        else
        {
            return retVal;
        }
    }

    internal bool IsFlush
    {
        get
        {
            Suit? suit = null;
            foreach(Card card in _cards)
            {
                if (suit == null)
                    suit = card.Suit;
                else if (suit != card.Suit)
                    return false;
            }

            return true;
        }
    }

    internal void ComputeBestScore(int minRank, int maxRank)
    {
        List<Card> cardsSortedHighestToLowest = _cards.OrderByDescending(a => a).ToList();
        Card highCard = cardsSortedHighestToLowest.First();
        _bestScore = new HandValue(cardsSortedHighestToLowest);
        bool isStraight = IsStraight(minRank, maxRank);
        bool isFlush = IsFlush;
        if (isStraight)
        {
            if (isFlush)
            {
                Card lowCard = _cards.OrderBy(a => a).First();
                if (lowCard.Rank.IsTenOrHigher())
                    _bestScore = new HandValue(HandValue.HandRanking.RoyalFlush, cardsSortedHighestToLowest);
                else
                    _bestScore = new HandValue(HandValue.HandRanking.StraightFlush, cardsSortedHighestToLowest);
            }
            else
            {
                _bestScore = new HandValue(HandValue.HandRanking.Straight, cardsSortedHighestToLowest);
            }
        }
        else if (isFlush)
        {
            _bestScore = new HandValue(HandValue.HandRanking.Flush, cardsSortedHighestToLowest);
        }

        ExtractOfAKind(_cards, out List<Card> ofAKind, out List<Card> remainder);
        if (ofAKind.Count > 0)
        {
            ofAKind.Sort();
            ofAKind.Reverse();
            HandValue? possiblyBetter = null;
            if (ofAKind.Count == 5)
            {
                possiblyBetter = new HandValue(HandValue.HandRanking.FiveOfAKind, ofAKind);
            }
            else if (ofAKind.Count == 4)
            {
                List<Card> orderOfImportance = new List<Card>(ofAKind);
                orderOfImportance.AddRange(remainder.OrderByDescending(a => a).ToList());
                possiblyBetter = new HandValue(HandValue.HandRanking.FourOfAKind, orderOfImportance);
            }
            else
            {
                List<Card> orderOfImportance = new List<Card>(ofAKind);
                ExtractOfAKind(remainder, out List<Card> secondOfAKind, out List<Card> remainderRemainder);
                orderOfImportance.AddRange(secondOfAKind.OrderByDescending(a => a));
                orderOfImportance.AddRange(remainderRemainder.OrderByDescending(a => a));

                if (ofAKind.Count == 3)
                {
                    if (secondOfAKind.Count > 0)
                    {
                        Card remainderHighCard = secondOfAKind.OrderByDescending(a => a).First();
                        possiblyBetter = new HandValue(HandValue.HandRanking.FullHouse, orderOfImportance);
                    }
                    else
                    {
                        Card remainderHighCard = remainder.OrderByDescending(a => a).First();
                        possiblyBetter = new HandValue(HandValue.HandRanking.ThreeOfAKind, orderOfImportance);
                    }
                }
                else if (ofAKind.Count == 2)
                {
                    if (secondOfAKind.Count > 0)
                    {
                        Card remainderHighCard = secondOfAKind.OrderByDescending(a => a).First();
                        possiblyBetter = new HandValue(HandValue.HandRanking.TwoPairs, orderOfImportance);
                    }
                    else
                    {
                        Card remainderHighCard = remainder.OrderByDescending(a => a).First();
                        possiblyBetter = new HandValue(HandValue.HandRanking.TwoOfAKind, orderOfImportance);
                    }
                }
            }

            if (possiblyBetter != null)
            {
                if (possiblyBetter.CompareTo(_bestScore) > 0)
                {
                    _bestScore = possiblyBetter;
                }
                else
                {
                    if (_bestScore._handRanking == HandValue.HandRanking.HighCard)
                    {
                        GD.Print($"HighCard better than {possiblyBetter._handRanking}?");
                    }
                }
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(_player);
        if (_bestScore != null)
            sb.Append($": ({ScoreAsString()})");
        else
            sb.Append(':');
        sb.Append(CardsAsString());

        return sb.ToString();
    }

    internal string ScoreAsString()
    {
        if (_bestScore != null)
            return $"{_bestScore}";
        else
            return "???";
    }

    internal string CardsAsString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (Card card in _cards.OrderByDescending(a => a))
        {
            sb.Append(" ");
            sb.Append(card.ToString());
        }

        return sb.ToString();
    }

    public int CompareTo(Hand? other)
    {
        if (other == null)
            throw new Exception("Comparing to null hand");

        if (_bestScore == null || other._bestScore == null)
            throw new Exception("Best Score not computed");

        return _bestScore.CompareTo(other._bestScore);
    }


    internal void CountTypes(int minRank, int maxRank, out bool isFlush, out bool isStraight, out int primaryOfAKind, out int secondaryOfAKind)
    {
        isFlush = IsFlush;
        isStraight = IsStraight(minRank, maxRank);
        ExtractOfAKind(_cards, out List<Card> ofAKind, out List<Card> remainder);
        primaryOfAKind = ofAKind.Count;
        if (primaryOfAKind > 0)
        {
            ExtractOfAKind(remainder, out List<Card> secondarySet, out List<Card> _);
            secondaryOfAKind = secondarySet.Count;
        }
        else
        {
            secondaryOfAKind = 0;
        }
    }

    private static void ExtractOfAKind(List<Card> cards, out List<Card> ofAKind, out List<Card> remainder)
    {
        Rank? bestRank = null;
        int bestCount = 0;
        Dictionary<Rank, List<Card>> buckets = new Dictionary<Rank, List<Card>>();
        foreach (Card card in cards)
        {
            if (buckets.TryGetValue(card.Rank, out List<Card>? cardsOfThisRank))
            {
                cardsOfThisRank.Add(card);
                buckets[card.Rank] = cardsOfThisRank;
                if (cardsOfThisRank.Count > bestCount)
                {
                    bestCount = cardsOfThisRank.Count;
                    bestRank = card.Rank;
                }
                else if (cardsOfThisRank.Count == bestCount && card.Rank.CompareTo(bestRank) > 0)
                {
                    bestRank = card.Rank;
                }
            }
            else
            {
                buckets[card.Rank] = new List<Card> { card };
            }
        }

        if (bestCount > 0)
        {
            ofAKind = buckets[bestRank!];
            remainder = new List<Card>();
            foreach (var entry in buckets)
            {
                if (entry.Key != bestRank)
                {
                    remainder.AddRange(entry.Value);
                }
            }
        }
        else
        {
            ofAKind = new List<Card>();
            remainder = new List<Card>(cards);
        }
    }

    internal Hand CloneWithDiscard(Card discardCard, Card replacementCard)
    {
        Hand retVal = new Hand(_player);
        foreach(Card card in _cards)
        {
            if (card == discardCard)
                retVal.AddCard(replacementCard);
            else
                retVal.AddCard(card);
        }

        return retVal;
    }

    internal Hand GeneratePotentialHand(List<Card> unseenCards, Random rnd)
    {
        Hand retVal = new Hand(_player);
        while (retVal._cards.Count < 5)
        {
            int cardIndex = rnd.Next() % unseenCards.Count;
            Card card = unseenCards.ElementAt(cardIndex);
            bool alreadyUsed = false;
            for (int i = 0; !alreadyUsed && i < retVal._cards.Count; ++i)
            {
                if (retVal._cards[i] == card)
                    alreadyUsed = true;
            }

            if (!alreadyUsed)
                retVal.AddCard(card);
        }

        return retVal;
    }

    internal static List<List<int>> GenerateUniqueDiscardSelections(int discards, int min, int max)
    {
        if (discards == 0)
        {
            throw new Exception("Why are we GenerateUniqueDiscardSelections() with zero discards?");
        }

        if (max - min < discards - 1)
        {
            throw new Exception($"Asking for more discards than is available (min={min}, max={max}, discards={discards})");
        }

        List<List<int>> retVal = new List<List<int>>();
        if (discards == 1)
        {
            for (int i = min; i <= max; ++i)
            {
                retVal.Add(new List<int>() { i });
            }
        }
        else
        {
            for (int i = min; i <= max; ++i)
            {
                if (i + discards - 1 <= max)
                {
                    foreach (List<int> subIter in GenerateUniqueDiscardSelections(discards - 1, i + 1, max))
                    {
                        var a = new List<int>() { i };
                        a.AddRange(subIter);
                        retVal.Add(a);
                    }
                }
            }
        }

        return retVal;
    }

    private Tuple<bool, List<Hand>> GenerateHands(List<int> discardIndices, List<Card> availableCards, Random rnd)
    {
        if (discardIndices.Count == 0)
            throw new Exception("GenerateHands() with zero discard indicies");

        int multiple = availableCards.Count;
        int count = 1;
        for (int i = 0; i < discardIndices.Count; ++i, --multiple)
            count *= multiple;

        if (count < 9000)
        {
            return Tuple.Create(false, GenerateAllHands(discardIndices, availableCards));
        }
        else
        {
            return Tuple.Create(true, GenerateSampledHands(discardIndices, availableCards, rnd));
        }
    }

    private void GeneratePullIndices(int cardCount, int discardCount, Random rnd, ref List<int> pullIndices)
    {
        for (int k = 0; k < discardCount; ++k)
        {
            bool matchFound = true;
            while (matchFound)
            {
                matchFound = false;
                pullIndices[k] = rnd.Next() % cardCount;
                for (int i = 0; i < k && !matchFound; ++i)
                {
                    if (pullIndices[i] == pullIndices[k])
                        matchFound = true;
                }
            }
        }
    }

    private List<Hand> GenerateSampledHands(List<int> discardIndices, List<Card> availableCards, Random rnd)
    {
        List<Hand> retVal = new List<Hand>();
        List<int> pullIndices = new List<int>();
        for(int k = 0; k < discardIndices.Count; ++k)
            pullIndices.Add(k);

        for (int i = 0; i<5000; ++i)
        {
            GeneratePullIndices(availableCards.Count, discardIndices.Count, rnd, ref pullIndices);
            Hand clone = CloneWithDiscard(_cards[discardIndices[0]], availableCards[pullIndices[0]]);
            for (int j = 1; j < discardIndices.Count; ++j)
            {
                clone._cards[discardIndices[j]] = availableCards[pullIndices[j]];
            }

            retVal.Add(clone);
        }

        return retVal;
    }

    private List<Hand> GenerateAllHands(List<int> discardIndices, List<Card> availableCards)
    {
        List<Hand> retVal = new List<Hand>();
        if (discardIndices.Count == 1)
        {
            foreach (Card card in availableCards)
            {
                retVal.Add(CloneWithDiscard(_cards[discardIndices[0]], card));
            }
        }
        else
        {
            List<int> subIndicies = discardIndices.GetRange(1, discardIndices.Count - 1);
            foreach (Card card in availableCards)
            {
                Hand subHand = CloneWithDiscard(_cards[discardIndices[0]], card);
                List<Card> remainingCards = availableCards.Where(a => a != card).ToList();
                retVal.AddRange(subHand.GenerateAllHands(subIndicies, remainingCards));
            }
        }

        return retVal;
    }

    internal Tuple<AggregateValue, List<Card>> SelectDiscards(List<int> discardIndices, List<Card> availableCards, int minRank, int maxRank, Random rnd)
    {
        if (discardIndices.Count == 0)
            throw new Exception("SelectDiscards() with zero discard indicies");

        Tuple<bool, List<Hand>> generatedHands = GenerateHands(discardIndices, availableCards, rnd);
        SortedSet<Hand> sortedHands = new SortedSet<Hand>();
        foreach (Hand hand in generatedHands.Item2)
        {
            hand.ComputeBestScore(minRank, maxRank);
            sortedHands.Add(hand);
        }

        List<Card> discards = new List<Card>();
        foreach(int index in discardIndices)
        {
            discards.Add(_cards[index]);
        }

        AggregateValue aggValue = new AggregateValue(_player);
        aggValue.Add(sortedHands.ToArray(), generatedHands.Item1);

        return Tuple.Create(aggValue, discards);
    }

    internal Tuple<AggregateValue, List<Card>> SelectDiscards(int noOfDiscards, List<Card> availableCards, int minRank, int maxRank, Random rnd)
    {
        if (noOfDiscards == 0)
        {
            return Tuple.Create(new AggregateValue(_player, this), new List<Card>());
        }

        List<Card> retVal = new List<Card>();
        AggregateValue best = new AggregateValue(_player);

        if (noOfDiscards == 1)
        {
            foreach(Card discardCard in _cards)
            {
                SortedSet<HandValue> scoreList = new SortedSet<HandValue>();
                foreach (Card replacement in availableCards)
                {
                    Hand potentialHand = CloneWithDiscard(discardCard, replacement);
                    potentialHand.ComputeBestScore(minRank, maxRank);
                    scoreList.Add(potentialHand._bestScore!);
                }

                if (scoreList.Count > 0)
                {
                    AggregateValue aggValue = new AggregateValue(_player);
                    aggValue.Add(scoreList.ToArray(), false);

                    if (best.UpdateIfBetter(aggValue))
                    {
                        retVal = new List<Card> { discardCard };
                    }
                }
            }
        }
        else
        {
            foreach (List<int> iter in GenerateUniqueDiscardSelections(noOfDiscards, 0, 4))
            {
                Tuple<AggregateValue, List<Card>> subBest = SelectDiscards(iter, availableCards, minRank, maxRank, rnd);
                if (best.UpdateIfBetter(subBest.Item1))
                {
                    retVal = subBest.Item2;
                }
            }
        }

        return Tuple.Create(best!, retVal);
    }

    internal List<Card> SelectDiscards(int minDiscards, int maxDiscards, Deal actualDeal, Random rnd)
    {
        Rank.ExtractMinAndMax(actualDeal._ranks, out int minRank, out int maxRank);
        List<Card> availableCards = actualDeal.AvailableCardsFromHandsView(this);
        List<Card> retVal = new List<Card>();
        AggregateValue best = new AggregateValue(_player);
        for (int discards = minDiscards; discards <= maxDiscards; ++discards)
        {
            Tuple<AggregateValue, List<Card>> subBest = SelectDiscards(discards, availableCards, minRank, maxRank, rnd);
            if (best.UpdateIfBetter(subBest.Item1))
            {
                retVal = subBest.Item2;
            }
        }

        if (best._hopefulValue != null)
        {
            StringBuilder cardsToText = new StringBuilder();
            foreach (Card card in retVal)
            {
                if (cardsToText.Length > 0)
                    cardsToText.Append(" ");
                cardsToText.Append(card.ToString());
            }

            GD.Print($"Player {_player.PositionID} replaces {cardsToText} trying for {best.GetDesc()}");
        }

        return retVal;
    }
}


class AggregateValue : IComparable<AggregateValue>
{
    readonly Player _player;
    internal HandValue? _hopefulValue = null;
    internal double _normalizedWealth = -1;
    internal Dictionary<HandValue.HandRanking, Tuple<double, int>> _normalizedByRank = new Dictionary<HandValue.HandRanking, Tuple<double, int>>();

    public AggregateValue(Player player, Hand hand)
    {
        _player = player;
        SetHopefulValue(hand._bestScore);

        _normalizedWealth = 0;
        if (hand._bestScore == null)
            throw new Exception("Initializing with scoreless hand");
        AddAggreageWorth(hand._bestScore, 1);
    }

    internal AggregateValue(Player player)
    {
        _player = player;
        _normalizedWealth = -2;
    }

    internal string DictText
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var pair in _normalizedByRank)
            {
                if (!first)
                    sb.Append(", ");
                first = false;
                sb.Append($"{pair.Key} {pair.Value.Item1:F2} {pair.Value.Item2}");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    internal string GetDesc()
    {
        if (_hopefulValue != null)
            return $"{_hopefulValue.GetDesc()} ${_normalizedWealth:F2} {DictText}";
        else
            return $"() ${_normalizedWealth:F2} {DictText}";
    }

    internal void Add(Hand[] sortedHands, bool sampled)
    {
        if (_normalizedWealth >= 0)
            throw new Exception($"Clobbering already set normalized wealth");

        _normalizedWealth = 0;

        // Best values are at the back
        int i = sortedHands.Length * 11 / 12;
        SetHopefulValue(sortedHands.ElementAt(i)._bestScore);

        // If we're reading sampled values then we can poison the results by having some bad edge cases.
        int maxReads = sortedHands.Length;
        if (sampled)
        {
            maxReads = (int)Math.Round(1999 * maxReads / 2000f);
        }

        for (i = 0; i < maxReads; ++i)
        {
            AddAggreageWorth(sortedHands.ElementAt(i)._bestScore, maxReads);
        }
    }

    // #TODO: Might be faster if we convert the SortedSet<> to an Array<> before we call here, if ElementAt() is slow
    internal void Add(HandValue[] sortedValues, bool sampled)
    {
        if (_normalizedWealth >= 0)
            throw new Exception($"Clobbering already set normalized wealth");

        _normalizedWealth = 0;

        // Best values are at the back
        int i = sortedValues.Length * 11 / 12;
        SetHopefulValue(sortedValues.ElementAt(i));

        // If we're reading sampled values then we can poison the results by having some bad edge cases.
        int maxReads = sortedValues.Length;
        if (sampled)
        {
            maxReads = (int)Math.Round(999 * maxReads / 1000f);
        }

        for (i = 0; i < maxReads; ++i)
        {
            AddAggreageWorth(sortedValues.ElementAt(i), maxReads);
        }
    }

    private void SetHopefulValue(HandValue? handValue)
    {
        if (_hopefulValue != null)
            throw new Exception("Clobbering AggregateValue");

        _hopefulValue = handValue;
    }

    private void AddAggreageWorth(HandValue? handValue, int setSize)
    {
        if (handValue == null)
            return;

        double highCardValue = (handValue._highCard.FractionalValue / Card.MaxFractionalValue);
        double totalValue = handValue.Worth + highCardValue;
        _normalizedWealth += totalValue / (double)setSize;

        if (_normalizedByRank.TryGetValue(handValue._handRanking, out Tuple<double, int>? value))
        {
            _normalizedByRank[handValue._handRanking] = Tuple.Create((totalValue / (double)setSize) + value.Item1, value.Item2 + 1);
        }
        else
        {
            _normalizedByRank[handValue._handRanking] = Tuple.Create((totalValue / (double)setSize), 1);
        }
    }

    public int CompareTo(AggregateValue? other)
    {
        if (other == null)
            return 1;
        if (_normalizedWealth <= 0)
        {
            throw new Exception($"Aggregate value [{GetDesc()}] doesn't have normalized wealth set");
        }
        if (other._normalizedWealth <= 0)
        {
            throw new Exception($"Aggregate value [{other.GetDesc()}] doesn't have normalized wealth set");
        }

        if (_normalizedWealth != other._normalizedWealth)
        {
            return _normalizedWealth.CompareTo(other._normalizedWealth);
        }

        return _hopefulValue.CompareTo(other._hopefulValue);
    }

    internal bool UpdateIfBetter(AggregateValue other)
    {
        if (_normalizedWealth < other._normalizedWealth)
        {
            _hopefulValue = other._hopefulValue;
            _normalizedWealth = other._normalizedWealth;
            _normalizedByRank = other._normalizedByRank;
            return true;
        }

        return false;
    }
}

class HandValue : IComparable<HandValue>
{
    internal enum HandRanking
    {
        FiveOfAKind = 3898434 * 4, // Only possible with more suits
        RoyalFlush = 3898434,
        StraightFlush = 433154,
        FourOfAKind = 24984,
        FullHouse = 4158,
        Flush = 3047, // LESS COMMON with more suits
        Straight = 1523,
        ThreeOfAKind = 278,
        TwoPairs = 120,
        TwoOfAKind = 8,
        HighCard = 6, // Maybe just lower this?
    };

    internal HandRanking _handRanking;
    internal Card _highCard;
    internal Double _fractionalValue;
    internal double Worth { get { return (double)_handRanking; } }

    internal string GetDesc()
    {
        return $"{ToString()} ({_highCard}) ({_fractionalValue:F2})";
    }

    public override string ToString()
    {
        switch (_handRanking)
        {
            case HandRanking.FiveOfAKind: return "Five of a Kind";
            case HandRanking.RoyalFlush: return "Royal Flush";
            case HandRanking.StraightFlush: return "Straight Flush";
            case HandRanking.FourOfAKind: return "Four of a Kind";
            case HandRanking.FullHouse: return "Full House";
            case HandRanking.Flush: return "Flush";
            case HandRanking.Straight: return "Straight";
            case HandRanking.ThreeOfAKind: return "Three of a Kind";
            case HandRanking.TwoPairs: return "Two Pair";
            case HandRanking.TwoOfAKind: return "A Pair";
            case HandRanking.HighCard: return "High Card";
            default:
                return $"MISSING \"{_handRanking}\"";
        }
    }

    internal HandValue(List<Card> cards)
    {
        _handRanking = HandRanking.HighCard;
        _highCard = cards.First();
        _fractionalValue = ComputeFractionalValue(cards);
    }

    internal HandValue(HandRanking ranking, List<Card> cards)
    {
        _handRanking = ranking;
        _highCard = cards.First();
        _fractionalValue = ComputeFractionalValue(cards);
    }

    private double ComputeFractionalValue(List<Card> cards)
    {
        double total = 0;
        foreach (Card card in cards)
        {
            total *= Card.MaxFractionalValue;
            total += card.FractionalValue;
        }

        return total;
    }

    /// <summary>
    /// Best scores are sorted to the back
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(HandValue? other)
    {
        if (other == null)
            return -1;

        if (_handRanking != other._handRanking)
        {
            return (_handRanking > other._handRanking) ? 1 : -1;
        }

        int comp = _highCard.CompareTo(other._highCard);
        if (comp != 0)
            return comp;

        return _fractionalValue.CompareTo(other._fractionalValue);
    }
}