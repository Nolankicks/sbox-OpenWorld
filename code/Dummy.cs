using System.Diagnostics.CodeAnalysis;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.VR;

public sealed class Dummy : Component, Component.ITriggerListener
{
	[Sync] int health { get; set; } = 100;
	[Property] public NavMeshAgent navMeshAgent { get; set; }
	[Property] public List<GameObject> players { get; set; } = new();
	[Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Property] public GameObject GibGameObject { get; set; }
	[Property] public bool SpawnGibs { get; set; }
	[Property, ShowIf("SpawnGibs", false)] public GameObject Ragdoll { get; set; }	
	public PlayerController player;
	protected override void OnStart()
	{
		player = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
	}
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		MoveToPlayer();
		UpdateAnimations();
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
		animationHelper.WithVelocity(navMeshAgent.Velocity);
		animationHelper.WithWishVelocity(navMeshAgent.WishVelocity);
	}

	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{
		if (other.GameObject.Tags.Has("player"))
		{
			players.Remove(other.GameObject);
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
		if (SpawnGibs)
		{
			var clone = GibGameObject.Clone(GameObject.Transform.World);
			var prop = clone.Components.Get<Prop>();
			prop.CreateGibs();
			clone.Destroy();
			GameObject.Destroy();
		}
		else
		{
			var ragdoll = Ragdoll.Clone(GameObject.Transform.World);
			ragdoll.NetworkSpawn(null);
			GameObject.Destroy();
		}
	}

}
