using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

public partial class Main : Node
{
    private List<Player> _players = new List<Player>();
    private Deal? _deal = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        for (int i = 0; i < 5; ++i)
        {
            _players.Add(new Player());
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void Test()
    {
        Player player = new Player();
        List<Hand> hands = new List<Hand>();
        Suit suitA = Suit.DefaultSuits[0];
        Suit suitB = Suit.DefaultSuits[1];
        Suit suitC = Suit.DefaultSuits[2];
        Suit suitD = Suit.DefaultSuits[3];
        Rank rank2 = Rank.DefaultRanks[0];
        Rank rank3 = Rank.DefaultRanks[1];
        Rank rank4 = Rank.DefaultRanks[2];
        Rank rank5 = Rank.DefaultRanks[3];
        Rank rank6 = Rank.DefaultRanks[4];
        Rank rank7 = Rank.DefaultRanks[5];
        Rank rank8 = Rank.DefaultRanks[6];
        Rank rank9 = Rank.DefaultRanks[7];
        Rank rank10 = Rank.DefaultRanks[8];
        Rank rankJ = Rank.DefaultRanks[9];
        Rank rankQ = Rank.DefaultRanks[10];
        Rank rankK = Rank.DefaultRanks[11];
        Rank rankA = Rank.DefaultRanks[12];
        { // straight
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitA, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitA, rankQ));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Nothing
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Nothing
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitC, rankK));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Nothing
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitD, rankA));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Two of a kind
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank4));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Two Pair
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank4));
            hand.AddCard(new Card(suitD, rankJ));
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitC, rankQ));
            hand.AddCard(new Card(suitA, rankQ));
            hands.Add(hand);
        }
        { // Three of a kind
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank5));
            hand.AddCard(new Card(suitA, rank5));
            hand.AddCard(new Card(suitC, rank5));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Full House
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank3));
            hand.AddCard(new Card(suitA, rank3));
            hand.AddCard(new Card(suitC, rank3));
            hand.AddCard(new Card(suitC, rank8));
            hand.AddCard(new Card(suitA, rank8));
            hands.Add(hand);
        }
        { // Four of a kind
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank6));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // flush
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitC, rank3));
            hand.AddCard(new Card(suitA, rankA));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitD, rank5));
            hands.Add(hand);
        }
        { // flush
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitB, rank10));
            hand.AddCard(new Card(suitC, rank9));
            hand.AddCard(new Card(suitA, rank8));
            hand.AddCard(new Card(suitD, rank7));
            hands.Add(hand);
        }
        { // straight flush
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitC, rank8));
            hand.AddCard(new Card(suitC, rank4));
            hand.AddCard(new Card(suitC, rank5));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitC, rank7));
            hands.Add(hand);
        }
        foreach (Hand hand in hands)
        {
            hand.ComputeBestScore(rank2._strength, rankK._strength);
        }
        hands.Sort();
        GD.Print("Worst to best:");
        foreach (Hand hand in hands)
        {
            GD.Print(hand.ToString());
        }
    }

    internal void StartFreshDeal()
    {
        _deal = new Deal(_players, new Random((int)DateTime.Now.Ticks));
        _deal.Dump();

        //Test();
    }

    internal state_machine GetStateMachine()
    {
        if (FindChild("StateMachine") is state_machine sm)
        {
            return sm;
        }

        throw new Exception($"{Name} does not have child state_machine");
    }
}

class Player
{
}

class Suit : IComparable<Suit>
{
    internal enum SuitColor {
        Red,
        Black,
        Blue,
    }

    internal static List<Suit> DefaultSuits = new List<Suit>() { 
        new Suit('\u2660', SuitColor.Black, "Spade"),
        new Suit('\u2661', SuitColor.Red, "Heart"),
        new Suit('\u2662', SuitColor.Red, "Diamond"),
        new Suit('\u2663', SuitColor.Black, "Club"),
    };

    static internal Suit Ankh = new Suit('\u2625', SuitColor.Blue, "Life");
    static internal Suit Swords = new Suit('\u2694', SuitColor.Blue, "Death");

    internal readonly string _unicode;
    internal readonly SuitColor _color;
    internal readonly string _sortOrder;

    internal Suit(string unicode, SuitColor color, string sortOrder)
    {
        _unicode = new string(unicode);
        _color = color;
        _sortOrder = sortOrder;
    }

    internal Suit(char ascii, SuitColor color, string sortOrder)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ascii);
        _unicode = sb.ToString();

        _color = color;
        _sortOrder = sortOrder;
    }

    public int CompareTo(Suit? other)
    {
        if (other == null)
            return -1;

        return _sortOrder.CompareTo(other._sortOrder);
    }
}

class Rank : IComparable<Rank>
{
    internal static List<Rank> DefaultRanks = new List<Rank>() {
        new Rank('2', 2),
        new Rank('3', 3),
        new Rank('4', 4),
        new Rank('5', 5),
        new Rank('6', 6),
        new Rank('7', 7),
        new Rank('8', 8),
        new Rank('9', 9),
        new Rank("10", 10),
        new Rank('J', 11),
        new Rank('Q', 12),
        new Rank('K', 13),
        new Rank('A', true),
    };

    internal static Rank Sadness = new Rank('\u2639', 0);
    internal static Rank Skull = new Rank('\u2620', 1);
    internal static Rank Saturn = new Rank('\u2644', 14);
    internal static Rank Jupiter = new Rank('\u2643', 15);

    internal bool Wraps { get; private set; }

    readonly internal int _strength;
    readonly internal string _unicode;

    internal Rank(char ascii, bool wraps)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ascii);
        _unicode = sb.ToString();
        Wraps = wraps;
        _strength = -1;
    }

    internal Rank(char ascii, int strength)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ascii);
        _unicode = sb.ToString();
        Wraps = false;
        _strength = strength;
    }

    internal Rank(string unicode, int strength)
    {
        _unicode = unicode;
        Wraps = false;
        _strength = strength;
    }

    public int CompareTo(Rank? other)
    {
        if (other == null)
            return -1;

        if (Wraps != other.Wraps)
        {
            return Wraps ? 1 : -1;
        }

        return _strength.CompareTo(other._strength);
    }

    internal bool IsNeighborTo(Rank other, int min, int max)
    {
        if (other.Wraps == Wraps)
        {
            int diff = Math.Abs(_strength - other._strength);
            return diff == 1;
        }

        if (Wraps)
        {
            return (other._strength == min || other._strength == max);
        }
        else
        {
            return (_strength == min || _strength == max);
        }
    }

    internal static void ExtractMinAndMax(List<Rank> ranks, out int minRank, out int maxRank)
    {
        minRank = int.MaxValue;
        maxRank = int.MinValue;
        foreach(Rank rank in ranks)
        {
            if (minRank < rank._strength)
                minRank = rank._strength;
            if (maxRank > rank._strength)
                maxRank = rank._strength;
        }
    }
}

class Card : IComparable<Card> 
{
    public Rank Rank { get; private set; }
    public Suit Suit { get; private set; }

    internal Card(Suit suit, Rank rank)
    {
        Rank = rank;
        Suit = suit;
    }

    public override string ToString()
    {
        return $"{Rank._unicode}{Suit._unicode}";
    }

    public int CompareTo(Card? other)
    {
        if (other == null)
            return -1;

        int comp = Rank.CompareTo(other.Rank);
        if (comp != 0)
            return comp;
        else
            return Suit.CompareTo(other.Suit);
    }
}

class Score : IComparable<Score>
{
    internal enum HandRanking
    {
        FiveOfAKind, // Only possible with more suits
        StraightFlush, // includes royal flush
        FourOfAKind,
        FullHouse,
        Flush, // LESS COMMON with more suits
        Straight,
        ThreeOfAKind,
        TwoPairs,
        TwoOfAKind,
        HighCard,
    };

    internal HandRanking _handRanking;
    internal Card _primaryHighCard;
    internal Card _secondaryHighCard;

    internal Score(Card highCard)
    {
        _handRanking = HandRanking.HighCard;
        _primaryHighCard = highCard;
        _secondaryHighCard = highCard;
    }

    internal Score(HandRanking ranking, Card highCard)
    {
        _handRanking = ranking;
        _primaryHighCard = highCard;
        _secondaryHighCard = highCard;
    }

    internal Score(HandRanking ranking, Card primaryHighCard, Card secondaryHighCard)
    {
        _handRanking = ranking;
        _primaryHighCard = primaryHighCard;
        _secondaryHighCard = secondaryHighCard;
    }

    public int CompareTo(Score? other)
    {
        if (other == null)
            return -1;

        if (_handRanking != other._handRanking)
        {
            return (_handRanking < other._handRanking) ? 1 : -1;
        }

        int comp = _primaryHighCard.CompareTo(other._primaryHighCard);
        if (comp != 0)
            return comp;

        return _secondaryHighCard.CompareTo(other._secondaryHighCard);
    }
}

class Hand : IComparable<Hand>
{
    readonly internal Player _player;
    internal List<Card> _cards = new List<Card>();
    internal Score? _bestScore = null;

    internal Hand(Player player)
    {
        _player = player;
    }

    internal void AddCard(Card card)
    {
        _cards.Add(card);
    }

    internal bool IsStraight(int minRank, int maxRank)
    {
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
        Card highCard = _cards.OrderByDescending(a => a).First();
        _bestScore = new Score(highCard);
        bool isStraight = IsStraight(minRank, maxRank);
        bool isFlush = IsFlush;
        if (isStraight)
        {
            if (isFlush)
            {
                _bestScore = new Score(Score.HandRanking.StraightFlush, highCard);
            }
            else
            {
                _bestScore = new Score(Score.HandRanking.Straight, highCard);
            }
        }
        else if (isFlush)
        {
            _bestScore = new Score(Score.HandRanking.Flush, highCard);
        }

        ExtractOfAKind(_cards, out List<Card> ofAKind, out List<Card> remainder);
        if (ofAKind.Count > 0)
        {
            Card ofAKindHighCard = ofAKind.OrderByDescending(a => a).First();
            Score? possiblyBetter = null;
            if (ofAKind.Count == 5)
            {
                possiblyBetter = new Score(Score.HandRanking.FiveOfAKind, ofAKindHighCard);
            }
            else if (ofAKind.Count == 4)
            {
                Card remainderHighCard = remainder.OrderByDescending(a => a).First();
                possiblyBetter = new Score(Score.HandRanking.FourOfAKind, ofAKindHighCard, remainderHighCard);
            }
            else
            {
                ExtractOfAKind(remainder, out List<Card> secondOfAKind, out List<Card> _);
                if (ofAKind.Count == 3)
                {
                    if (secondOfAKind.Count > 0)
                    {
                        Card remainderHighCard = secondOfAKind.OrderByDescending(a => a).First();
                        possiblyBetter = new Score(Score.HandRanking.FullHouse, ofAKindHighCard, remainderHighCard);
                    }
                    else
                    {
                        Card remainderHighCard = remainder.OrderByDescending(a => a).First();
                        possiblyBetter = new Score(Score.HandRanking.ThreeOfAKind, ofAKindHighCard, remainderHighCard);
                    }
                }
                else if (ofAKind.Count == 2)
                {
                    if (secondOfAKind.Count > 0)
                    {
                        Card remainderHighCard = secondOfAKind.OrderByDescending(a => a).First();
                        possiblyBetter = new Score(Score.HandRanking.TwoPairs, ofAKindHighCard, remainderHighCard);
                    }
                    else
                    {
                        Card remainderHighCard = remainder.OrderByDescending(a => a).First();
                        possiblyBetter = new Score(Score.HandRanking.TwoOfAKind, ofAKindHighCard, remainderHighCard);
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
                    if (_bestScore._handRanking == Score.HandRanking.HighCard)
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
            sb.Append($": ({_bestScore._handRanking})");
        else
            sb.Append(':');
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

}

class Deal
{
    internal List<Suit> _suits = new List<Suit>();
    internal List<Rank> _ranks = new List<Rank>();
    internal List<Card> _drawPile = new List<Card>();
    internal List<Hand> _hands = new List<Hand>();

    internal Deal(List<Player> players, Random rnd)
    {
        _suits.AddRange(Suit.DefaultSuits);
        //_suits.RemoveAll(suit => suit._color != Suit.SuitColor.Red);

        //_suits.Add(Suit.Ankh);
        //_suits.Add(Suit.Swords);

        _ranks.AddRange(Rank.DefaultRanks);

        //_ranks.Add(Rank.Sadness);
        //_ranks.Add(Rank.Skull);
        _ranks.Add(Rank.Saturn);
        //_ranks.Add(Rank.Jupiter);

        Rank.ExtractMinAndMax(_ranks, out int minRank, out int maxRank);

        foreach (Suit suit in _suits)
        {
            foreach (Rank rank in _ranks)
            {
                Card card = new Card(suit, rank);
                _drawPile.Add(card);
            }
        }

        _drawPile = _drawPile.Shuffle(rnd).ToList();

        //Test(rnd, players[0]);
        foreach (Player player in players)
        {
            Hand hand = new Hand(player);
            for (int i = 0; i < 5; ++i)
            {
                hand.AddCard(_drawPile.First());
                _drawPile.RemoveAt(0);
            }

            hand.ComputeBestScore(minRank, maxRank);

            _hands.Add(hand);
        }
    }

    private void Test(Random rnd, Player player)
    {
        Rank.ExtractMinAndMax(_ranks, out int minRank, out int maxRank);
        int flush = 0;
        int straight = 0;
        int straightFlush = 0;
        Dictionary<Tuple<int, int>, int> kindCount = new Dictionary<Tuple<int, int>, int>();
        for (int i = 0; i < 1250000; ++i)
        {
            var newPile = _drawPile.Shuffle(rnd).ToList();
            Hand hand = new Hand(player);
            for (int j = 0; j < 5; ++j)
            {
                hand.AddCard(newPile[j]);
            }

            hand.CountTypes(minRank, maxRank, out bool isFlush, out bool isStraight, out int primaryOfAKind, out int secondaryOfAKind);
            if (isFlush)
                ++flush;
            if (isStraight)
                ++straight;
            if (isStraight && isFlush)
                ++straightFlush;
            var key = Tuple.Create(primaryOfAKind, secondaryOfAKind);
            if (kindCount.TryGetValue(key, out int count))
            {
                kindCount[key] = count + 1;
            }
            else
            {
                kindCount[key] = 1;
            }
        }

        GD.Print($"Flush: {flush}");
        GD.Print($"Straight: {straight}");
        GD.Print($"Straight Flush: {straightFlush}");
        foreach (var entry in kindCount)
        {
            GD.Print($"{entry.Key}: {entry.Value}");
        }
    }

    private void Test2(Random rnd, Player player)
    {
        Rank.ExtractMinAndMax(_ranks, out int minRank, out int maxRank);
        Hand? bestHand = null;
        Hand? worstHand = null;
        for (int i = 0; i < 25000; ++i)
        {
            var newPile = _drawPile.Shuffle(rnd).ToList();
            Hand hand = new Hand(player);
            for (int j = 0; j < 5; ++j)
            {
                hand.AddCard(newPile[j]);
            }

            hand.ComputeBestScore(minRank, maxRank);
            if (bestHand == null || hand.CompareTo(bestHand) > 0)
            {
                bestHand = hand;
            }
            if (worstHand == null || hand.CompareTo(worstHand) < 0)
            {
                worstHand = hand;
            }
        }

        GD.Print($"Best Hand: {bestHand}");
        GD.Print($"Worst Hand: {worstHand}");
    }

    public void Dump()
    {
        GD.Print("Sorted hands:");

        _hands.Sort();
        _hands.Reverse();

        foreach(Hand hand in _hands)
        {
            GD.Print(hand);
        }
    }
}

public static class ExtensionMethods
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
    {
        return source.Select(a => Tuple.Create(rnd.Next(), a)).ToList().OrderBy(a => a.Item1).Select(a => a.Item2);
    }
}
