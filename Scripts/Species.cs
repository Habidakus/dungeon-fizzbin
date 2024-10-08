﻿using Godot;
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
    public String RuleBBCode { get; private set; }
    private SpeciesNameGenerator? _nameGenerator;
    private readonly DealComponent _dealComponent;
    private readonly SpeciesAllowed? _allowed;
    private SpeciesText? _speciesText;

    private Species(String name, double weight, int introHand, String ruleBBCode, DealComponent dealComponent, SpeciesNameGenerator? sng = null, SpeciesAllowed? allowed = null, SpeciesText? speciesText = null)
    {
        Name = name;
        RuleBBCode = ruleBBCode;
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
            //new Species("BLANK1", 1, 0, "TBD", DealComponent_DoNothing, null, null, null),
            //new Species("BLANK2", 1, 0, "TBD", DealComponent_DoNothing, null, null, null),
            new Species("Human", 0.5, 0,
                "First card(s) discarded by each player are revealed to the entire table.",
                DealComponent_Human, NameGenerator_Human, null, GetText_Human),
            new Species("Elf", 1, 0,
                $"Add the {Rank.Empress._unicode} ({Rank.Empress.Tooltip}) and {Rank.Jupiter._unicode} ({Rank.Jupiter.Tooltip}) ranks to the deck, which are higher than a {Rank.DefaultRanks[11]._unicode} ({Rank.DefaultRanks[11].Tooltip})",
                DealComponent_Elf, NameGenerator_Elf, null, GetText_Elf),
            new Species("Goblin", 0.75, 0,
                $"Removes the {Rank.DefaultRanks[5]._unicode} ({Rank.DefaultRanks[5].Tooltip}) and {Rank.DefaultRanks[4]._unicode} ({Rank.DefaultRanks[4].Tooltip}) ranks from the deck.",
                DealComponent_Goblin, NameGenerator_Goblin, CanAdd_Goblin, GetText_Greenskin),
            new Species("Dwarf", 0.25, 5,
                "For each Dwarf add two cards to the river, and deal one less card to each player.",
                DealComponent_Dwarf, NameGenerator_Dwarf, null, GetText_Dwarf),
            new Species("Dragonkin", 1, 5,
                $"Add the {Suit.Skull._unicode} ({Suit.Skull.Tooltip}) and {Suit.Swords._unicode} ({Suit.Swords.Tooltip}) suits to the deck; this allows the Prison hand.",
                DealComponent_Dragonkin, NameGenerator_Dragonkin, null, GetText_Dragonkin),
            new Species("Troll", 1, 10,
                $"Remove the {Suit.DefaultSuits[0]._unicode} ({Suit.DefaultSuits[0].Tooltip}) and {Suit.DefaultSuits[3]._unicode} ({Suit.DefaultSuits[3].Tooltip}) suits from the deck.",
                DealComponent_Troll, NameGenerator_Troll, CanAdd_Troll, GetText_Greenskin),
            new Species("Lizardman", 1, 10,
                "See the highest ranked card(s) in your right neighbor's hand.",
                DealComponent_Lizardman, NameGenerator_Lizardman),
            new Species("Kobold", 0.75, 10,
                "See the lowest ranked card(s) in your left neighbor's hand.",
                DealComponent_Kobold, NameGenerator_Kobold, null, GetText_Kobold),
            new Species("Dogman", 0.75, 13,
                "Add an extra round of discarding cards.",
                DealComponent_Dogman, NameGenerator_Dogman, CanAdd_Dogman, GetText_Dogman),
            new Species("Catperson", 0.33, 13,
                "There is no discard round.",
                DealComponent_Catperson, NameGenerator_Catperson, CanAdd_Catperson, GetText_Catperson),
            new Species("Orc", 0.75, 15,
                $"Removes the {Rank.DefaultRanks[6]._unicode} ({Rank.DefaultRanks[6].Tooltip}) and {Rank.DefaultRanks[7]._unicode} ({Rank.DefaultRanks[7].Tooltip}) ranks from the deck.",
                DealComponent_Orc, NameGenerator_Orc, CanAdd_Orc, GetText_Greenskin),
            new Species("Halfling", 1, 15, 
                "Before the discard phase, pass card(s) to your left neighbor - this card will continue to be revealed to you.",
                DealComponent_Halfling, NameGenerator_Halfling, null, GetText_Halfling),
            new Species("Centaur", 0.5, 15,
                "You must pay more to the ante for each card you discard.",
                DealComponent_Centaur, NameGenerator_Centaur, CanAdd_Centaur, GetText_Centaur),
            new Species("Pixie", 1, 15,
                "The ranking of cards are inverted; thus a two of hearts is more important than a three of hearts, although hands still rank in difficulty.",
                DealComponent_Pixie, NameGenerator_Pixie, CanAdd_Pixie),
            new Species("Giant", 1, 15,
                "The highest three cards dealt are visible to all players.",
                DealComponent_Giant, NameGenerator_Giant, null, GetText_Giant),
            //new Species("Ghoul", 1, 15),
            new Species("Werewolf", 1, 15,
                $"{Card.Joker} ({Card.Joker.Tooltip}) card(s) are added to the deck, which will duplicate any card in your hand or the river.",
                DealComponent_Werewolf, NameGenerator_Werewolf, null, GetText_Werewolf),
            new Species("Birdman", 0.5, 20,
                "Any hand that finishes with merely a pair (or prison with two Birdmen) is scrubbed and the pot is added to the next hand.",
                 DealComponent_Birdman, NameGenerator_Birdman, CanAdd_Birdman, GetText_Birdman),
            new Species("Firbolg", 1, 30,
                $"Add the {Rank.Lead._unicode} ({Rank.Lead.Tooltip}) and {Rank.Anhk._unicode} ({Rank.Anhk.Tooltip}) ranks to the deck, which are lower than a {Rank.DefaultRanks[0]._unicode} ({Rank.DefaultRanks[0].Tooltip})",
                DealComponent_Firbolg, NameGenerator_Firbolg, null, GetText_Firbolg),
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

    internal static IEnumerable<Tuple<Species,float>> GetUnlockedSpeciesPlayableAndFraction(AchievementManager achievementManager)
    {
        if (AllSpecies == null)
        {
            InitSpeciesList();
        }

        foreach (Species species in AllSpecies!)
        {
            float unlockedFraction = achievementManager.GetUnlockedFraction(species, s_minUnlockPlayable);
            yield return Tuple.Create(species, unlockedFraction);
        }

        yield break;
    }

    static readonly float s_minUnlockPlayable = 3.33f;
    internal static IEnumerable<Species> GetUnlockedSpeciesPlayable(AchievementManager achievementManager)
    {
        if (AllSpecies == null)
        {
            InitSpeciesList();
        }

        foreach (Species species in AllSpecies!)
        {
            float unlockedFraction = achievementManager.GetUnlockedFraction(species, s_minUnlockPlayable);
            if (unlockedFraction >= 1)
            {
                yield return species;
            }
        }

        yield break;
    }

    static readonly float s_minUnlockRules = 5f;
    internal static IEnumerable<Species> GetUnlockedSpeciesRules(AchievementManager achievementManager)
    {
        if (AllSpecies == null)
        {
            InitSpeciesList();
        }

        foreach (Species species in AllSpecies!)
        {
            float unlockedFraction = achievementManager.GetUnlockedFraction(species, s_minUnlockRules);
            if (unlockedFraction >= 1)
            {
                yield return species;
            }
        }

        yield break;
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
        foreach (Species species in AllSpecies!)
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

    // If we just ask for random elements of the name to be generated with a random roll, we will from time to time get
    // the same sub-elements appearing at the same table. This looks clumsy. If we instead do a prime sampling of the
    // arrays we cover the entire base, and if we use different primes for each sub-index they're still appear pretty
    // random in order of appearance.
    private static Dictionary<string, Tuple<uint, uint>> s_speciesNameSteppers = new Dictionary<string, Tuple<uint, uint>>();
    private static string PickFromArray(string speciesName, int index, string[] array, Random rnd)
    {
        uint nextSelection = 0;
        uint primeStepper = 101;
        string lookupHash = $"{speciesName}{index}";
        if (s_speciesNameSteppers.TryGetValue(lookupHash, out Tuple<uint, uint>? stepper))
        {
            primeStepper = stepper.Item2;
            nextSelection = (stepper.Item1 + primeStepper) % (uint) array.Length;
        }
        else
        {
            // First time used, initialize
            uint[] primes = new uint[] {
                1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063,
                1069, 1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151
            };
            primeStepper = primes[rnd.Next(primes.Length)];
            nextSelection = ((uint)rnd.Next(5000) + primeStepper) % (uint)array.Length;
        }

        string retVal = array[nextSelection];
        s_speciesNameSteppers[lookupHash] = Tuple.Create(nextSelection, primeStepper);

        return retVal;
    }
    static internal void DealComponent_DoNothing(Deal deal)
    {
    }

    internal static StaticSpeciesSaveElement GenerateStaticSpeciesSaveElement()
    {
        return new StaticSpeciesSaveElement(s_speciesNameSteppers);
    }

    internal static void SetStaticSpeciesData(StaticSpeciesSaveElement staticSpeciesEl)
    {
        s_speciesNameSteppers = staticSpeciesEl.SpeciesNameSteppers;
    }

    // -------------------------------- HUMAN --------------------------------

    static private List<string> HUMAN_FIRST_NAMES = new List<string>() {
        "Agatha", "Bartholomew", "Cassandra", "Derrick", "Edmund", "Felicity", "Gawain", "Hannabel", "Ishmael", "Jesmond", "Nowell", "Samara", "Victor",
    };
    static private List<string> HUMAN_LAST_NAMES = new List<string>() {
        "Yorke", "Whitgyft", "Unthank", "Tonstall", "Smith", "Ruddok", "Plumton", "Norfolk", "Abbot", "Bishop", "Grimm",
    };
    internal static string NameGenerator_Human(Random rng)
    {
        string firstName = PickFromArray("Human", 0, HUMAN_FIRST_NAMES.ToArray(), rng);
        string secondName = PickFromArray("Human", 1, HUMAN_LAST_NAMES.ToArray(), rng);
        return $"{firstName} {secondName}";
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
                throw new Exception($"No human text for bark={bark}");
        }
    }

    // -------------------------------- DWARF --------------------------------

    static private List<string> DWARF_FIRST_NAMES = new List<string>() {
        "Urist", "Ùshrir", "Sodel", "Limul", "Dumat", "Bofur", "Dori", "Ori", "Thorin", "Rhys", "Tagwen", "Hjodill"
    };
    static private List<string> DWARF_LAST_NAMES_PREFIX = new List<string>() {
        "Mac", "Mc", "O'",
    };
    static private List<string> DWARF_LAST_NAMES_SUFFIX = new List<string>() {
        "Blackanvil",
        "Deepshaft", "Doubleax",
        "Goldbones",
        "Hammer",
        "Leadshoe",
        "Mountain", "Motherlode",
        "Nostril",
        "Rocknoggin",
        "Smashy", "Stonefinger", "Stalewind", "Shaletooth",
        "Tankard", "Thickale", "Tinmonger",
    };
    internal static string NameGenerator_Dwarf(Random rng)
    {
        string firstName = PickFromArray("Dwarf", 0, DWARF_FIRST_NAMES.ToArray(), rng);
        string prefixName = PickFromArray("Dwarf", 1, DWARF_LAST_NAMES_PREFIX.ToArray(), rng);
        string suffixName = PickFromArray("Dwarf", 2, DWARF_LAST_NAMES_SUFFIX.ToArray(), rng);
        return $"{firstName} {prefixName}{suffixName}";
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

    static private List<string> ELF_START = new List<string>() {
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
        string startName = PickFromArray("Elf", 0, ELF_START.ToArray(), rng);
        string midName = PickFromArray("Elf", 1, ELF_MIDDLE.ToArray(), rng);
        string endName = PickFromArray("Elf", 2, ELF_LAST.ToArray(), rng);
        return $"{startName}{midName}{endName}";
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
        string startName = PickFromArray("Halfling", 0, HALFLING_FIRST_NAMES.ToArray(), rng);
        string lastAName = PickFromArray("Halfling", 1, HALFLING_LAST_A_NAMES.ToArray(), rng);
        string lastBName = PickFromArray("Halfling", 2, HALFLING_LAST_B_NAMES.ToArray(), rng);
        return $"{startName} {lastAName}{lastBName}";
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
        string startName = PickFromArray("Lizard", 0, LIZARD_FRONT.ToArray(), rng);
        string midAName = PickFromArray("Lizard", 1, LIZARD_MIDDLE.ToArray(), rng);
        string midBName = PickFromArray("Lizard", 1, LIZARD_MIDDLE.ToArray(), rng);
        string endName = PickFromArray("Lizard", 2, LIZARD_END.ToArray(), rng);
        return $"{startName}{midAName}{midBName}{endName}";
    }
    static internal void DealComponent_Lizardman(Deal deal)
    {
        deal.IncreaseObserveNeighborHighCard();
    }

    // -------------------------------- KOBOLD --------------------------------

    static private List<string> KOBOLD_FRONT = new List<string>() {
        "G", "R", "L", "Zz", "Ch", "B", "V", "K", "Sl", "Asm", "N", "K",
    };
    static private List<string> KOBOLD_END = new List<string>() {
        "a", "ack", "al", "as", "ard",
        "in", "ip", "isk", "ist", "ith",
        "odz", "om",
        "eus",
        "u",
    };
    internal static string NameGenerator_Kobold(Random rng)
    {
        string startName = PickFromArray("Kobold", 0, KOBOLD_FRONT.ToArray(), rng);
        string endName = PickFromArray("Kobold", 1, KOBOLD_END.ToArray(), rng);
        return $"{startName}{endName}";
    }
    static internal void DealComponent_Kobold(Deal deal)
    {
        deal.IncreaseObserveNeighborLowCard();
    }
    static private string GetText_Kobold(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "Bah, none of this money is shiny!";
            case Bark.LeavingRich: return "So much shiny! I start my lair now!";
            default:
                throw new Exception($"No kobold text for bark={bark}");
        }
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
        string a = PickFromArray("Goblin", 0, GOBLIN_START.ToArray(), rng);
        string b = PickFromArray("Greenskin", 0, GREENSKIN_MIDDLE_A.ToArray(), rng);
        string c = PickFromArray("Greenskin", 1, GREENSKIN_MIDDLE_B.ToArray(), rng);
        string d = PickFromArray("Goblin", 1, GOBLIN_END.ToArray(), rng);
        return $"{a}{b}{c}{d}";
    }
    static internal void DealComponent_Goblin(Deal deal)
    {
        deal.RemoveRank(7, 6);
    }
    static internal bool CanAdd_Goblin(Deal deal)
    {
        return deal.MeetsMinCards(-2, 0, 0, 0);
    }
    static internal string GetText_Greenskin(Player player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return $"Stupid game. {player.Name} go now.";
            case Bark.LeavingRich: return $"{player.Name} has enough gold, leave now.";
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
        string a = PickFromArray("Orc", 0, ORC_START.ToArray(), rng);
        string b = PickFromArray("Greenskin", 0, GREENSKIN_MIDDLE_A.ToArray(), rng);
        string c = PickFromArray("Greenskin", 1, GREENSKIN_MIDDLE_B.ToArray(), rng);
        string d = PickFromArray("Orc", 1, ORC_END.ToArray(), rng);
        return $"{a}{b}{c}{d}";
    }
    static internal void DealComponent_Orc(Deal deal)
    {
        deal.RemoveRank(8, 9);
    }
    static internal bool CanAdd_Orc(Deal deal)
    {
        return deal.MeetsMinCards(-2, 0, 0, 0);
    }

    // -------------------------------- Troll --------------------------------
    static private List<string> TROLL_MID = new List<string>()
    {
        "a", "u", "o", "i", "uu", "oo", "au",
    };
    internal static string NameGenerator_Troll(Random rng)
    {
        string a = PickFromArray("Orc", 0, ORC_START.ToArray(), rng);
        string b = PickFromArray("Troll", 0, TROLL_MID.ToArray(), rng);
        string c = PickFromArray("Orc", 1, ORC_END.ToArray(), rng);
        return $"{a}{b}{c}";
    }
    static internal void DealComponent_Troll(Deal deal)
    {
        deal.RemoveSuit(Suit.SuitColor.Black);
    }
    static internal bool CanAdd_Troll(Deal deal)
    {
        bool canAdd = deal.MeetsMinCards(0, -1, 0, 0);
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
        "Gold", "Silver", "Bronze", "Fierce", "Iron", "Wise", "Old", "Pale", "Vile", "Foul", "Doombane",
    };
    internal static string NameGenerator_Dragonkin(Random rng)
    {
        string a = PickFromArray("Dragonkin", 0, DRAGONKIN_FRONT.ToArray(), rng);
        string b = PickFromArray("Dragonkin", 1, DRAGONKIN_MIDDLE.ToArray(), rng);
        string c = PickFromArray("Dragonkin", 2, DRAGONKIN_END.ToArray(), rng);
        string d = PickFromArray("Dragonkin", 3, DRAGONKIN_ADJ.ToArray(), rng);
        return $"{a}{b}{c} the {d}";
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
        string a = PickFromArray("Firbolg", 0, FIRBOLG_FRONT.ToArray(), rng);
        string b = PickFromArray("Firbolg", 1, FIRBOLG_MIDDLE.ToArray(), rng);
        string c = PickFromArray("Firbolg", 2, FIRBOLG_END.ToArray(), rng);
        return $"{a}{b}{c}";
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
        string a = PickFromArray("Birdman", 0, BIRDMAN_FRONT.ToArray(), rng);
        string b = PickFromArray("Birdman", 1, BIRDMAN_MIDDLE.ToArray(), rng);
        string c = PickFromArray("Birdman", 2, BIRDMAN_END.ToArray(), rng);
        return $"{a}{b}{c}";
    }
    static internal void DealComponent_Birdman(Deal deal)
    {
        deal.SetMinimumHandToWinPot(HandValue.HandRanking.TwoPairs, HandValue.HandRanking.Prison);
    }
    static private bool CanAdd_Birdman(Deal deal)
    {
        if (deal.MinimumHandToWinPot == HandValue.HandRanking.HighCard)
        {
            return true;
        }
        else
        {
            return (deal._suits.Count > 4);
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
        string a = PickFromArray("Pixie", 0, PIXIE_FRONT.ToArray(), rng);
        string b = PickFromArray("Pixie", 1, PIXIE_END.ToArray(), rng);
        return $"{a}{b}";
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
        "Swift", "Thunder", "Arrow", "Untrammeled", "Far", "Black", "Flame", "Ginger", "Wild", "Shadow", "Snow", "Sun", "Summer", "Winter",
    };
    static private List<string> CENTAUR_END = new List<string>() {
        "hoof", "legs", "mane", "wind", "breeze", "cloud", "heart", "fire", "bow", 
    };
    private static string NameGenerator_Centaur(Random rng)
    {
        string a = PickFromArray("Centaur", 0, CENTAUR_FRONT.ToArray(), rng);
        string b = PickFromArray("Centaur", 1, CENTAUR_END.ToArray(), rng);
        return $"{a}{b}";
    }
    private static void DealComponent_Centaur(Deal deal)
    {
        deal.IncreaseCostPerDiscard();
    }
    static private bool CanAdd_Centaur(Deal deal)
    {
        return deal.DiscardRoundsRemaining > 0;
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
        string a = PickFromArray("Giant", 0, GIANT_ADJECTIVE.ToArray(), rng);
        string b = PickFromArray("Giant", 1, GIANT_NAME.ToArray(), rng);
        return $"{a} {b}";
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

    // -------------------------------- Werewolf --------------------------------

    static private List<string> WEREWOLF_NAME_PREFIX = new List<string>() {
        "Ael", "Ag", "Agrio", "Al", "Asbo",
        "Cana", "Cyp",
        "Dor", "Dro",
        "Harpa", "Har", "Hylac", "Hy",
        "Ichnoba",
        "Lab", "Lach", "La", "Lea", "Leu", "Lycis",
        "Malam", "Mela",
        "Na", "Nebropho",
        "Oriba",
        "Pampha", "Ptere",
        "Stric",
        "Tig",
    };
    static private List<string> WEREWOLF_NAME_SUFFIX = new List<string>() {
        "ce", "ceus", "con",
        "don", "dos",
        "gus",
        "laeus", "laps", "las", "lo", "los", "lus",
        "max",
        "ne", "neus", "nus",
        "pe", "pus", "pyia",
        "re", "ris", "rius", "ron", "ros",
        "sus",
        "te", "tes", "tor",
    };
    private static string NameGenerator_Werewolf(Random rng)
    {
        string prefix = PickFromArray("Werewolf", 0, WEREWOLF_NAME_PREFIX.ToArray(), rng);
        string suffix = PickFromArray("Werewolf", 1, WEREWOLF_NAME_SUFFIX.ToArray(), rng);
        return $"{prefix}{suffix}";
    }
    private static void DealComponent_Werewolf(Deal deal)
    {
        deal.AddDoppelganger();
    }
    static private string GetText_Werewolf(Player _player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "Dang, this has taken a bite out of my wallet.";
            case Bark.LeavingRich: return "It's been nice, but the moon calls to me.";
            default:
                throw new Exception($"No werewolf text for bark={bark}");
        }
    }

    // -------------------------------- Dogman --------------------------------

    static private List<string> DOGMAN_NAME = new List<string>() {
        "Buddy", "Bear", "Max", "Rex", "Fluffy", "Lassie", "Lucky", "Rover", "Spot", "Fido", "Duke", "Checkers",
    };
    private static string NameGenerator_Dogman(Random rng)
    {
        // TODO: Make dogman and werewolf names separate
        return PickFromArray("Dogman", 0, DOGMAN_NAME.ToArray(), rng);
    }
    private static void DealComponent_Dogman(Deal deal)
    {
        deal.AddDiscardRound();
    }
    private static bool CanAdd_Dogman(Deal deal)
    {
        int newMaxDiscards = Math.Max(1, deal.MaxDiscard - 1);
        bool enoughCards = deal.MeetsMinCards(0, 0, newMaxDiscards - deal.MaxDiscard, 1);
        bool enoughSpaceToDisplayAllDiscards = (newMaxDiscards * (deal.DiscardRoundsRemaining + 1)) < 5;
        bool weHaventSetDicardRoundsToZero = deal.DiscardRoundsRemaining > 0;
        return weHaventSetDicardRoundsToZero && enoughSpaceToDisplayAllDiscards && enoughCards;
    }
    private static string GetText_Dogman(Player player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "I'll see myself out.";
            case Bark.LeavingRich: return "This was so much fun! Lets play again soon!";
            default:
                throw new Exception($"No dogman text for bark={bark}");
        }
    }

    // -------------------------------- Catperson --------------------------------

    static private List<string> CAT_NAME_PREFIX = new List<string>() {
        "Admiral",
        "Baby",
        "Captain",
        "Lord",
        "Major",
        "Mister",
        "Professor",
        "Queen",
        "Sir",
    };
    static private List<string> CAT_NAME_BASE = new List<string>()
    {
        "Boo",
        "Chonks",
        "Edgar",
        "Fluffles",
        "Kinako",
        "Maite",
        "Mingus",
        "Mittens",
        "Sushi",
        "Turbo",
        "Tumtum",
    };
    static private List<string> CAT_NAME_SUFFIX = new List<string>()
    {
        "Clawmonger",
        "Purrmachine",
        "the Crepuscular",
        "the Curious",
        "the Inquisitive",
        "the Lightning",
        "the Patient",
        "the Regal",
        "the Scribbler",
        "the Sniffer",
        "the Stampede",
        "the Thief",
        "Von Meow",
        "Von Clausewitz",
        "Wigglebutt",
    };

    private static string NameGenerator_Catperson(Random rng)
    {
        string branchType = PickFromArray("Catperson", 0, new string[] { "a", "b", "a", "b", "a", "b" }, rng);
        string name = PickFromArray("Catperson", 1, CAT_NAME_BASE.ToArray(), rng);
        if (branchType == "a")
        {
            string prefix = PickFromArray("Catperson", 2, CAT_NAME_PREFIX.ToArray(), rng);
            return $"{prefix} {name}";
        }
        else
        {
            string suffix = PickFromArray("Catperson", 3, CAT_NAME_SUFFIX.ToArray(), rng);
            return $"{name} {suffix}";
        }
    }
    private static void DealComponent_Catperson(Deal deal)
    {
        deal.NoDiscardRounds();
    }
    private static bool CanAdd_Catperson(Deal deal)
    {
        return deal.DiscardRoundsRemaining == 1 && deal.CostPerDiscard == 0;
    }
    private static string GetText_Catperson(Player player, Bark bark)
    {
        switch (bark)
        {
            case Bark.LeavingPoor: return "I was never here.";
            case Bark.LeavingRich: return "No crying now, all this is mine...";
            default:
                throw new Exception($"No catman text for bark={bark}");
        }
    }
}

public class SpeciesSaveElement : SaveElement
{
    internal string Name { get; private set; }
    public SpeciesSaveElement()
    {
        SaveVersion = 1;
        Name = Species.Human.Name;
    }
    internal SpeciesSaveElement(Species species)
    {
        SaveVersion = 1;
        Name = species.Name;
    }
    protected override void LoadData(uint loadVersion, FileAccess access)
    {
        if (loadVersion != SaveVersion)
            throw new Exception($"No upgrade path from version {loadVersion} to {SaveVersion} for SpeciesElement");

        if (loadVersion >= 1)
        {
            Name = access.GetPascalString();
        }
    }

    protected override void SaveData(FileAccess access)
    {
        access.StorePascalString(Name);
    }
}

public class StaticSpeciesSaveElement : SaveElement
{
    internal Dictionary<string, Tuple<uint, uint>> SpeciesNameSteppers;

    public StaticSpeciesSaveElement()
    {
        SaveVersion = 1;
        SpeciesNameSteppers = new Dictionary<string, Tuple<uint, uint>>();
    }

    public StaticSpeciesSaveElement(Dictionary<string, Tuple<uint, uint>> speciesNameSteppers)
    {
        SaveVersion = 1;
        SpeciesNameSteppers = speciesNameSteppers;
    }

    protected override void LoadData(uint loadVersion, FileAccess access)
    {
        SpeciesNameSteppers.Clear();

        if (loadVersion >= 1)
        {
            uint entries = access.Get32();
            for (uint i = 0; i < entries; i++)
            {
                string key = access.GetPascalString();
                uint index = access.Get32();
                uint stepper = access.Get32();
                SpeciesNameSteppers[key] = Tuple.Create(index, stepper);
            }
        }
    }

    protected override void SaveData(FileAccess access)
    {
        access.Store32((uint) SpeciesNameSteppers.Count);
        foreach (KeyValuePair<string, Tuple<uint, uint>> entry in SpeciesNameSteppers)
        {
            access.StorePascalString(entry.Key);
            access.Store32(entry.Value.Item1);
            access.Store32(entry.Value.Item2);
        }
    }
}