using Sandbox;

public sealed class WaterTrigger : Component, Component.ITriggerListener
{
	protected override void OnUpdate()
	{

	}

	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		Log.Info(other.GameObject);
		if (other.Tags.Has("player"))
		{
			Log.Info("Player entered water");
			Scene.PhysicsWorld.Gravity = new Vector3(0, 0, -25);
		}
	}

	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{

	}
}
