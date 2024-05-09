using Godot;
using System;

#nullable enable

public class SaveFile
{
    internal static bool Load(SaveElement baseElement, string path)
    {
        using (FileAccess access = FileAccess.Open(path, FileAccess.ModeFlags.Read))
        {
            if (access != null)
            {
                baseElement.Load(access);
                return true;
            }
        }

        return false;
    }

    internal static void Save(SaveElement baseElement, string path)
    {
        using (FileAccess access = FileAccess.Open(path, FileAccess.ModeFlags.Write))
        {
            if (access != null)
            {
                baseElement.Save(access);
                GD.Print($"Saved to {access.GetPathAbsolute()}");
            }
            else
            {
                GD.PrintErr($"Failed to save: {FileAccess.GetOpenError()}");
            }
        }
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