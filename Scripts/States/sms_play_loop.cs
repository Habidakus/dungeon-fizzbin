using Godot;
using System;

#nullable enable

public partial class sms_play_loop : state_machine_state
{
    public override void EnterState()
    {
    }

    public override void Update(double delta)
    {
        if (GetMainNode().SomeoneNeedsToDiscard())
        {
            GetStateMachine().SwitchState("play_someone_discards");
        }
        else if (GetMainNode().SomeoneNeedsToBet())
        {
            GetStateMachine().SwitchState("play_someone_bets");
        }
        else if (GetMainNode().SomeoneNeedsToReveal())
        {
            GetStateMachine().SwitchState("play_someone_reveals");
        }
        else
        {
            GetStateMachine().SwitchState("play_declare_winner");
        }
    }

    public override void ExitState()
    {
    }
}
