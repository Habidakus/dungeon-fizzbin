using Godot;
using System;

#nullable enable

public partial class sms_play_loop : state_machine_state
{
    public override void EnterState(Object? additionalInfo = null)
    {
    }

    public override void Update(double delta)
    {
        int highlightPositionId = -1;
        if (GetMainNode().SomeoneNeedsToPass(out highlightPositionId))
        {
            GetHUD().HighlightPosition(highlightPositionId);
            GetStateMachine().SwitchState("play_someone_passes");
        }
        else if (GetMainNode().NeedsToResolvePassAndRiver(out highlightPositionId))
        {
            GetHUD().HighlightPosition(highlightPositionId);
            GetStateMachine().SwitchState("play_resolve_passandriver");
        }
        else if (GetMainNode().SomeoneNeedsToDiscard(out highlightPositionId))
        {
            GetHUD().HighlightPosition(highlightPositionId);
            GetStateMachine().SwitchState("play_someone_discards");
        }
        else if (GetMainNode().HasPostDiscard())
        {
            GetHUD().HighlightPosition(-1);
            GetStateMachine().SwitchState("play_post_discard");
        }
        else if (GetMainNode().SomeoneNeedsToBet(out highlightPositionId))
        {
            GetHUD().HighlightPosition(highlightPositionId);
            GetStateMachine().SwitchState("play_someone_bets");
        }
        else if (GetMainNode().SomeoneNeedsToReveal(out highlightPositionId))
        {
            GetHUD().HighlightPosition(highlightPositionId);
            GetStateMachine().SwitchState("play_someone_reveals");
        }
        else if (GetMainNode().NeedToDeclareWinner(out highlightPositionId))
        {
            GetHUD().HighlightPosition(highlightPositionId);
            GetStateMachine().SwitchState("play_declare_winner");
        }
        else if (GetMainNode().SomeoneNeedsToLeaveGame(out highlightPositionId))
        {
            GetHUD().HighlightPosition(highlightPositionId);
            GetStateMachine().SwitchState("play_player_leaves_game");
        }
        else
        {
            GetHUD().HighlightPosition(-1);
            GetStateMachine().SwitchState("play_offer_another_hand");
        }

        //GetHUD().HighlightPosition(-1);
    }

    public override void ExitState()
    {
    }
}
