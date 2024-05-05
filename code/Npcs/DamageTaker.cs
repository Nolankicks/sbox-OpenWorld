using System;
using Kicks;
using Sandbox;
[Icon("download")]
public sealed class DamageTaker : Component
{
	public delegate void OnTakeDamageVoid(PlayerController PlayerController, Inventory Inventory);
	[Property] public OnTakeDamageVoid OnTakeDamage  { get; set; }
	public delegate void OnDeathActionVoid(PlayerController PlayerController, Inventory Inventory);
	[Property] public OnDeathActionVoid OnDeathAction { get; set; }
	[Property] public int Health { get; set; } = 100;
	[Property, Sync] public bool CanDie { get; set; } = true;
	protected override void OnUpdate()
	{

	}
	[Broadcast]
	public void TakeDamage(int damage, Guid guid)
	{
		if (IsProxy) return;
		Health -= damage;
		var player = Scene.Directory.FindByGuid(guid);
		if (player is null) return;
		player.Components.TryGet<PlayerController>(out var playerController, FindMode.EverythingInSelfAndParent);
		player.Components.TryGet<Inventory>(out var inventory, FindMode.EverythingInSelfAndParent);
		if (playerController is null || inventory is null) return;
		OnTakeDamage?.Invoke(playerController, inventory);
		if (Health <= 0 && CanDie)
		{
			OnDeath(playerController, inventory);
		}
	}

	void OnDeath(PlayerController playerController, Inventory inventory)
	{
		OnDeathAction?.Invoke(playerController, inventory);
	}

}
