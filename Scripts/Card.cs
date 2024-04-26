using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

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

class Suit : IComparable<Suit>
{
    internal enum SuitColor
    {
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

    // frowny face = '\u2639'
    internal static Rank Anhk = new Rank('\u2625', 0); 
    internal static Rank Lead = new Rank('\u2644', 1);
    internal static Rank Empress = new Rank('E', 14);
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

    internal static void ExtractMinAndMax(List<Rank> ranks, List<Suit> suits, out int minRank, out int maxRank, out int suitsCount)
    {
        minRank = int.MaxValue;
        maxRank = int.MinValue;
        suitsCount = suits.Count;
        foreach (Rank rank in ranks)
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