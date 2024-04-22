using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

public partial class Main : Node
{
    Random rnd = new Random((int)DateTime.Now.Ticks);
    private List<Player> _players = new List<Player>();
    private Deal? _deal = null;

    private int startingPlayer = 0;
    private int currentBetLimit = 1;
    private int currentBetPlayer = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        for (int i = 0; i < 5; ++i)
        {
            _players.Add(new Player(i));
        }

        startingPlayer = 1; // Should advance each round
        currentBetLimit = 1;
        currentBetPlayer = 0;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void Test(Random rnd)
    {
        Player player = new Player(0);
        
        Suit suitA = Suit.DefaultSuits[0];
        Suit suitB = Suit.DefaultSuits[1];
        Suit suitC = Suit.DefaultSuits[2];
        Suit suitD = Suit.DefaultSuits[3];

        List<Suit> suits = new List<Suit>()
        {
            suitA, suitB, suitC, suitD
        };

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

        List<Rank> ranks = new List<Rank>()
        {
            rank2,
            rank3,
            rank4,
            rank5,
            rank6,
            rank7,
            rank8,
            rank9,
            rank10,
            rankJ,
            rankQ,
            rankK,
            rankA,
        };

        Rank.ExtractMinAndMax(ranks, out int minRank, out int maxRank);

        { // A 9 4 3 2
            Hand hand = new Hand(player);

            //Card cardAce = new Card(suitA, rankA);
            //Card cardFour = new Card(suitC, rank4);
            //Card cardThree = new Card(suitD, rank3);

            hand.AddCard(new Card(suitA, rankA));
            hand.AddCard(new Card(suitB, rankA));
            hand.AddCard(new Card(suitC, rankQ));
            hand.AddCard(new Card(suitA, rank9));
            hand.AddCard(new Card(suitA, rank3));
            hand.ComputeBestScore(minRank, maxRank);

            GD.Print($"Evaluating {hand}");

            List<Card> availableCards = new List<Card>();
            foreach(Rank rank in ranks)
            {
                foreach(Suit suit in suits)
                {
                    Card c = new Card(suit, rank);
                    bool addCard = true;
                    foreach (Card card in hand._cards)
                    {
                        if (c.CompareTo(card) == 0)
                        {
                            addCard = false;
                        }
                    }

                    if (addCard)
                    {
                        availableCards.Add(c);
                    }
                }
            }

            //List<Tuple<Hand, Card, Card>> sortedList = new List<Tuple<Hand, Card, Card>>();
            //foreach (Card discardCard in new List<Card>() { cardNine, cardTwo })
            //{
            //    foreach (Card replacement in availableCards)
            //    {
            //        Hand potentialHand = hand.CloneWithDiscard(discardCard, replacement);
            //        potentialHand.ComputeBestScore(minRank, maxRank);
            //        sortedList.Add(Tuple.Create(potentialHand, discardCard, replacement));
            //    }
            //}

            //sortedList.Sort((a,b) => {
            //    return a.Item1.CompareTo(b.Item1);
            //});

            //foreach (var entry in sortedList)
            //{
            //    GD.Print($"Replacing {entry.Item2} for {entry.Item1}: {entry.Item1.GetDesc()}");
            //}

            List<Tuple<AggregateValue, string, TimeSpan>> choices = new List<Tuple<AggregateValue, string, TimeSpan>>();
            foreach (int i in new List<int>(){ 2, 3, 3, 3, 3})
            {
                DateTime start = DateTime.Now;
                Tuple<AggregateValue, List<Card>> discard = hand.SelectDiscards(i, availableCards, minRank, maxRank, rnd);
                TimeSpan dur = DateTime.Now - start;
                StringBuilder cardsAsText = new StringBuilder();
                if (discard.Item2.Count == 0)
                {
                    cardsAsText.Append("nothing");
                }
                else
                {
                    foreach (Card card in discard.Item2)
                    {
                        if (cardsAsText.Length > 0)
                            cardsAsText.Append(", ");
                        cardsAsText.Append(card.ToString());
                    }
                }

                choices.Add(Tuple.Create(discard.Item1, $"Discarding {i} cards: trying for {discard.Item1.GetDesc()} by discarding {cardsAsText}", dur));
            }

            choices.Sort((a,b) => a.Item1.CompareTo(b.Item1));
            choices.Reverse();
            foreach (var choice in choices)
            {
                GD.Print($"{choice.Item2} time={choice.Item3.TotalSeconds:F2}");
            }
        }
    }

    private void Test1()
    {
        Player player = new Player(0);
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

        //Test(rnd);

        _deal = new Deal(_players, rnd);
        _deal.UpdateHUD(GetHUD());
    }

    internal HUD GetHUD()
    {
        if (FindChild("HUD") is HUD hud)
        {
            return hud;
        }

        throw new Exception($"{Name} does not have child HUD");
    }

    internal state_machine GetStateMachine()
    {
        if (FindChild("StateMachine") is state_machine sm)
        {
            return sm;
        }

        throw new Exception($"{Name} does not have child state_machine");
    }

    public bool SomeoneNeedsToDiscard()
    {
        foreach (Player player in _players)
        {
            if (!player.HasDiscarded)
            {
                return true;
            }
        }

        return false;
    }

    public void ForceSomeoneToDiscard()
    {
        if (_deal == null)
            throw new Exception("Can force someone to discard if there is no deal");

        for (int i = 0; i < _players.Count; ++i)
        {
            int j = (startingPlayer + i) % _players.Count;
            if (!_players[j].HasDiscarded)
            {
                Hand hand = _deal.GetPlayerHand(_players[j]);
                _players[j].Discards = hand.SelectDiscards(0, 3, _deal, rnd);
                _players[j].HasDiscarded = true;
                //hud.SetHandDiscards(hand);

                GetStateMachine().SwitchState("Play_Animate_Discards");

                return;
            }
        }

        throw new Exception("There was no player awaiting discard");
    }

    public bool ProgressDiscardAnimation()
    {
        if (_deal == null)
            throw new Exception("Can force someone to animate discard if there is no deal");

        HUD hud = GetHUD();
        foreach (Player player in _players)
        {
            if (player.Discards != null && player.Discards.Count > 0)
            {
                _deal.MoveCardToDiscard(hud, player, player.Discards.First());
                player.Discards.RemoveAt(0);
                return true;
            }
        }

        if (_deal.ProgressReplaceDiscard(hud))
            return true;

        return false;
    }

    public bool SomeoneNeedsToBet()
    {
        int consider = _players.Count;

        while (consider > 0)
        {
            if (_players[currentBetPlayer].HasFolded)
            {
                consider -= 1;
                currentBetPlayer = (currentBetPlayer + 1) % _players.Count;
                continue;
            }

            if (_players[currentBetPlayer].AmountBet == currentBetLimit)
            {
                consider -= 1;
                currentBetPlayer = (currentBetPlayer + 1) % _players.Count;
                continue;
            }

            if (_players[currentBetPlayer].AmountBet > currentBetLimit)
                throw new Exception($"Why is player #{currentBetPlayer} got more bet ({_players[currentBetPlayer]}) than the current bet limit ({currentBetLimit})?");

            return true;
        }

        return false;
    }

    public void ForceNextBet()
    {

    }

    public bool SomeoneNeedsToReveal()
    {
        foreach (Player player in _players)
        {
            if (!player.HasRevealed)
            {
                return true;
            }
        }

        return false;
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
            StringBuilder sb = new  StringBuilder();
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
            maxReads = (int) Math.Round(1999 * maxReads / 2000f);
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

        for (i = 0; i<maxReads; ++i)
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
        _normalizedWealth += totalValue / (double) setSize;

        if (_normalizedByRank.TryGetValue(handValue._handRanking, out Tuple<double, int>? value))
        {
            _normalizedByRank[handValue._handRanking] = Tuple.Create((totalValue / (double) setSize) + value.Item1, value.Item2 + 1);
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

        //if (_hopefulValue == null && other._hopefulValue == null)
        //    return false;
        //if (other._hopefulValue == null)
        //    return false;
        //if (_hopefulValue == null || _hopefulValue.CompareTo(other._hopefulValue) < 0)
        //{
        //    _hopefulValue = other._hopefulValue;
        //    return true;
        //}

        //return false;
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

class Player
{
    internal int PositionID { get; private set; }
    internal bool IsNPC { get; private set; }
    internal bool HasDiscarded { get; set; }
    internal bool HasFolded { get; private set; }
    internal int AmountBet { get; private set; }
    internal bool HasRevealed { get; private set; }
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
}

class Suit : IComparable<Suit>
{
    internal enum SuitColor {
        Red,
        Black,
        Blue,
    }

    internal static List<Suit> DefaultSuits = new List<Suit>() { 
        new Suit('\u2660', SuitColor.Black, "Spade", 0.80),
        new Suit('\u2661', SuitColor.Red, "Heart", 0.50),
        new Suit('\u2662', SuitColor.Red, "Diamond", 0.25),
        new Suit('\u2663', SuitColor.Black, "Club", 0.10),
    };

    static internal Suit Skull = new Suit('\u2620', SuitColor.Blue, "Death", 0.20);
    static internal Suit Swords = new Suit('\u2694', SuitColor.Blue, "Swords", 0.90);

    internal readonly string _unicode;
    internal readonly SuitColor _color;
    internal readonly string _text;
    internal double FractionalValue { get; private set; }
    internal static double MaxFractionalValue { get { return 0.95; } }

    internal Suit(string unicode, SuitColor color, string text, double fractionalValue)
    {
        _unicode = new string(unicode);
        _color = color;
        _text = text;
        FractionalValue = fractionalValue;
    }

    internal Suit(char ascii, SuitColor color, string text, double fractionalValue)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ascii);
        _unicode = sb.ToString();

        _color = color;
        _text = text;
        FractionalValue = fractionalValue;
    }

    public int CompareTo(Suit? other)
    {
        if (other == null)
            return -1;

        return FractionalValue.CompareTo(other.FractionalValue);
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

    internal bool FaceCard { get { return _strength > 10; } }

    internal bool IsTenOrHigher()
    {
        return _strength >= 10;
    }

    internal static Rank Sadness = new Rank('\u2639', 0);
    internal static Rank Ankh = new Rank('\u2625', 1);
    internal static Rank Saturn = new Rank('\u2644', 14);
    internal static Rank Jupiter = new Rank('\u2643', 15);

    internal bool Wraps { get; private set; }

    internal static double MaxFractionalValue { get { return 15; } }
    internal double FractionalValue { get { return Wraps ? MaxFractionalValue : _strength; } }
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
            if (rank.Wraps)
                continue;

            if (minRank > rank._strength)
                minRank = rank._strength;
            if (maxRank < rank._strength)
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

    public static double MaxFractionalValue { get { return Rank.MaxFractionalValue + Suit.MaxFractionalValue; } }
    public double FractionalValue { get { return Rank.FractionalValue + Suit.FractionalValue; } }

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

class Hand : IComparable<Hand>
{
    readonly internal Player _player;
    internal List<Card> _cards = new List<Card>();
    internal HandValue? _bestScore = null;

    public int PositionID { get { return _player.PositionID; } }

    internal Hand(Player player)
    {
        _player = player;
    }

    internal bool IsVisible(Card card)
    {
        if (_player.IsNPC)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsEveryCardVisible
    {
        get
        {
            foreach (Card card in _cards)
            {
                if (!IsVisible(card))
                    return false;
            }

            return true;
        }
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

class Deal
{
    internal List<Suit> _suits = new List<Suit>();
    internal List<Rank> _ranks = new List<Rank>();
    internal List<Card> _drawPile = new List<Card>();
    internal List<Hand> _hands = new List<Hand>();
    internal List<Card> _discards = new List<Card> ();

    internal Deal(List<Player> players, Random rnd)
    {
        _suits.AddRange(Suit.DefaultSuits);

        //_suits.RemoveAll(suit => suit._color != Suit.SuitColor.Red);
        //_suits.Add(Suit.Skull);
        //_suits.Add(Suit.Swords);

        _ranks.AddRange(Rank.DefaultRanks);

        //_ranks.Add(Rank.Sadness);
        //_ranks.Add(Rank.Ankh);
        //_ranks.Add(Rank.Saturn);
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

    internal void UpdateHUD(HUD hud)
    {
        foreach (Hand hand in _hands)
        {
            hud.SetVisibleHand(hand);
        }
    }

    //internal List<Card> SelectDiscards(Player player, int minDiscards, int maxDiscards, Random rnd)
    //{
    //    Hand hand = _hands.First(a => a._player == player);
    //    return hand.SelectDiscards(minDiscards, maxDiscards, this, rnd);
    //    hud.SetHandDiscards(hand, discards);
    //}

    internal List<Card> AvailableCardsFromHandsView(Hand viewHand)
    {
        List<Card> retVal = new List<Card>();
        retVal.AddRange(_drawPile);
        foreach (Hand hand in _hands)
        {
            if (hand == viewHand)
                continue;
            foreach (Card card in hand._cards)
            {
                if (!hand.IsVisible(card))
                {
                    retVal.Add(card);
                }
            }
        }

        return retVal;
    }

    internal Hand GetPlayerHand(Player player)
    {
        return _hands.First(a => a._player == player);
    }

    internal void MoveCardToDiscard(HUD hud, Player player, Card card)
    {
        Hand hand = GetPlayerHand(player);
        hand._cards.Remove(card);
        _discards.Add(card);
        hud.SetVisibleHand(hand);
        hud.MoveCardToDiscard(player.PositionID, card);
    }

    internal bool ProgressReplaceDiscard(HUD hud)
    {
        foreach (Hand hand in _hands)
        {
            if (hand._cards.Count < 5)
            {
                Card card = _drawPile.First();
                _drawPile.RemoveAt(0);
                hand._cards.Add(card);
                hud.SetVisibleHand(hand);
                return true;
            }
        }

        return false;
    }
}

public static class ExtensionMethods
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
    {
        return source.Select(a => Tuple.Create(rnd.Next(), a)).ToList().OrderBy(a => a.Item1).Select(a => a.Item2);
    }
}
