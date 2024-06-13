using Sandbox;
namespace Kicks;
[Icon("favorite")]
public sealed class EnemyHealthComponent : Component
{
    [Property, Sync] public int health { get; set; } = 100;
    private bool isDead = false; // Add this line
    public delegate void OnHurtDelgate(PlayerController playerController, Inventory inventory, AmmoContainer ammoContainer);
    public delegate void OnDeathDelegate(PlayerController playerController, Inventory inventory, AmmoContainer ammoContainer);
    [Property] public bool CanBeKilled { get; set; } = true;
    [Property] public OnHurtDelgate OnHurt { get; set; }
    [Property] public OnDeathDelegate OnDeath { get; set; }
	[Property] public bool CanDie { get; set; } = true;
    protected override void OnUpdate()
    {

    }

    [Broadcast]
    public void Hurt(int damage, Guid player)
    {
        if (IsProxy || isDead || !CanBeKilled) return; // Check the isDead flag here
        health -= damage;
        var attacker = Scene.Directory.FindByGuid(player);
        attacker.Components.TryGet<PlayerController>( out var playerController, FindMode.EverythingInSelfAndParent );
        attacker.Components.TryGet<Inventory>( out var inventory, FindMode.EverythingInSelfAndParent );
        attacker.Components.TryGet<AmmoContainer>( out var ammoContainer, FindMode.EverythingInSelfAndParent );
        if (playerController is null || inventory is null || ammoContainer is null) return;
        OnHurt?.Invoke( playerController, inventory, ammoContainer );
        if (health <= 0)
        {
            Kill(playerController, inventory, ammoContainer);
        }
    }

    public void Kill(PlayerController playerController, Inventory inventory, AmmoContainer ammoContainer)
    {
        if (isDead) return; // Check the isDead flag here
        isDead = true; // Set the isDead flag to true
        OnDeath?.Invoke(playerController, inventory, ammoContainer);
		if (CanDie)
		{
			GameObject.Destroy();
		}
    }
}
