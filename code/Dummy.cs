using Sandbox;

public sealed class Dummy : Component
{
	[Sync] int health { get; set; } = 100;
	[Property] public NavMeshAgent navMeshAgent { get; set; }
	public PlayerController player;
	protected override void OnStart()
	{
		player = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
	}
	protected override void OnUpdate()
	{
		//SphereTrace();
	}

	void SphereTrace()
	{
		var tr = Scene.Trace.Radius(500).WithoutTags("dummy", "map").Run();
		{
			if (tr.Hit)
			{
				Log.Info(tr.GameObject);
			}
		}
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
