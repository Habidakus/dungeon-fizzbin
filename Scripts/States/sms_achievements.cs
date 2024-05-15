using Godot;
using System;
using System.Linq;

public partial class sms_achievements : state_machine_state
{
    private AchievementUnlock[] _unlockedAchievements;

    public override void EnterState()
    {
        _unlockedAchievements = GetMainNode().Achievments.AchievementsUnlocked.ToArray();
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
