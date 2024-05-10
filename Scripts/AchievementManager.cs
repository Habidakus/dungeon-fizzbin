using Godot;
using System;
using System.Collections.Generic;

#nullable enable

public class AchievementManager
{
    private Dictionary<string, Achievement> _achievements = new();

    internal AchievementsSaveElement GenerateAchievementsSaveElement()
    {
        return new AchievementsSaveElement(_achievements);
    }

    internal void Load(AchievementsSaveElement achievementsEl)
    {
        _achievements = achievementsEl._achievements;
    }

    private Achievement GetAchievement(Species species, string category)
    {
        string hash = $"{species.Name}_{category}";
        if (_achievements.TryGetValue(hash, out Achievement? achievement))
        {
            return achievement;
        }
        else
        {
            Achievement retVal = new Achievement(hash, 0);
            _achievements[hash] = retVal;
            return retVal;
        }
    }

    internal void TrackGamesAgainstSpecies(Species species)
    {
        GetAchievement(species, "game_against").Increase();
    }

    internal void TrackLossesToSpecies(Species species, bool hasFolded)
    {
        if (hasFolded)
        {
            GetAchievement(species, "folded_before").Increase();
        }
        else
        {
            GetAchievement(species, "lost_too").Increase();
        }
    }

    internal void TrackWinsAgainstSpecies(Species species, bool hasFolded)
    {
        if (hasFolded)
        {
            GetAchievement(species, "caused_to_folded").Increase();
        }
        else
        {
            GetAchievement(species, "won_against").Increase();
        }
    }

    internal void TrackSpeciesLeavingTable(Species species, bool becauseTheyArePoor)
    {
        if (becauseTheyArePoor)
            GetAchievement(species, "outlasted").Increase();
        else
            GetAchievement(species, "humbled_by").Increase();
    }

    internal void TrackPlaysAsSpecies(Species species, bool hasFolded)
    {
        if (hasFolded)
            GetAchievement(species, "folded_as").Increase();
        else
            GetAchievement(species, "revealed_as").Increase();
    }

    internal void TrackWinsAsSpecies(Species species)
    {
        GetAchievement(species, "won_as").Increase();
    }
}

public class Achievement
{
    public string Name { get; private set; }
    public uint Count { get; private set; } = 0;

    internal Achievement(string name, uint count)
    {
        Name = name;
        Count = count;
    }

    public void Increase()
    {
        Count += 1;
    }
}

public class AchievementsSaveElement : SaveElement
{
    internal Dictionary<string, Achievement> _achievements;

    internal AchievementsSaveElement()
    {
        SaveVersion = 1;
        _achievements = new();
    }

    internal AchievementsSaveElement(Dictionary<string, Achievement> achievements)
    {
        SaveVersion = 1;
        _achievements = achievements;

        foreach (var ach in _achievements.Values)
        {
            GD.Print($"{ach.Name} {ach.Count}");
        }
    }

    protected override void LoadData(uint loadVersion, FileAccess access)
    {
        if (loadVersion >= 1)
        {
            _achievements.Clear();
            uint entries = access.Get32();
            for (int i = 0; i < entries; i++)
            {
                string key = access.GetPascalString();
                uint count = access.Get32();
                _achievements[key] = new Achievement(key, count);
            }
        }

        foreach (var ach in _achievements.Values)
        {
            GD.Print($"{ach.Name} {ach.Count}");
        }

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
            access.StorePascalString(achievement.Name);
            access.Store32(achievement.Count);
        }
    }
}
