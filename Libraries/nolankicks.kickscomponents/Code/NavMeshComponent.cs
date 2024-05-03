using System.ComponentModel;
using Sandbox;

public sealed class NavMeshComponent : Sandbox.Component
{
	[Property] public NavMeshAgent Agent { get; set; }
	[Property] public GameObject Target { get; set; }

	protected override void OnFixedUpdate()
	{
		MoveToTarget();
	}

	void MoveToTarget()
	{
		if (Agent is null || Target is null) return;
		Agent.MoveTo(Target.Transform.Position);
	}
}
