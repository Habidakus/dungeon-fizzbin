using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

class Species
{
    internal enum Bark
    {
        LeavingPoor,
        LeavingRich,
    };

    private delegate void DealComponent(Deal deal);
    private delegate string SpeciesNameGenerator(Random rnd);
    private delegate bool SpeciesAllowed(Deal deal);
    private delegate string SpeciesText(Player player, Bark bark);

    private int _introHand = 0;
    private double _weight = 1;
    private double Weight { get { return (Main.HandNumber < _introHand) ? 0 : _weight; } }
    public String Name { get; private set; }
    private SpeciesNameGenerator? _nameGenerator;
    private readonly DealComponent _dealComponent;
    private readonly SpeciesAllowed? _allowed;
    private SpeciesText? _speciesText;

    private Species(String name, double weight, int introHand, DealComponent dealComponent, SpeciesNameGenerator? sng = null, SpeciesAllowed? allowed = null, SpeciesText? speciesText = null)
    {
        Name = name;
        _weight = weight;
        _introHand = introHand;
        _nameGenerator = sng;
        _dealComponent = dealComponent;
        _allowed = allowed;
        _speciesText = speciesText; 
    }

    internal static Species Get(string name)
    {
        if (AllSpecies == null)
        {
            InitSpeciesList();
        }
        return AllSpecies!.Where(s => s.Name.ToLower().CompareTo(name.ToLower()) == 0).First();
    }

    static private List<Species>? AllSpecies = null;
    private static void InitSpeciesList()
    {
        AllSpecies = new List<Species>() {
            new Species("Human", 0.5, 0, DealComponent_Human, NameGenerator_Human, null, GetText_Human),
            new Species("Elf", 1, 0, DealComponent_Elf, NameGenerator_Elf, null, GetText_Elf),
            new Species("Dwarf", 0.25, 0, DealComponent_Dwarf, NameGenerator_Dwarf, null, GetText_Dwarf),
            new Species("Goblin", 0.5, 0, DealComponent_Goblin, NameGenerator_Goblin, CanAdd_Goblin, GetText_Greenskin),
            new Species("Dragonkin", 1, 5, DealComponent_Dragonkin, NameGenerator_Dragonkin, null, GetText_Dragonkin),
            new Species("Troll", 1, 10, DealComponent_Troll, NameGenerator_Troll, CanAdd_Troll, GetText_Greenskin),
            new Species("Lizardman", 1, 10, DealComponent_Lizardman, NameGenerator_Lizardman),
            new Species("Orc", 1, 15, DealComponent_Orc, NameGenerator_Orc, CanAdd_Orc, GetText_Greenskin),
            new Species("Halfling", 1, 15, DealComponent_Halfling, NameGenerator_Halfling, null, GetText_Halfling),
            new Species("Centaur", 0.5, 15, DealComponent_Centaur, NameGenerator_Centaur, null, GetText_Centaur),
            new Species("Pixie", 1, 15, DealComponent_Pixie, NameGenerator_Pixie, CanAdd_Pixie),
            new Species("Giant", 1, 15, DealComponent_Giant, NameGenerator_Giant, null, GetText_Giant),
            //new Species("Ghoul", 1, 15),
            //new Species("Dogman", 1, 15),
            new Species("Birdman", 1, 20, DealComponent_Birdman, NameGenerator_Birdman, CanAdd_Birdman, GetText_Birdman),
            new Species("Firbolg", 1, 30, DealComponent_Firbolg, NameGenerator_Firbolg, null, GetText_Firbolg),
            //new Species("Golem", 1, 30),
            //new Species("Lich", 0.25, 30),
            //new Species("Vampire", 0.5, 30),
        };
    }

    static public Species Human
    {
        get
        {
            if (AllSpecies == null)
            {
                InitSpeciesList();
            }

            return AllSpecies!.Where(n => n.Name == "Human").First();
        }
    }

    internal string GetLeavingText(Player player, bool becauseTheyArePoor)
    {
        Bark bark = becauseTheyArePoor ? Bark.LeavingPoor : Bark.LeavingRich;
        return (_speciesText == null) ? GenericText(player, bark) : _speciesText(player, bark);
    }

    private string GenericText(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "I'm out...";
            case Bark.LeavingRich: return "I'll leave while I'm ahead.";
            default:
                throw new Exception($"No generic text for bark={bark}");
        }
    }

    internal void ApplyDealComponent(Deal deal)
    {
        _dealComponent(deal);
    }

    private double CalculateSelectionWeight(List<Species> speciesAlreadyAtTable, Deal deal)
    {
        if (_allowed != null && !_allowed(deal))
            return 0;
        int count = speciesAlreadyAtTable.Where(a => a == this).Count();
        if (count > 1)
            return 0;
        else if (count == 1)
            return Weight / 2.0;
        else
            return Weight;
    }

    static public Species PickSpecies(Random rnd, List<Species> speciesAlreadyAtTable, Deal deal)
    {
        if (AllSpecies == null)
        {
            InitSpeciesList();
        }

        double index = rnd.NextDouble() * AllSpecies!.Sum(a => a.CalculateSelectionWeight(speciesAlreadyAtTable, deal));
        foreach(Species species in AllSpecies!)
        {
            index -= species.CalculateSelectionWeight(speciesAlreadyAtTable, deal);
            if (index <= 0)
                return species;
        }

        throw new Exception("bad roll for species");
    }

    internal string GenerateRandomName(Random rng, int positionId)
    {
        if (_nameGenerator == null)
            return $"NPC #{positionId}";
        else
            return _nameGenerator(rng);
    }

    // -------------------------------- HUMAN --------------------------------

    static private List<string> HUMAN_FIRST_NAMES = new List<string>() {
        "Agatha", "Bartholomew", "Cassandra", "Derrick", "Edmund", "Felicity", "Gawain", "Hannabel", "Ishmael", "Jesmond", "Nowell", "Samara",
    };
    static private List<string> HUMAN_LAST_NAMES = new List<string>() {
        "Yorke", "Whitgyft", "Unthank", "Tonstall", "Smith", "Ruddok", "Plumton", "Norfolk", "Abbot", "Bishop",
    };
    internal static string NameGenerator_Human(Random rng)
    {
        return $"{HUMAN_FIRST_NAMES.ElementAt(rng.Next(HUMAN_FIRST_NAMES.Count))} {HUMAN_LAST_NAMES.ElementAt(rng.Next(HUMAN_LAST_NAMES.Count))}";
    }
    static internal void DealComponent_Human(Deal deal)
    {
        deal.IncreaseDiscardReveal();
    }
    static private string GetText_Human(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "I better go while I still have some coin.";
            case Bark.LeavingRich: return "My thanks, everyone, the bar calls to me.";
            default:
                throw new Exception($"No elf text for bark={bark}");
        }
    }

    // -------------------------------- DWARF --------------------------------

    static private List<string> DWARF_FIRST_NAMES = new List<string>() {
        "Urist", "Ùshrir", "Sodel", "Limul", "Dumat", "Bofur", "Dori", "Ori", "Thorin", "Rhys", "Tagwen"
    };
    static private List<string> DWARF_LAST_NAMES = new List<string>() {
        "MacHammer", "McSmashy", "McTankard", "McStonefinger", "MacMountain", "McNostril", "MacStalewind", "McDoubleax"
    };
    internal static string NameGenerator_Dwarf(Random rng)
    {
        return $"{DWARF_FIRST_NAMES.ElementAt(rng.Next(DWARF_FIRST_NAMES.Count))} {DWARF_LAST_NAMES.ElementAt(rng.Next(DWARF_LAST_NAMES.Count))}";
    }
    static internal void DealComponent_Dwarf(Deal deal)
    {
        deal.IncreaseRiver();
    }
    static private string GetText_Dwarf(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "This mine is all tapped out.";
            case Bark.LeavingRich: return "Grand! Now I can buy a new ax!";
            default:
                throw new Exception($"No dwarf text for bark={bark}");
        }
    }

    // -------------------------------- ELF --------------------------------

    static private List<string> ELF_COMMON = new List<string>() {
        "E", "Ga", "Glo", "Fëa", "Eare", "Fi", "Ea", "A", "Lú", "Dri", "Elmi",
    };
    static private List<string> ELF_MIDDLE = new List<string>() {
        "lro", "la", "drie", "rfi", "nde", "no", "ndi", "ngo", "lfi", "rwe", "thei", "nste",
    };
    static private List<string> ELF_LAST = new List<string>() {
        "nd", "l", "r", "n", "st",
    };
    internal static string NameGenerator_Elf(Random rng)
    {
        return $"{ELF_COMMON.ElementAt(rng.Next(ELF_COMMON.Count))}{ELF_MIDDLE.ElementAt(rng.Next(ELF_MIDDLE.Count))}{ELF_LAST.ElementAt(rng.Next(ELF_LAST.Count))}";
    }
    static internal void DealComponent_Elf(Deal deal)
    {
        deal.AddRank(addToLowEnd: false);
    }
    static private string GetText_Elf(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "The leaves have fallen from this tree.";
            case Bark.LeavingRich: return "It seems this table is beneath me.";
            default:
                throw new Exception($"No elf text for bark={bark}");
        }
    }

    // -------------------------------- HALFLING --------------------------------

    static private List<string> HALFLING_FIRST_NAMES = new List<string>() {
        "Bunny", "Batty",
        "Cheery", "Chuckle", "Candy", "Curly",
        "Doc", "Dawn",
        "Flower",
        "Honest", "Happy", "Hope",
        "Musky",
        "Petal",
        "Smiley", "Spanky", "Sunny",
    };
    static private List<string> HALFLING_LAST_A_NAMES = new List<string>() {
        "Blush", "Bright",
        "Dank", "Dirty",
        "Funny", "Fuzzy",
        "Moist",
        "Oil", "Onion",
        "Pickle",
        "Rose",
        "Shine", "Squeak", "Stink", "Sweet",
        "Tart", "Tickle",
        "Whisper", "Wicked", "Wonder",
    };
    static private List<string> HALFLING_LAST_B_NAMES = new List<string>() {
        "barrel", "belly", "bottom", "button",
        "cheek",
        "feet", "finger", "foot", "farm",
        "hole",
        "leaf",
        "navel",
        "palm", "pants",
        "sausage", "smoke", "stockings",
        "tater", "thighs", "toe", "tooth", "tummy",
    };
    internal static string NameGenerator_Halfling(Random rng)
    {
        return $"{HALFLING_FIRST_NAMES.ElementAt(rng.Next(HALFLING_FIRST_NAMES.Count))} {HALFLING_LAST_A_NAMES.ElementAt(rng.Next(HALFLING_LAST_A_NAMES.Count))}{HALFLING_LAST_B_NAMES.ElementAt(rng.Next(HALFLING_LAST_B_NAMES.Count))}";
    }
    static internal void DealComponent_Halfling(Deal deal)
    {
        deal.AddPassToNeighbor();
    }
    static private string GetText_Halfling(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "I better go while I still own my pants.";
            case Bark.LeavingRich: return "I'll go before any of you get too mad.";
            default:
                throw new Exception($"No halfling text for bark={bark}");
        }
    }

    // -------------------------------- LIZARDMAN --------------------------------

    static private List<string> LIZARD_FRONT = new List<string>() {
        "G", "R", "L", "Zz", "Ch", "B", "V", "K", "Sl", "Asm", "N", "K",
    };
    static private List<string> LIZARD_MIDDLE = new List<string>() {
        "as", "ill", "is", "ip", "odz", "om", "od", "yth", "er", "ag", "aij",
    };
    static private List<string> LIZARD_END = new List<string>() {
        "a", "al", "ard", "u", "isk", "o", "in", "eus",
    };
    internal static string NameGenerator_Lizardman(Random rng)
    {
        int middleA = rng.Next() % LIZARD_MIDDLE.Count;
        int middleB = rng.Next() % (LIZARD_MIDDLE.Count - 1);
        if (middleB >= middleA)
        {
            middleB += 1;
        }
        return $"{LIZARD_FRONT.ElementAt(rng.Next(LIZARD_FRONT.Count))}{LIZARD_MIDDLE.ElementAt(middleA)}{LIZARD_MIDDLE.ElementAt(middleB)}{LIZARD_END.ElementAt(rng.Next(LIZARD_END.Count))}";
    }
    static internal void DealComponent_Lizardman(Deal deal)
    {
        deal.IncreaseObserveNeighborHighCard();
    }

    // -------------------------------- GOBLIN --------------------------------

    static private List<string> GOBLIN_START = new List<string>() {
        "B", "Cr", "Gr", "Sk", "Sn", "V",
    };
    static private List<string> GREENSKIN_MIDDLE_A = new List<string>()
    {
        "az", "ub", "ar", "ag", "aw", "ut", "og", "im", "ur", "or", "uz", "aug",
    };
    static private List<string> GREENSKIN_MIDDLE_B = new List<string>()
    {
        "dre", "a", "gha", "ro", "ja", "sni", "go", "zu", "glu", "zi", "la", "za", "re",
    };
    static private List<string> GOBLIN_END = new List<string>()
    {
        "sh", "k", "ttz", "t", "rb", "p",
    };
    internal static string NameGenerator_Goblin(Random rng)
    {
        string a = GOBLIN_START[rng.Next() % GOBLIN_START.Count];
        string b = GREENSKIN_MIDDLE_A[rng.Next() % GREENSKIN_MIDDLE_A.Count];
        string c = GREENSKIN_MIDDLE_B[rng.Next() % GREENSKIN_MIDDLE_B.Count];
        string d = GOBLIN_END[rng.Next() % GOBLIN_END.Count];
        return $"{a}{b}{c}{d}";
    }
    static internal void DealComponent_Goblin(Deal deal)
    {
        deal.RemoveRank(7, 6);
    }
    static internal bool CanAdd_Goblin(Deal deal)
    {
        return deal.MeetsMinCards(-2, 0);
    }
    static internal string GetText_Greenskin(Player player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return $"Stupid game. {player.Name} go now.";
            case Bark.LeavingRich: return $"{player.Name} has enough of your gold now.";
            default:
                throw new Exception($"No halfling text for bark={bark}");
        }
    }

    // -------------------------------- ORC --------------------------------

    static private List<string> ORC_START = new List<string>() {
        "N", "G", "Gh", "Kl", "T", "Arb", "M", "Az",
    };
    static private List<string> ORC_END = new List<string>() {
        "w", "d", "b", "g", "ruk", "m", "z", "rk", "r", "ng", "l",
    };
    internal static string NameGenerator_Orc(Random rng)
    {
        string a = ORC_START[rng.Next() % ORC_START.Count];
        string b = GREENSKIN_MIDDLE_A[rng.Next() % GREENSKIN_MIDDLE_A.Count];
        string c = GREENSKIN_MIDDLE_B[rng.Next() % GREENSKIN_MIDDLE_B.Count];
        string d = ORC_END[rng.Next() % ORC_END.Count];
        return $"{a}{b}{c}{d}";
    }
    static internal void DealComponent_Orc(Deal deal)
    {
        deal.RemoveRank(8, 9);
    }
    static internal bool CanAdd_Orc(Deal deal)
    {
        return deal.MeetsMinCards(-2, 0);
    }

    // -------------------------------- Troll --------------------------------
    static private List<string> TROLL_MID = new List<string>()
    {
        "a", "u", "o", "i", "uu", "oo", "au",
    };
    internal static string NameGenerator_Troll(Random rng)
    {
        string a = ORC_START[rng.Next() % ORC_START.Count];
        string b = TROLL_MID[rng.Next() % TROLL_MID.Count];
        string c = ORC_END[rng.Next() % ORC_END.Count];
        return $"{a}{b}{c}";
    }
    static internal void DealComponent_Troll(Deal deal)
    {
        deal.RemoveSuit(Suit.SuitColor.Black);
    }
    static internal bool CanAdd_Troll(Deal deal)
    {
        bool canAdd = deal.MeetsMinCards(0, -1);
        return canAdd;
    }

    // -------------------------------- Dragonkin --------------------------------
    static private List<string> DRAGONKIN_FRONT = new List<string>() {
        "Sm", "Dr", "Gh", "F", "T", "N", "Or",
    };
    static private List<string> DRAGONKIN_MIDDLE = new List<string>() {
        "au", "a", "ido", "afni", "iama", "alko", "idho", "o",
    };
    static private List<string> DRAGONKIN_END = new List<string>() {
        "g", "co", "ra", "r", "t", "kr", "gg", "chi",
    };
    static private List<string> DRAGONKIN_ADJ = new List<string>() {
        "Gold", "Silver", "Bronze", "Fierce", "Iron", "Wise", "Old", "Pale", "Vile", "Foul", "Doombringer",
    };
    internal static string NameGenerator_Dragonkin(Random rng)
    {
        int a = rng.Next() % DRAGONKIN_FRONT.Count;
        int b = rng.Next() % DRAGONKIN_MIDDLE.Count;
        int c = rng.Next() % DRAGONKIN_END.Count;
        int d = rng.Next() % DRAGONKIN_ADJ.Count;
        return $"{DRAGONKIN_FRONT[a]}{DRAGONKIN_MIDDLE[b]}{DRAGONKIN_END[c]} the {DRAGONKIN_ADJ[d]}";
    }
    static internal void DealComponent_Dragonkin(Deal deal)
    {
        deal.AddSuit();
    }
    static private string GetText_Dragonkin(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "I must fly.";
            case Bark.LeavingRich: return "I've enjoyed taking your coin.";
            default:
                throw new Exception($"No Dragonkin text for bark={bark}");
        }
    }

    // -------------------------------- Firbolg --------------------------------

    static private List<string> FIRBOLG_FRONT = new List<string>() {
        "T", "Galg", "M", "Orv", "Raey", "V",
    };
    static private List<string> FIRBOLG_MIDDLE = new List<string>() {
        "av", "ad", "ort", "orv", "adf", "and",
    };
    static private List<string> FIRBOLG_END = new List<string>() {
        "is", "ayle", "en", "al", "ourne", "er",
    };
    internal static string NameGenerator_Firbolg(Random rng)
    {
        int a = rng.Next() % FIRBOLG_FRONT.Count;
        int b = rng.Next() % FIRBOLG_MIDDLE.Count;
        int c = rng.Next() % FIRBOLG_END.Count;
        return $"{FIRBOLG_FRONT[a]}{FIRBOLG_MIDDLE[b]}{FIRBOLG_END[c]}";
    }
    static internal void DealComponent_Firbolg(Deal deal)
    {
        deal.AddRank(addToLowEnd: true);
    }
    static private string GetText_Firbolg(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "The sides of my coin purse touch.";
            case Bark.LeavingRich: return "May I leave my luck with you.";
            default:
                throw new Exception($"No Dragonkin text for bark={bark}");
        }
    }

    // -------------------------------- Birdman --------------------------------

    static private List<string> BIRDMAN_FRONT = new List<string>() {
        "Xh", "Kh", "Nk", "Qw", "X", "G'", "Khw", "'H", "Z", "Qw", "'X", "N'", "Kl",
    };
    static private List<string> BIRDMAN_MIDDLE = new List<string>() {
        "os", "ois", "ar", "ol", "an", "oan", "ul", "ant", "ath", "am", "om", "yl", "oph", "ix", "anth",
    };
    static private List<string> BIRDMAN_END = new List<string>() {
        "a", "an", "i", "ui", "e", "u", "ia",
    };
    internal static string NameGenerator_Birdman(Random rng)
    {
        int a = rng.Next() % BIRDMAN_FRONT.Count;
        int b = rng.Next() % BIRDMAN_MIDDLE.Count;
        int c = rng.Next() % BIRDMAN_END.Count;
        return $"{BIRDMAN_FRONT[a]}{BIRDMAN_MIDDLE[b]}{BIRDMAN_END[c]}";
    }
    static internal void DealComponent_Birdman(Deal deal)
    {
        deal.SetMinimumHandToWinPot(HandValue.HandRanking.TwoPairs, HandValue.HandRanking.Prison);
    }
    static private bool CanAdd_Birdman(Deal deal)
    {
        if (deal.MinimumHandToWinPot == HandValue.HandRanking.HighCard)
        {
            GD.Print($"Birdman allowed to join because min hand is HighCard");
            return true;
        }
        else
        {
            if (deal._suits.Count > 4)
            {
                GD.Print($"Birdman allowed because there are {deal._suits.Count} suits.");
                return true;
            }
            else
            {
                GD.Print($"Birdman denied because there are only {deal._suits.Count} suits.");
                return false;
            }
        }
    }
    static private string GetText_Birdman(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "If I stay any longer, I will never fly.";
            case Bark.LeavingRich: return "I hope I didn't ruffle any feathers.";
            default:
                throw new Exception($"No Birdman text for bark={bark}");
        }
    }

    // -------------------------------- Pixie --------------------------------

    static private List<string> PIXIE_FRONT = new List<string>() {
        "Tinker", "Plum", "Petal", "Star", "Moon", "Honey", "Sugar", "Chime",
    };
    static private List<string> PIXIE_END = new List<string>() {
        "bell", "blossom", "flower", "shine", "dust", "flicker", "beam",
    };
    internal static string NameGenerator_Pixie(Random rng)
    {
        int a = rng.Next() % PIXIE_FRONT.Count;
        int b = rng.Next() % PIXIE_END.Count;
        return $"{PIXIE_FRONT[a]}{PIXIE_END[b]}";
    }
    static internal void DealComponent_Pixie(Deal deal)
    {
        deal.SetPixieCompare();
    }
    static internal bool CanAdd_Pixie(Deal deal)
    {
        return deal.PixieCompare == false;
    }

    // -------------------------------- Centaur --------------------------------

    static private List<string> CENTAUR_FRONT = new List<string>() {
        "Swift", "Thunder", "Arrow", "Untrammeled", "Far", "Black", "Flame", "Ginger", "Wild", "Shadow", "Snow", "Sun", 
    };
    static private List<string> CENTAUR_END = new List<string>() {
        "hoof", "legs", "mane", "wind", "breeze", "cloud", "heart", "fire", 
    };
    private static string NameGenerator_Centaur(Random rng)
    {
        int a = rng.Next() % CENTAUR_FRONT.Count;
        int b = rng.Next() % CENTAUR_END.Count;
        return $"{CENTAUR_FRONT[a]}{CENTAUR_END[b]}";
    }
    private static void DealComponent_Centaur(Deal deal)
    {
        deal.IncreaseCostPerDiscard();
    }
    static private string GetText_Centaur(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "The high plains call to me.";
            case Bark.LeavingRich: return "As if any of you were real competition.";
            default:
                throw new Exception($"No Centaur text for bark={bark}");
        }
    }

    // -------------------------------- Giant --------------------------------

    static private List<string> GIANT_ADJECTIVE = new List<string>() {
        "Big", "Mega", "Colossal", "Titanic", "Large", "Huge", "Enormous", "Mammoth", "Considerable", "Quite Sizeable",
    };
    static private List<string> GIANT_NAME = new List<string>() {
        "Ed", "Ted", "Joe", "Sam", "Tod", "Abe", "Gus", "Ben", "Dan", "Moe", "Ox",
    };
    private static string NameGenerator_Giant(Random rng)
    {
        int a = rng.Next() % GIANT_ADJECTIVE.Count;
        int b = rng.Next() % GIANT_NAME.Count;
        return $"{GIANT_ADJECTIVE[a]} {GIANT_NAME[b]}";
    }
    private static void DealComponent_Giant(Deal deal)
    {
        deal.ShowHighestRankCards();
    }
    static private string GetText_Giant(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "Looks like I'm now short on cash...";
            case Bark.LeavingRich: return "I go now to play with taller gamblers.";
            default:
                throw new Exception($"No giant text for bark={bark}");
        }
    }
}
