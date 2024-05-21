using Godot;
using System;
using System.Linq;

#nullable enable

public partial class sms_show_revealed_rules : state_machine_state
{
    public override void EnterState(Object? additionalInfo = null)
    {
        AchievementManager achievments = GetMainNode().Achievments;
        GetHUD().SetRevealedRules(Species.GetUnlockedSpeciesRules(achievments).ToArray());
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
