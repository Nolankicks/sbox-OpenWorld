using System.Diagnostics.CodeAnalysis;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.VR;
namespace Kicks;
public sealed class Dummy : Component, Component.ITriggerListener
{
	[Sync] int health { get; set; } = 100;
	[Property] public NavMeshAgent navMeshAgent { get; set; }
	[Property] public List<GameObject> players { get; set; } = new();
	[Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Property] public GameObject GibGameObject { get; set; }
	[Property] public bool SpawnGibs { get; set; }
	[Property, ShowIf("SpawnGibs", false)] public GameObject Ragdoll { get; set; }	
	public delegate void OnDeath( PlayerController attacker);
	[Property] public OnDeath OnDeathAction { get; set; }
	[Property] public GameObject ItemDrop { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }
	[Sync] public Vector3 Velocity { get; set; }
	public PlayerController player;
	protected override void OnStart()
	{
		player = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
	}
	protected override void OnUpdate()
	{
		WishVelocity = navMeshAgent.WishVelocity;
		Velocity = navMeshAgent.Velocity;
		UpdateAnimations();
		if (!IsProxy)
		{
			MoveToPlayer();
		}
	}
	void MoveToPlayer()
	{
		if (players.Count == 0)
		{
			navMeshAgent.Stop();
			animationHelper.LookAt = null;
		}
		else
		{
		var player = Game.Random.FromList(players);
		var targetRot = Rotation.LookAt(player.Transform.Position.WithZ(Transform.Position.z) - Transform.Position, Vector3.Up);
		animationHelper.Target.Transform.Rotation = Rotation.Slerp(animationHelper.Target.Transform.Rotation, targetRot, Time.Delta * 10f);
		navMeshAgent.MoveTo(player.Transform.Position);
		animationHelper.LookAt = player;
		}
	}
	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		if (other.GameObject.Tags.Has("player"))
		{
			players.Add(other.GameObject);
		}
	}

	void UpdateAnimations()
	{
		animationHelper.WithVelocity(Velocity);
		animationHelper.WithWishVelocity(WishVelocity);
	}

	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{
		if (other.GameObject.Tags.Has("player"))
		{
			players.Remove(other.GameObject);
		}
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
			OnDeathAction?.Invoke(playerController);
		}
	}

	public void Kill()
	{
		if (SpawnGibs)
		{
			var clone = GibGameObject.Clone(GameObject.Transform.World);
			var prop = clone.Components.Get<Prop>();
			prop.Kill();
			if (ItemDrop is not null)
			{
				var item = ItemDrop.Clone(GameObject.Transform.Position + Vector3.Up * 50f);
				item.NetworkSpawn(null);
			}
		}
		else
		{
			var ragdoll = Ragdoll.Clone(GameObject.Transform.World);
			ragdoll.NetworkSpawn(null);
			if (ItemDrop is not null)
			{
				var item = ItemDrop.Clone(GameObject.Transform.Position + Vector3.Up * 50f);
				item.NetworkSpawn(null);
			}
		}
		GameObject.Destroy();
	}

}
