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

    internal void StartFreshDeal()
    {
        _deal = new Deal(_players, new Random());
        GD.Print($"{_deal}");
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

class Suit
{
    internal enum SuitColor {
        Red,
        Black,
    }

    internal static List<Suit> DefaultSuits = new List<Suit>() { 
        new Suit('\u2660', SuitColor.Black),
        new Suit('\u2661', SuitColor.Red),
        new Suit('\u2662', SuitColor.Red),
        new Suit('\u2663', SuitColor.Black),
    };

    static internal Suit Asclepius = new Suit('\u2695', SuitColor.Red);
    static internal Suit Swords = new Suit('\u2694', SuitColor.Black);

    internal readonly string _unicode;
    internal readonly SuitColor _color;

    internal Suit(string unicode, SuitColor color)
    {
        _unicode = new string(unicode);
        _color = color;
    }

    internal Suit(char ascii, SuitColor color)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ascii);
        _unicode = sb.ToString();

        _color = color;
    }
}

class Rank
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

    readonly internal int _strength;
    readonly internal string _unicode;
    readonly internal bool _wraps;

    internal Rank(char ascii, bool wraps)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ascii);
        _unicode = sb.ToString();
        _wraps = wraps;
        _strength = -1;
    }

    internal Rank(char ascii, int strength)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ascii);
        _unicode = sb.ToString();
        _wraps = false;
        _strength = strength;
    }

    internal Rank(string unicode, int strength)
    {
        _unicode = unicode;
        _wraps = false;
        _strength = strength;
    }
}

class Card
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
}

class Deal
{
    internal List<Suit> _suits = new List<Suit>();
    internal List<Rank> _ranks = new List<Rank>();
    internal List<Card> _cards = new List<Card>();

    internal Deal(List<Player> player, Random rnd)
    {
        _suits.AddRange(Suit.DefaultSuits);

        _suits.Add(Suit.Asclepius);
        _suits.Add(Suit.Swords);

        _ranks.AddRange(Rank.DefaultRanks);

        _ranks.Add(Rank.Sadness);
        _ranks.Add(Rank.Skull);
        _ranks.Add(Rank.Saturn);
        _ranks.Add(Rank.Jupiter);

        foreach (Suit suit in _suits)
        {
            foreach (Rank rank in _ranks)
            {
                Card card = new Card(suit, rank);
                _cards.Add(card);
            }
        }

        _cards = _cards.Shuffle(rnd).ToList();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i<15; ++i)
        {
            if (i > 0)
                sb.Append(" ");
            sb.Append(_cards[i]);
        }

        return sb.ToString();
    }
}

public static class ExtensionMethods
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
    {
        return source.Select(a => Tuple.Create(rnd.Next(), a)).ToList().OrderBy(a => a.Item1).Select(a => a.Item2);
    }
}
