using Kicks;
using Sandbox;
[Icon("water_drop")]
public sealed class Water : Component, Component.ITriggerListener
{
   	Kicks.PlayerController PlayerController;
    [Property] List<Rigidbody> rigidbodies = new();

    protected override void OnFixedUpdate()
    {
        if (PlayerController is not null)
        {
			PlayerController.CharacterController.ApplyFriction(0.1f);
			PlayerController.CharacterController.Punch(Vector3.Up * 20);
			PlayerController.AnimationHelper.IsSwimming = true;
        }
		foreach (var rb in rigidbodies)
		{
		if (rb is not null)
		{
			rb.ApplyForce(Vector3.Up * 550000);
		}
		}
    }
void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
{
	if (other.GameObject.Tags.Has("watercollider"))
	{
	other.Components.TryGet<Kicks.PlayerController>(out var cc, FindMode.InParent);
    if (cc is not null)
    {
        PlayerController = cc;
    }
	Log.Info("Player entered water");
	}
	other.GameObject.Components.TryGet<Rigidbody>(out var rig);
	if (rig is not null)
	{
		rigidbodies.Add(rig);
	}
  
}
	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{
		if (other.GameObject.Tags.Has("watercollider"))
		{
		if (PlayerController is not null)
		{
			PlayerController.AnimationHelper.IsSwimming = false;
			PlayerController.CharacterController.Velocity.Clamp(Vector3.Zero, Vector3.One * 100);
			PlayerController = null;
		}
		Log.Info("Player exited water");
		}
		other.GameObject.Components.TryGet<Rigidbody>(out var rig);
		if (rig is not null)
		{
			rigidbodies.Remove(rig);
		}
	}

}
