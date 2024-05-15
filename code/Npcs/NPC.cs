using Sandbox;
using Sandbox.Citizen;
using System; 
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
namespace Kicks;

public sealed class NPC : Component
{
	public delegate Task NPCActionDelegate();
	public delegate void OnAttackDelegate(GameObject Target, float TraceLength, int Damage);
	[Property, Title("NPC Action")] public NPCActionDelegate npcAction { get; set; }
	[Property] public OnAttackDelegate OnAttack { get; set; }
	[Sync] public Vector3 SyncedWishVelocity { get; set; }
	[Sync] public Vector3 SyncedVelocity { get; set; }
	
	[ActionGraphNode("NPC Attack"), Impure]
	public void ClosestPlayer(out PlayerController ClosestPlayer, out bool FoundPlayer)
	{
		var players = Scene.GetAllComponents<PlayerController>().ToList();
		var closestPlayers = players.OrderBy(x => Vector3.DistanceBetween(x.Transform.Position, Transform.Position)).ToList();
		if (closestPlayers.Count == 0)
		{
			FoundPlayer = false;
			ClosestPlayer = null;
			return;
		}
		ClosestPlayer = closestPlayers.FirstOrDefault();
		FoundPlayer = true;
	}

	[ActionGraphNode("Move To Target"), Impure]
	public void MoveToTarget(GameObject Target, NavMeshAgent navMeshAgent, float Distance, out bool Result)
	{
		if (Vector3.DistanceBetween(Target.Transform.Position, Transform.Position) > Distance)
		{
			navMeshAgent.Stop();
			Result = false;
		}
		else
		{
			navMeshAgent.MoveTo(Target.Transform.Position);
			Result = true;
			if (Vector3.DistanceBetween(Target.Transform.Position, Transform.Position) < 150)
			{
				navMeshAgent.Stop();
			}
		}
	}
	[ActionGraphNode("Look At Target")]
	public void LookAtTarget(GameObject Body, GameObject Target)
	{
		var targetRot = Rotation.LookAt(Target.Transform.Position.WithZ(Transform.Position.z) - Transform.Position, Vector3.Up);
		Body.Transform.Rotation = Rotation.Slerp(Body.Transform.Rotation, targetRot, Time.Delta * 10f);
	}
	[ActionGraphNode("Get Synced Velocity"), Impure]
	public void GetSyncedVelocity(out Vector3 Velocity, out Vector3 WishVelocity, NavMeshAgent navMeshAgent)
	{
		SyncedWishVelocity = navMeshAgent.WishVelocity;
		SyncedVelocity = navMeshAgent.Velocity;
		WishVelocity = SyncedWishVelocity;
		Velocity = SyncedVelocity;
	}
	[Title("Set NPC Anims")]
	public void SetNPCAnims(CitizenAnimationHelper citizenAnimationHelper, Vector3 Velocity, Vector3 WishVelocity)
	{
		citizenAnimationHelper.WithVelocity(Velocity);
		citizenAnimationHelper.WithWishVelocity(WishVelocity);
	}
	public void NPCAttack(GameObject Target, float TraceLength, int Damage, bool ChangeScene = true)
	{
        Vector3 directionToTarget = (Target.Transform.Position - Transform.Position).Normal;
        var tr = Scene.Trace.Ray(Transform.Position + Vector3.Up * 55, directionToTarget * TraceLength).Run();
        if (tr.Hit)
        {
            tr.GameObject.Components.TryGet<PlayerController>(out var player, FindMode.EverythingInSelfAndParent);
            if (player is not null)
            {
                player.TakeDamage(Damage, ChangeScene);
				OnAttack?.Invoke(player.GameObject, TraceLength, Damage);
            }
        }
}

	protected override void OnFixedUpdate()
	{
		npcAction?.Invoke().ContinueWith(task => {});
	}
}
