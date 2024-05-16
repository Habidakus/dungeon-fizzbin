using System;

#nullable enable

public partial class sms_change_species : state_machine_state
{

    public override void EnterState(Object? additionalInfo = null)
    {
        if (additionalInfo is Species species)
            GetMainNode().ChangeSpecies(species);
        else
            throw new Exception($"{additionalInfo} given to sms_change_species is not a species");

        GetStateMachine().SwitchState("Play_Deal");
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
