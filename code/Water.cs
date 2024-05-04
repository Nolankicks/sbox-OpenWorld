using Kicks;
using Sandbox;

public sealed class Water : Component, Component.ITriggerListener
{
   	PlayerController PlayerController;
    private Rigidbody rb;

    protected override void OnFixedUpdate()
    {
        if (PlayerController is not null)
        {
			PlayerController.CharacterController.ApplyFriction(0.1f);
			PlayerController.CharacterController.Punch(Vector3.Up * 20);
			PlayerController.AnimationHelper.IsSwimming = true;
        }
    }
void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
{
    if (!other.GameObject.Tags.Has("watercollider")) return;
    other.Components.TryGet<PlayerController>(out var cc, FindMode.InParent);
    if (cc is not null)
    {
        PlayerController = cc;
    }
	Log.Info("Player entered water");
}
	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{
		if (!other.GameObject.Tags.Has("watercollider")) return;
		if (PlayerController is not null)
		{
			PlayerController.AnimationHelper.IsSwimming = false;
			PlayerController.CharacterController.Velocity.Clamp(Vector3.Zero, Vector3.One * 100);
			PlayerController = null;
		}
		Log.Info("Player exited water");
	}

}
