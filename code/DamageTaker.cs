using System;
using Sandbox;

public sealed class DamageTaker : Component
{
	[Property] public Action OnTakeDamage { get; set; }
	[Property] public Action OnDeathAction { get; set; }
	[Property] public int Health { get; set; } = 100;
	[Property, Sync] public bool CanDie { get; set; } = true;
	protected override void OnUpdate()
	{

	}
	[Broadcast]
	public void TakeDamage(int damage)
	{
		if (IsProxy) return;
		Health -= damage;
		OnTakeDamage?.Invoke();
		if (Health <= 0 && CanDie)
		{
			OnDeath();
		}
	}

	void OnDeath()
	{
		OnDeathAction?.Invoke();
	}

}
