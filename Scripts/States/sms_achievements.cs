using Godot;
using System;
using System.Linq;

#nullable enable

public partial class sms_achievements : state_machine_state
{

    public override void EnterState(Object? additionalInfo = null)
    {
        AchievementManager achievments = GetMainNode().Achievments;
        GetHUD().SetAchievmentsAndUnlocks(
            achievments.AchievementsUnlocked.ToArray(),
            Species.GetUnlockedSpeciesPlayableAndFraction(achievments).ToArray());
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
