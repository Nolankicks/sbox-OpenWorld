using Sandbox;

public sealed class Dummy : Component
{
	[Sync] int health { get; set; } = 100;

	protected override void OnUpdate()
	{

	}

	[Broadcast]
	public void Hurt(int damage)
	{
		if (IsProxy) return;
		health -= damage;
		if (health <= 0)
		{
			Kill();
		}
	}

	public void Kill()
	{
		GameObject.Destroy();
	}
	
}
