using System.Diagnostics.CodeAnalysis;
using Sandbox;
using Sandbox.Citizen;

public sealed class Dummy : Component, Component.ITriggerListener
{
	[Sync] int health { get; set; } = 100;
	[Property] public NavMeshAgent navMeshAgent { get; set; }
	[Property] public List<GameObject> players { get; set; } = new();
	[Property] public CitizenAnimationHelper animationHelper { get; set; }
	[Property] public Model GibModel { get; set; }
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
		Log.Info(players.Count);
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
		animationHelper.AimAngle = player.Transform.Rotation;
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
		GameObject.Destroy();
		var gibGameObject = new GameObject();
		var prop = gibGameObject.Components.Create<Prop>().Model = GibModel;
		var clone = gibGameObject.Clone(GameObject.Transform.Position, GameObject.Transform.Rotation);
		var cloneGib = clone.Components.Get<Prop>();
		cloneGib.CreateGibs();
		clone.Destroy();
	}

}
