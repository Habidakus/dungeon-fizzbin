using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

class Species
{
    private delegate string SpeciesNameGenerator(Random rnd);

    private int _introHand = 0;
    private double _weight = 1;
    private double Weight { get { return (Main.HandNumber < _introHand) ? 0 : _weight; } }
    public String Name { get; private set; }
    private SpeciesNameGenerator? _nameGenerator;

    private Species(String name, double weight, int introHand, SpeciesNameGenerator? sng = null)
    {
        Name = name;
        _weight = weight;
        _introHand = introHand;
        _nameGenerator = sng;
    }

    static private List<Species>? AllSpecies = null;
    private static void InitSpeciesList()
    {
        AllSpecies = new List<Species>() {
            new Species("Human", 1, 0, HumanNameGenerator),
            new Species("Dwarf", 1, 0, DwarfNameGenerator),
            new Species("Elf", 0.5, 0, ElfNameGenerator),
            new Species("Goblin", 0.5, 5),
            new Species("Lizardman", 0.5, 10, LizardmanNameGenerator),
            new Species("Dragonkin", 0.5, 10),
            new Species("Troll", 0.5, 10),
            new Species("Halfling", 1, 15, HalflingNameGenerator),
            new Species("Ghoul", 1, 15),
            new Species("Dogman", 1, 15),
            new Species("Centaur", 1, 15),
            new Species("Pixie", 1, 15),
            new Species("Elf", 1, 15),
            new Species("Orc", 1, 15),
            new Species("Giant", 1, 15),
            new Species("Birdman", 1, 15),
            new Species("Firbolg", 1, 15),
            new Species("Golem", 1, 30),
            new Species("Lich", 0.25, 30),
            new Species("Vampire", 0.5, 30),
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

    static public Species PickSpecies(Random rnd)
    {
        if (AllSpecies == null)
        {
            InitSpeciesList();
        }

        double index = rnd.NextDouble() * AllSpecies!.Sum(a => a.Weight);
        foreach(Species species in AllSpecies!)
        {
            index -= species.Weight;
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

    static private List<string> HUMAN_FIRST_NAMES = new List<string>() {
        "Agatha", "Bartholomew", "Cassandra", "Derrick", "Edmund", "Felicity", "Gawain", "Hannabel", "Ishmael", "Jesmond", "Nowell", "Samara",
    };
    static private List<string> HUMAN_LAST_NAMES = new List<string>() {
        "Yorke", "Whitgyft", "Unthank", "Tonstall", "Smith", "Ruddok", "Plumton", "Norfolk", "Abbot", "Bishop",
    };
    internal static string HumanNameGenerator(Random rng)
    {
        return $"{HUMAN_FIRST_NAMES.ElementAt(rng.Next(HUMAN_FIRST_NAMES.Count))} {HUMAN_LAST_NAMES.ElementAt(rng.Next(HUMAN_LAST_NAMES.Count))}";
    }

    static private List<string> DWARF_FIRST_NAMES = new List<string>() {
        "Urist", "Ùshrir", "Sodel", "Limul", "Dumat", "Bofur", "Dori", "Ori", "Thorin", "Rhys", "Tagwen"
    };
    static private List<string> DWARF_LAST_NAMES = new List<string>() {
        "MacHammer", "McSmashy", "McTankard", "McStonefinger", "MacMountain", "McNostril", "MacStalewind", "McDoubleax"
    };
    internal static string DwarfNameGenerator(Random rng)
    {
        return $"{DWARF_FIRST_NAMES.ElementAt(rng.Next(DWARF_FIRST_NAMES.Count))} {DWARF_LAST_NAMES.ElementAt(rng.Next(DWARF_LAST_NAMES.Count))}";
    }

    static private List<string> ELF_COMMON = new List<string>() {
        "E", "Ga", "Glo", "Fëa", "Eare", "Fi", "Ea", "A", "Lú", "Dri", "Elmi",
    };
    static private List<string> ELF_MIDDLE = new List<string>() {
        "lro", "la", "drie", "rfi", "nde", "no", "ndi", "ngo", "lfi", "rwe", "thei", "nste",
    };
    static private List<string> ELF_LAST = new List<string>() {
        "nd", "l", "r", "n", "st",
    };
    internal static string ElfNameGenerator(Random rng)
    {
        return $"{ELF_COMMON.ElementAt(rng.Next(ELF_COMMON.Count))}{ELF_MIDDLE.ElementAt(rng.Next(ELF_MIDDLE.Count))}{ELF_LAST.ElementAt(rng.Next(ELF_LAST.Count))}";
    }

    static private List<string> HALFLING_FIRST_NAMES = new List<string>() {
        "Cheery", "Petal", "Smiley", "Doc", "Honest", "Chuckle", "Bunny", "Flower", "Glow", "Dawn", "Musky", "Hope", "Spanky", "Curly", "Batty", "Sunny", 
    };
    static private List<string> HALFLING_LAST_A_NAMES = new List<string>() {
        "Pickle", "Tickle", "Squeak", "Whisper", "Dirty", "Oil", "Rose", "Wicked", "Fuzzy", "Blush", "Bright", "Moist", "Dank", "Onion", "Stink", "Sweet", "Tart", "Funny",
    };
    static private List<string> HALFLING_LAST_B_NAMES = new List<string>() {
        "Bottom", "Foot", "Feet", "Navel", "Belly", "Barrel", "Hole", "Toe", "Finger", "Button", "Palm", "Tooth", "Smoke", "Thighs", "Leaf", "Pants", "Stockings", "Tummy",
    };
    internal static string HalflingNameGenerator(Random rng)
    {
        return $"{HALFLING_FIRST_NAMES.ElementAt(rng.Next(HALFLING_FIRST_NAMES.Count))} {HALFLING_LAST_A_NAMES.ElementAt(rng.Next(HALFLING_LAST_A_NAMES.Count))}{HALFLING_LAST_B_NAMES.ElementAt(rng.Next(HALFLING_LAST_B_NAMES.Count))}";
    }

    static private List<string> LIZARD_FRONT = new List<string>() {
        "G", "R", "L", "Zz", "Ch", "B", "V", "K", "Sl", "Asm", "N", "K",
    };
    static private List<string> LIZARD_MIDDLE = new List<string>() {
        "as", "ill", "is", "ip", "odz", "om", "od", "yth", "er", "ag", "aij",
    };
    static private List<string> LIZARD_END = new List<string>() {
        "a", "al", "ard", "u", "isk", "o", "in", "eus",
    };
    internal static string LizardmanNameGenerator(Random rng)
    {
        int middleA = rng.Next() % LIZARD_MIDDLE.Count;
        int middleB = rng.Next() % (LIZARD_MIDDLE.Count - 1);
        if (middleB >= middleA)
        {
            middleB += 1;
        }
        return $"{LIZARD_FRONT.ElementAt(rng.Next(LIZARD_FRONT.Count))}{LIZARD_MIDDLE.ElementAt(middleA)}{LIZARD_MIDDLE.ElementAt(middleB)}{LIZARD_END.ElementAt(rng.Next(LIZARD_END.Count))}";
    }
}
