using Godot;
using System.Linq;

#nullable enable

public partial class sms_playAsNewSpecies : state_machine_state
{

    public override void EnterState()
    {
        AchievementManager achievments = GetMainNode().Achievments;
        GetHUD().SetSelectSpecies(Species.GetUnlockedSpeciesAndFraction(achievments).Where(a => a.Item2 >= 0f).Select(a => a.Item1).ToArray());
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
