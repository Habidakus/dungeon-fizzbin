using Godot;
using System;

#nullable enable

public partial class sms_play_someone_passes : state_machine_state
{
    private double _wait = 0;
    private double _delay = 1;
    private bool _confirmed = false;
    private int _positionID = -1;

    public override void EnterState(Object? additionalInfo = null)
    {
        _confirmed = false;
        _positionID = -1;
        GetMainNode().ForceSomeoneToPass(ConfirmingPassCardsDetermined);
    }

    internal void ConfirmingPassCardsDetermined(int positionID)
    {
        _wait = _delay;
        _confirmed = true;
        _positionID = positionID;
    }

    public override void Update(double delta)
    {
        if (_confirmed)
        {
            if (_positionID != -1)
            {
                GetMainNode().ForceSomeoneToPass_Post(_positionID, _delay * 0.8);
                _positionID = -1;
            }
            else
            {
                _wait -= delta;
                if (_wait <= 0)
                {
                    GetStateMachine().SwitchState("Play_Loop");
                    _confirmed = false;
                }
            }
        }
    }

    public override void ExitState()
    {
    }
}
