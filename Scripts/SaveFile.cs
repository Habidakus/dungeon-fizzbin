using Godot;
using System;

#nullable enable

public class SaveFile
{
    internal MainElement wrapperElement { get; private set; }
    internal SaveFile(FileAccess access)
    {
        wrapperElement = new MainElement();
        wrapperElement.Load(access);
    }

    private SaveFile(Main main)
    {
        wrapperElement = new MainElement(main);
    }

    private void Save(FileAccess access)
    {
        wrapperElement.Save(access);
    }

    internal static void Save(Main main, string path)
    {
        using (FileAccess access = FileAccess.Open(path, FileAccess.ModeFlags.Write))
        {
            if (access != null)
            {
                SaveFile saveFile = new SaveFile(main);
                saveFile.Save(access);
                GD.Print($"Saved to {access.GetPathAbsolute()}");
            }
            else
            {
                GD.PrintErr($"Failed to save: {FileAccess.GetOpenError()}");
            }
        }
    }
}

public class MainElement : SaveElement
{
    internal PlayerElement PlayerEl { get; private set; }
    public MainElement()
    {
        SaveVersion = 1;
        PlayerEl = new PlayerElement();
    }
    internal MainElement(Main main)
    {
        SaveVersion = 1;
        PlayerEl = new PlayerElement(main.NonNPCPlayer);
    }
    protected override void LoadData(uint loadVersion, FileAccess access)
    {
        if (loadVersion != SaveVersion)
            throw new Exception($"No upgrade path from version {loadVersion} to {SaveVersion} for MainElement");

        if (loadVersion >= 1)
        {
            PlayerEl.Load(access);
        }
    }

    protected override void SaveData(FileAccess access)
    {
        PlayerEl.Save(access);
    }
}

public class SpeciesElement : SaveElement
{
    internal string Name { get; private set; }
    public SpeciesElement()
    {
        SaveVersion = 1;
        Name = "???";
    }
    internal SpeciesElement(Species species)
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

public class PlayerElement : SaveElement
{
    internal SpeciesElement SpeciesEl { get; private set; }
    internal double Wallet { get; private set; }
    public PlayerElement()
    {
        SaveVersion = 1;
        SpeciesEl = new SpeciesElement();
    }
    internal PlayerElement(Player player)
    {
        SaveVersion = 1;
        Wallet = player.Wallet;
        SpeciesEl = new SpeciesElement(player.Species);
    }

    protected override void LoadData(uint loadVersion, FileAccess access)
    {
        if (loadVersion != SaveVersion)
            throw new Exception($"No upgrade path from version {loadVersion} to {SaveVersion} for PlayerElement");

        if (loadVersion >= 1)
        {
            Wallet = access.GetDouble();
            SpeciesEl.Load(access);
        }
    }

    protected override void SaveData(FileAccess access)
    {
        access.StoreDouble(Wallet);
        SpeciesEl.Save(access);
    }
}

public abstract class SaveElement
{
    protected uint SaveVersion { get; set; }
    protected abstract void LoadData(uint loadVersion, FileAccess access);
    protected abstract void SaveData(FileAccess access);
    public void Save(FileAccess access)
    {
        access.Store32(SaveVersion);
        SaveData(access);
    }
    public void Load(FileAccess access)
    {
        uint version = access.Get32();
        LoadData(version, access);
    }
}