using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

class Species
{
    private delegate void DealComponent(Deal deal);
    private delegate string SpeciesNameGenerator(Random rnd);

    private int _introHand = 0;
    private double _weight = 1;
    private double Weight { get { return (Main.HandNumber < _introHand) ? 0 : _weight; } }
    public String Name { get; private set; }
    private SpeciesNameGenerator? _nameGenerator;
    private readonly DealComponent _dealComponent;

    private Species(String name, double weight, int introHand, DealComponent dealComponent, SpeciesNameGenerator? sng = null)
    {
        Name = name;
        _weight = weight;
        _introHand = introHand;
        _nameGenerator = sng;
        _dealComponent = dealComponent;
    }

    static private List<Species>? AllSpecies = null;
    private static void InitSpeciesList()
    {
        AllSpecies = new List<Species>() {
            new Species("Human", 1, 0, DealComponent_Human, NameGenerator_Human),
            //new Species("Dwarf", 1, 0, DealComponent_Dwarf, NameGenerator_Dwarf),
            new Species("Elf", 0.5, 0, DealComponent_Elf, NameGenerator_Elf),
            new Species("Goblin", 0.5, 5, DealComponent_Goblin, NameGenerator_Goblin),
            //new Species("Lizardman", 0.5, 10, DealComponent_Lizardman, NameGenerator_Lizardman),
            new Species("Dragonkin", 0.5, 10, DealComponent_Dragonkin),
            new Species("Troll", 0.5, 10, DealComponent_Troll),
            //new Species("Halfling", 1, 15, DealComponent_Halfling, NameGenerator_Halfling),
            //new Species("Ghoul", 1, 15),
            //new Species("Dogman", 1, 15),
            //new Species("Centaur", 1, 15),
            //new Species("Pixie", 1, 15),
            //new Species("Elf", 1, 15),
            new Species("Orc", 1, 15, DealComponent_Orc, NameGenerator_Orc),
            //new Species("Giant", 1, 15),
            //new Species("Birdman", 1, 15),
            new Species("Firbolg", 1, 30, DealComponent_Firbolg),
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

    internal void ApplyDealComponent(Deal deal)
    {
        _dealComponent(deal);
    }

    private double CalculateSelectionWeight(List<Species> speciesAlreadyAtTable)
    {
        int count = speciesAlreadyAtTable.Where(a => a == this).Count();
        if (count > 1)
            return 0;
        else if (count == 1)
            return Weight / 2.0;
        else
            return Weight;
    }

    static public Species PickSpecies(Random rnd, List<Species> speciesAlreadyAtTable)
    {
        if (AllSpecies == null)
        {
            InitSpeciesList();
        }

        double index = rnd.NextDouble() * AllSpecies!.Sum(a => a.CalculateSelectionWeight(speciesAlreadyAtTable));
        foreach(Species species in AllSpecies!)
        {
            index -= species.CalculateSelectionWeight(speciesAlreadyAtTable);
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

    // -------------------------------- HALFLING --------------------------------

    static private List<string> HALFLING_FIRST_NAMES = new List<string>() {
        "Cheery", "Petal", "Smiley", "Doc", "Honest", "Chuckle", "Bunny", "Flower", "Glow", "Dawn", "Musky", "Hope", "Spanky", "Curly", "Batty", "Sunny", 
    };
    static private List<string> HALFLING_LAST_A_NAMES = new List<string>() {
        "Pickle", "Tickle", "Squeak", "Whisper", "Dirty", "Oil", "Rose", "Wicked", "Fuzzy", "Blush", "Bright", "Moist", "Dank", "Onion", "Stink", "Sweet", "Tart", "Funny",
    };
    static private List<string> HALFLING_LAST_B_NAMES = new List<string>() {
        "Bottom", "Foot", "Feet", "Navel", "Belly", "Barrel", "Hole", "Toe", "Finger", "Button", "Palm", "Tooth", "Smoke", "Thighs", "Leaf", "Pants", "Stockings", "Tummy",
    };
    internal static string NameGenerator_Halfling(Random rng)
    {
        return $"{HALFLING_FIRST_NAMES.ElementAt(rng.Next(HALFLING_FIRST_NAMES.Count))} {HALFLING_LAST_A_NAMES.ElementAt(rng.Next(HALFLING_LAST_A_NAMES.Count))}{HALFLING_LAST_B_NAMES.ElementAt(rng.Next(HALFLING_LAST_B_NAMES.Count))}";
    }
    static internal void DealComponent_Halfling(Deal deal)
    {
        deal.AddPassToNeighbor();
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
        deal.RemoveRank(8, 7);
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
        deal.RemoveRank(2, 3);
    }

    // -------------------------------- Dragonkin --------------------------------

    static internal void DealComponent_Dragonkin(Deal deal)
    {
        deal.AddSuit();
    }

    // -------------------------------- Troll --------------------------------

    static internal void DealComponent_Troll(Deal deal)
    {
        deal.RemoveSuit(Suit.SuitColor.Black);
    }

    // -------------------------------- Firbolg --------------------------------

    static internal void DealComponent_Firbolg(Deal deal)
    {
        deal.AddRank(addToLowEnd: true);
    }
}
