using System.ComponentModel;
using Kicks;
using Sandbox;

public sealed class DeathTrigger : Sandbox.Component, Sandbox.Component.ITriggerListener
{
	protected override void OnUpdate()
	{

	}

	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		other.GameObject.Parent.Components.TryGet<PlayerController>( out var player );
		if (player is not null)
		{
			player.TakeDamage(10000);
		}
	}

	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{

	}
}
