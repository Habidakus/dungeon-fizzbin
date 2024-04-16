using Godot;
using System;

public partial class state_machine_state : Node
{
    public virtual void EnterState()
    {
    }

    public virtual void Update(double delta)
    {
    }

    public virtual void ExitState()
    {
    }

    protected HUD GetHUD()
    {
        return GetStateMachine().GetHUD();
    }

    protected state_machine GetStateMachine()
    {
        if (GetParent() is state_machine sm)
        {
            return sm;
        }

        throw new Exception($"sms_start_up {this.Name} is not a child of a state machine");
    }
}
