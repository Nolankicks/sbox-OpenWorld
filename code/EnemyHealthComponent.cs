using Sandbox;
namespace Kicks;
public sealed class EnemyHealthComponent : Component
{
	[Sync] public int health { get; set; } = 100;
	protected override void OnUpdate()
	{

	}

	[Broadcast]
	public void Hurt(int damage, Guid player)
	{
		if (IsProxy) return;
		health -= damage;
		if (health <= 0)
		{
			Kill();
			var attacker = Scene.Directory.FindByGuid(player);
			var playerController = attacker.Components.Get<PlayerController>();
			if (playerController is null) return;
		}
	}

	public void Kill()
	{
		GameObject.Destroy();
	}
}
