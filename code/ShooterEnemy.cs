using Kicks;
using Sandbox;
using Sandbox.Citizen;
using System.Threading;
using System.Threading.Tasks;
public sealed class ShooterEnemy : Component, Component.ITriggerListener
{
	[Property] public CitizenAnimationHelper citizenAnimationHelper { get; set; }
	[Property] public NavMeshAgent navMeshAgent { get; set; }
	[Sync] Vector3 WishVelocity { get; set; }
	[Property] public GameObject body { get; set; }
	[Sync] Vector3 Velocity { get; set; }
	[Sync] int health { get; set; } = 100;
	[Property] public SoundEvent ShootSound { get; set; }
	protected override void OnStart()
	{
		_ = FindPlayer();
		_ = TryAttackPlayer();
		citizenAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
	}

async Task FindPlayer()
{
    while (true)
    {
		if (navMeshAgent is null) return;
		Transform.Rotation = Rotation.FromYaw(Random.Shared.Int(90, 240)) * Transform.Rotation;
        var tr = Scene.Trace.Ray(Transform.Position, Transform.Position + Transform.Rotation.Forward * 1000).WithoutTags("enemy").Run();
        var targetPos = tr.EndPosition;
        while (Vector3.DistanceBetween(targetPos, Transform.Position) > 100)
        {
        if (navMeshAgent is not null)
		{
    	navMeshAgent.MoveTo(targetPos);
		}
            await Task.Delay(100);
        }

        // Wait a bit before finding the player again.
        await Task.Delay(1000);
    }
}

	protected override void OnUpdate()
	{
		if (navMeshAgent is null) return;
		WishVelocity = navMeshAgent.WishVelocity;
		Velocity = navMeshAgent.Velocity;
		citizenAnimationHelper.WithVelocity(Velocity);
		citizenAnimationHelper.WithWishVelocity(WishVelocity);	
	}

	async Task TryAttackPlayer()
	{
	while (true)
	{
		Attack();
		await Task.DelaySeconds(3);
	}
	}

	CancellationTokenSource lookAtPlayerCts;
void Attack()
{
		var tr = Scene.Trace.Ray(new Ray(Transform.Position + Vector3.Up * 55, Transform.Rotation.Forward), 5000).WithoutTags("enemy").Run();
		if (!tr.Hit) return;
		Log.Info(tr.GameObject);
		tr.GameObject.Components.TryGet<PlayerController>(out var player, FindMode.EverythingInSelfAndParent);
		if (player is not null)
		{
			Log.Info("Hit player");
			player.TakeDamage(10);
			citizenAnimationHelper.Target.Set("b_attack", true);
			Sound.Play(ShootSound, tr.EndPosition);
		}
}
void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		Log.Info("Triggered");
		other.GameObject.Components.TryGet<PlayerController>(out var player, FindMode.EverythingInSelfAndParent);
		if (player is not null)
		{
			lookAtPlayerCts = new CancellationTokenSource();
			_ = LookAtPlayer(player.GameObject, lookAtPlayerCts.Token);
		}
	}

void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
{
    other.GameObject.Components.TryGet<PlayerController>(out var player, FindMode.EverythingInSelfAndParent);
    if (player is not null)
    {
        lookAtPlayerCts?.Cancel();
    }
}

async Task LookAtPlayer(GameObject player, CancellationToken cancellationToken)
{
    while (true)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            break;
        }
		Log.Info("Looking at player");
        GameObject.Transform.Rotation = Rotation.LookAt(player.Transform.Position.WithZ(0) - GameObject.Transform.Position.WithZ(0));
        await Task.DelayRealtime(1);
    }
}
}
