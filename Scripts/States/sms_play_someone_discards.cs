using Godot;
using System;

#nullable enable

public partial class sms_play_someone_discards : state_machine_state
{
    private int _positionID = -1;

    public override void EnterState(Object? additionalInfo = null)
    {
        _positionID = -1;
        GetMainNode().ForceSomeoneToDiscard(ConfirmDiscardEvent);
    }

    internal void ConfirmDiscardEvent(int positionID)
    {
        _positionID = positionID;
    }

    public override void Update(double delta)
    {
        if (_positionID != -1)
        {
            GetMainNode().ForceSomeoneToDiscard_Post(_positionID);
            _positionID = -1;
        }
    }

    public override void ExitState()
    {
    }
}
