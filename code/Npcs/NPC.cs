using Sandbox;
using Sandbox.Citizen;
using System;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
namespace Kicks;
[Icon("person"), Description("Gives access to NPC actions.")]
public sealed class NPC : Component
{
	public delegate void NPCActionDelegate();
	public delegate void OnAttackDelegate(GameObject Target, float TraceLength, int Damage);
	public delegate Task OnAttackDelegateAsync(GameObject Target, float TraceLength, int Damage);
	[Property, Title("NPC Action")] public NPCActionDelegate npcAction { get; set; }
	[Sync] public bool Wandering { get; set; }
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
	public void MoveToTarget(GameObject Target, NavMeshAgent navMeshAgent, float Distance, out bool Result, int Avoidance = 150)
	{
		if (Vector3.DistanceBetween(Target.Transform.Position, Transform.Position) > Distance)
		{
			navMeshAgent.Stop();
			Result = false;
		}
		else
		{
			if (Vector3.DistanceBetween(Target.Transform.Position, Transform.Position) < Avoidance)
			{
				navMeshAgent.Stop();
			}
			else
			{
				navMeshAgent.MoveTo(Target.Transform.Position);
			}
			Result = true;
		}
	}

	private PlayerController NonActionGraphFindPlayer()
	{
		var players = Scene.GetAllComponents<PlayerController>().ToList();
		var closestPlayers = players.OrderBy(x => Vector3.DistanceBetween(x.Transform.Position, Transform.Position)).ToList();
		if (closestPlayers.Count == 0)
		{
			return null;
		}
		return closestPlayers.FirstOrDefault();
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
	[Impure]
	public void NPCAttack(GameObject Target, float TraceLength, int Damage, out bool TraceHit, out Vector3 HitPos, out Vector3 TraceNormal, bool ChangeScene = true, float spread = 0)
	{
        Vector3 directionToTarget = (Target.Transform.Position - Transform.Position).Normal;
		var ray = new Ray(Transform.Position + Vector3.Up * 55, directionToTarget);
		ray.Forward += Vector3.Random * spread;
        var tr = Scene.Trace.Ray(ray, TraceLength).WithoutTags("dummy").Run();
        if (tr.Hit)
        {
            tr.GameObject.Components.TryGet<PlayerController>(out var player, FindMode.EverythingInSelfAndParent);
            if (player is not null)
            {
                player.TakeDamage(Damage, ChangeScene);
				OnAttack?.Invoke(player.GameObject, TraceLength, Damage);
            }
        }
		HitPos = tr.EndPosition;
		TraceNormal = tr.Normal;
		TraceHit = tr.Hit;
}
	CancellationTokenSource CtsWander = new();
	public async Task Wander(NavMeshAgent navMeshAgent, GameObject body)
{
    while (true)
    {
        if (navMeshAgent is null) return;
		body.Transform.Rotation = Angles.Random.WithRoll(0).WithPitch(0);
        var tr = Scene.Trace.Ray(Transform.Position, Transform.Position + Transform.Rotation.Forward * 500).WithoutTags("enemy").Run();
        var targetPos = tr.HitPosition;
        while (Vector3.DistanceBetween(targetPos, Transform.Position) > 300)
        {
            if (navMeshAgent is not null)
            {
                navMeshAgent.MoveTo(targetPos);
            }
            if (CtsWander.Token.IsCancellationRequested)
            {
                return;
            }
            await GameTask.Delay(1);
        }

        await GameTask.DelaySeconds(5, CtsWander.Token);
    }
}

	protected override void OnFixedUpdate()
	{
		npcAction?.Invoke();
	}

	[ActionGraphNode("Clone GameObject This Pos and Rot"), Impure]
	public static void CloneGameObjectThisPosAndRot(GameObject gameObject, out GameObject clone)
	{
		clone = gameObject.Clone(gameObject.Transform.Position, gameObject.Transform.Rotation);
	}

	public void RagdollDeath(SkinnedModelRenderer Ragdoll)
	{
		if (Components.TryGet<NavMeshAgent>(out var agent, FindMode.EverythingInSelfAndAncestors & FindMode.EverythingInSelfAndParent))
		{
			agent.Stop();
			agent.Destroy();
		}
		if (Components.TryGet<NPC>(out var npc, FindMode.EverythingInSelfAndAncestors & FindMode.EverythingInSelfAndParent))
		{
			npc.Destroy();
		}
		if (Components.TryGet<ActionGraphDelay>(out var delay, FindMode.EverythingInSelfAndAncestors & FindMode.EverythingInSelfAndParent))
		{
			delay.Destroy();
		}
		var ragdollGb = Ragdoll.GameObject;
		var modelPhys = ragdollGb.Components.Create<ModelPhysics>();
		modelPhys.Renderer = Ragdoll;
		modelPhys.Model = Ragdoll.Model;
		if (Components.TryGet<EnemyHealthComponent>(out var health, FindMode.EverythingInSelfAndAncestors & FindMode.EverythingInSelfAndParent))
		{
			health.Destroy();
		}
	}
}
