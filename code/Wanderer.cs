using Sandbox;
namespace Kicks;
[Icon("hiking")]
public sealed class Wanderer : Component
{
	[Property] public NavMeshAgent navMeshAgent { get; set; }
	[Sync] Vector3 WishVelocity { get; set; }
	[Sync] Vector3 Velocity { get; set; }
	[Property, Sync] public int distance { get; set; } = 500; 
	[Property, Sync] public int separation { get; set; } = 150;
	public delegate void NPCActionDelegate(bool Wandering, Vector3 WishVelocity, Vector3 Velocity, GameObject Target, bool Separating);
	[Property, Title("NPC Action")] public NPCActionDelegate npcAction { get; set; }
	protected override void OnStart()
	{
		_ = Wander(distance);
	}
	
	async Task Wander(int distance)
{
    while (true)
    {
        if (navMeshAgent is null) return;
        var player = GetClosestPlayer();
        if (player == null)
        {
            await WanderRandomly();
        }
        else if (IsPlayerWithinDistance(player, distance))
        {
            await MoveTowardsPlayer(player);
        }
        else
        {
            await WanderRandomly();
        }
        await Task.Delay(1);
    }
}

PlayerController GetClosestPlayer()
{
    var players = Scene.GetAllComponents<PlayerController>().ToList();
    if (players.Count == 0) return null;
    return players.OrderBy(x => Vector3.DistanceBetween(x.Transform.Position, Transform.Position)).FirstOrDefault();
}

bool IsPlayerWithinDistance(PlayerController player, int distance)
{
    return Vector3.DistanceBetween(player.Transform.Position, Transform.Position) <= distance;
}

async Task MoveTowardsPlayer(PlayerController player)
{
    while (IsPlayerWithinDistance(player, distance))
    {
		
		if (Vector3.DistanceBetween(player.Transform.Position, Transform.Position) > separation)
		{
			npcAction?.Invoke(false, WishVelocity, Velocity, player.GameObject, false);
			navMeshAgent.MoveTo(player.Transform.Position);
		}
        else
		{
			npcAction?.Invoke(false, WishVelocity, Velocity, player.GameObject, true);
			navMeshAgent.Stop();
		}
        await Task.Delay(1);
    }
}

async Task WanderRandomly()
{
    Transform.Rotation = Rotation.FromYaw(Random.Shared.Int(90, 240)) * Transform.Rotation;
    var tr = Scene.Trace.Ray(Transform.Position, Transform.Position + Transform.Rotation.Forward * 1000).WithoutTags("enemy").Run();
    var targetPos = tr.EndPosition;
    while (Vector3.DistanceBetween(targetPos, Transform.Position) > 100)
    {
		npcAction?.Invoke(true, WishVelocity, Velocity, null, false);
        if (navMeshAgent is not null)
        {
            navMeshAgent.MoveTo(targetPos);
        }

        var player = GetClosestPlayer();
        if (player != null && IsPlayerWithinDistance(player, distance))
        {
            break;
        }

        await Task.Delay(1);
    }
}
	
	protected override void OnUpdate()
	{
		if (navMeshAgent is null) return;
		WishVelocity = navMeshAgent.WishVelocity;
		Velocity = navMeshAgent.Velocity;
	}
}
