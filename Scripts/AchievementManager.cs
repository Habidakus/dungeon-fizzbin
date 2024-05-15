using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

public class AchievementUnlock : IComparable<AchievementUnlock>
{
    private Achievement _achievement;
    private int _levelReached;
    private string _text;

    public string Text { get { return _text; } }
    public bool IsBronze { get { return _levelReached == 0; } }
    public bool IsSilver { get { return _levelReached == 1; } }
    public bool IsGold { get { return _levelReached == 2; } }

    static string[] levelsAsText = new string[] { "Bronze", "Silver", "Gold" };

    public AchievementUnlock(Achievement ach, int levelReached, string text)
    {
        _achievement = ach;
        _levelReached = levelReached;
        _text = text;
    }

    public static AchievementUnlock? Generate(Achievement ach, uint[] levels, string text)
    {
        int levelReached = -1;
        for (int i = 0; i< levels.Count(); ++i)
        {
            if (ach.Count >= levels[i])
            {
                levelReached = i;
            }
        }
        
        if (levelReached < 0)
            return null;

        uint count = levels[levelReached];
        string unlockText = text;
        if (count > 1)
            unlockText = unlockText.Replace("#", count.ToString());
        else
            unlockText = unlockText.Replace("#", "one");
        unlockText = unlockText.Replace("GAMES", count > 1 ? "games" : "game");
        return new AchievementUnlock(ach, levelReached, unlockText);
    }

    public int CompareTo(AchievementUnlock? other)
    {
        if (other == null)
            throw new NullReferenceException();
        
        int comp = _levelReached.CompareTo(other._levelReached);
        if (comp != 0)
            return comp;

        return _achievement.CompareTo(other._achievement);
    }

    public override string ToString()
    {
        return $"{levelsAsText[_levelReached]}: {_text}";
    }
}

public class AchievementManager
{
    internal enum Categories {
        CAT_PLAY_AGAINST,
        CAT_FORCED_US_TO_FOLD,
        CAT_LOST_TO_IN_A_SHOWDOWN ,
        CAT_WE_FORCED_THEM_TO_FOLD,
        CAT_WE_WON_AGAINST,
        CAT_THEY_LEFT_WITH_NO_MONEY,
        CAT_THEY_LEFT_WITH_OUR_MONEY,
        CAT_WE_PLAYED_A_HAND_AND_FOLDED,
        CAT_WE_PLAYED_A_HAND_TO_THE_END_AND_LOST,
        CAT_WE_PLAYED_A_HAND_TO_THE_END_AND_WON,
        CAT_WE_WON_WITH_A_RANKING,
    }

    private Dictionary<Tuple<Categories, string>, Achievement> _achievements = new();

    internal AchievementsSaveElement GenerateAchievementsSaveElement()
    {
        return new AchievementsSaveElement(_achievements);
    }

    internal void Load(AchievementsSaveElement achievementsEl)
    {
        _achievements = achievementsEl._achievements;
    }

    static readonly float s_minUnlock = 5f;
    static readonly Dictionary<Categories, float> s_unlockFractions = new Dictionary<Categories, float>()
    {
        { Categories.CAT_PLAY_AGAINST, 0.1f },
        { Categories.CAT_FORCED_US_TO_FOLD, 0.1f },
        { Categories.CAT_LOST_TO_IN_A_SHOWDOWN, 0.2f },
        { Categories.CAT_WE_FORCED_THEM_TO_FOLD, 0.1f },
        { Categories.CAT_WE_WON_AGAINST, 0.35f },
        { Categories.CAT_THEY_LEFT_WITH_NO_MONEY, 0.10f },
        { Categories.CAT_THEY_LEFT_WITH_OUR_MONEY, 0.10f },
    };

    internal float GetUnlockedFraction(Species species)
    {
        float total = 0;
        foreach (Achievement ach in _achievements.Values)
        {
            if (ach.SubCat == species.Name)
            {
                if (s_unlockFractions.TryGetValue(ach.Category, out float multiplier))
                {
                    total += multiplier * ach.Count;
                }
            }
        }

        return total / s_minUnlock;
    }

    private AchievementUnlock? GetUnlock(Achievement ach)
    {
        switch (ach.Category)
        {
            case Categories.CAT_PLAY_AGAINST:
                return AchievementUnlock.Generate(ach, new uint[] { 10, 100, 500 }, $"Played # interesting hands against a {ach.SubCat}");
            case Categories.CAT_FORCED_US_TO_FOLD:
                break;
            case Categories.CAT_LOST_TO_IN_A_SHOWDOWN:
                break;
            case Categories.CAT_WE_FORCED_THEM_TO_FOLD:
                return AchievementUnlock.Generate(ach, new uint[] { 10, 100, 500 }, $"Saw # {ach.SubCat} fold against us");
            case Categories.CAT_WE_WON_AGAINST:
                return AchievementUnlock.Generate(ach, new uint[] { 1, 25, 25 }, $"Won # GAMES against a {ach.SubCat}");
            case Categories.CAT_THEY_LEFT_WITH_NO_MONEY:
                return AchievementUnlock.Generate(ach, new uint[] { 1, 5, 25 }, $"Saw # {ach.SubCat} leave the table broken");
            case Categories.CAT_THEY_LEFT_WITH_OUR_MONEY:
                return AchievementUnlock.Generate(ach, new uint[] { 1, 5, 25 }, $"Saw # {ach.SubCat} leave the table with our money");
            case Categories.CAT_WE_PLAYED_A_HAND_AND_FOLDED:
                return AchievementUnlock.Generate(ach, new uint[] { 100, 500, 2500 }, $"Folded # times as a {ach.SubCat}");
            case Categories.CAT_WE_PLAYED_A_HAND_TO_THE_END_AND_LOST:
                break;
            case Categories.CAT_WE_PLAYED_A_HAND_TO_THE_END_AND_WON:
                return AchievementUnlock.Generate(ach, new uint[] { 1, 25, 25 }, $"Won # GAMES as a {ach.SubCat}");
            case Categories.CAT_WE_WON_WITH_A_RANKING:
                return AchievementUnlock.Generate(ach, new uint[] { 1, 25, 25 }, $"Won # GAMES with a {ach.SubCat}");
            default:
                throw new NotImplementedException($"There is no Unlock implemented for achievement category {ach.Category}");
        }

        return null;
    }

    public IEnumerable<AchievementUnlock> AchievementsUnlocked
    {
        get
        {
            foreach (Achievement ach in _achievements.Values.OrderBy(a => a))
            {
                AchievementUnlock? unlock = GetUnlock(ach);
                if (unlock != null)
                {
                    yield return unlock;
                }
            }
        }
    }

    private Achievement GetAchievement(Species species, Categories category)
    {
        return GetAchievement(category, species.Name);
    }

    private Achievement GetAchievement(HandValue.HandRanking ranking, Categories category)
    {
        return GetAchievement(category, HandValue.GetPlayerFacingTextForHandRanking(ranking));
    }

    private Achievement GetAchievement(Categories category, string subCat)
    {
        Tuple<Categories, string> key = Tuple.Create(category, subCat);
        if (_achievements.TryGetValue(key, out Achievement? achievement))
        {
            return achievement;
        }
        else
        {
            Achievement retVal = new Achievement(category, subCat, 0);
            _achievements[key] = retVal;
            return retVal;
        }
    }

    internal void TrackGamesAgainstSpecies(Species species)
    {
        GetAchievement(species, Categories.CAT_PLAY_AGAINST).Increase();
    }

    internal void TrackLossesToSpecies(Species species, bool hasFolded)
    {
        if (hasFolded)
        {
            GetAchievement(species, Categories.CAT_FORCED_US_TO_FOLD).Increase();
        }
        else
        {
            GetAchievement(species, Categories.CAT_LOST_TO_IN_A_SHOWDOWN).Increase();
        }
    }

    internal void TrackWinsAgainstSpecies(Species species, bool hasFolded)
    {
        if (hasFolded)
        {
            GetAchievement(species, Categories.CAT_WE_FORCED_THEM_TO_FOLD).Increase();
        }
        else
        {
            GetAchievement(species, Categories.CAT_WE_WON_AGAINST).Increase();
        }
    }

    internal void TrackSpeciesLeavingTable(Species species, bool becauseTheyArePoor)
    {
        if (becauseTheyArePoor)
            GetAchievement(species, Categories.CAT_THEY_LEFT_WITH_NO_MONEY).Increase();
        else
            GetAchievement(species, Categories.CAT_THEY_LEFT_WITH_OUR_MONEY).Increase();
    }

    internal void TrackPlaysAsSpecies(Species species, bool hasFolded)
    {
        if (hasFolded)
            GetAchievement(species, Categories.CAT_WE_PLAYED_A_HAND_AND_FOLDED).Increase();
        else
            GetAchievement(species, Categories.CAT_WE_PLAYED_A_HAND_TO_THE_END_AND_LOST).Increase();
    }

    internal void TrackWinsAsSpecies(Species species, HandValue.HandRanking ranking)
    {
        GetAchievement(species, Categories.CAT_WE_PLAYED_A_HAND_TO_THE_END_AND_WON).Increase();
        GetAchievement(ranking, Categories.CAT_WE_WON_WITH_A_RANKING).Increase();
    }
}

public class Achievement : IComparable<Achievement>
{
    internal AchievementManager.Categories Category { get; private set; }
    public string SubCat { get; private set; }
    public uint Count { get; private set; } = 0;

    internal Achievement(AchievementManager.Categories category, string subCat, uint count)
    {
        Category = category;
        SubCat = subCat;
        Count = count;
    }

    public void Increase()
    {
        Count += 1;
    }

    public override string ToString()
    {
        return $"{Category} {SubCat} {Count}";
    }

    public int CompareTo(Achievement? other)
    {
        if (other == null)
            throw new ArgumentNullException("other");

        int comp = Category.CompareTo(other.Category);
        if (comp != 0)
            return comp;

        comp = SubCat.CompareTo(other.SubCat);
        if (comp != 0)
            return comp;

        comp = Count.CompareTo(other.Count);
        return comp;
    }
}

public class AchievementsSaveElement : SaveElement
{
    internal Dictionary<Tuple<AchievementManager.Categories, string>, Achievement> _achievements;

    internal AchievementsSaveElement()
    {
        SaveVersion = 3;
        _achievements = new();
    }

    internal AchievementsSaveElement(Dictionary<Tuple<AchievementManager.Categories, string>, Achievement> achievements)
    {
        SaveVersion = 3;
        _achievements = achievements;
    }

    protected override void LoadData(uint loadVersion, FileAccess access)
    {
        if (loadVersion >= 3)
        {
            _achievements.Clear();
            uint entries = access.Get32();
            for (int i = 0; i < entries; i++)
            {
                AchievementManager.Categories cat = (AchievementManager.Categories) access.Get16();
                string sub = access.GetPascalString();
                uint count = access.Get32();
                _achievements[Tuple.Create(cat, sub)] = new Achievement(cat, sub, count);
            }
        }

        //foreach (Achievement? ach in _achievements.Values.OrderBy(a => a))
        //{
        //    GD.Print(ach);
        //}

        if (loadVersion > SaveVersion)
        {
            throw new Exception($"Loading version {loadVersion} of AchievementSaveElemenet but only support version {SaveVersion}");
        }
    }

    protected override void SaveData(FileAccess access)
    {
        access.Store32((uint)_achievements.Count);
        foreach (Achievement achievement in _achievements.Values)
        {
            access.Store16((ushort)achievement.Category);
            access.StorePascalString(achievement.SubCat);
            access.Store32(achievement.Count);
        }
    }
}
