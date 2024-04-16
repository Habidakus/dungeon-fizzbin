using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class state_machine : Node
{
	private state_machine_state _current_state = null;
	private Dictionary<string, state_machine_state> _all_states = new Dictionary<string, state_machine_state>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_all_states.Clear();

		foreach (Node child in GetChildren())
		{
			if (child is state_machine_state sms)
			{
				_all_states.Add(sms.Name.ToString().ToLower(), sms);
			}
		}

		SwitchState("StartUp");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_current_state != null)
		{
			_current_state.Update(delta);
		}
	}

	public void SwitchState(string stateName)
	{
		if (_current_state != null)
		{
			_current_state.ExitState();
            GetHUD().EndState(_current_state.Name.ToString());
        }


        if (!_all_states.TryGetValue(stateName.ToLower(), out _current_state))
		{
			_current_state = null;
			throw new Exception($"Failed to find state \"{stateName}\"");
		}

        GetHUD().StartState(_current_state.Name.ToString());
        _current_state.EnterState();
    }

    public HUD GetHUD()
	{
		if (GetParent() is Node mainNode)
		{
			if (mainNode.FindChild("HUD") is HUD hud)
			{
				return hud;
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				foreach (var node in mainNode.GetChildren())
				{
					if (sb.Length > 0)
						sb.Append(", ");
					sb.Append(node.Name);
				}
				throw new Exception($"state machine parent {mainNode.Name} has no child %HUD. Children are: {sb.ToString()}");
			}
		}
		else
		{
			throw new Exception("state machine has no parent");
		}
	}
}
