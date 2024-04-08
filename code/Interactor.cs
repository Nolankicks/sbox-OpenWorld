using Sandbox;

public sealed class Interactor : Component
{
	PhysicsBody grabbedBody;
	Transform grabbedOffset;
	GameObject PhysicsGameObject;
	public bool IsGrabbing => grabbedBody is not null;
	public PlayerController PlayerController;

	protected override void OnStart()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
	}
	protected override void OnUpdate()
	{
		PlayerController.IsGrabbing = IsGrabbing;
		Transform aimTransform = Scene.Camera.Transform.World;
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 300).WithoutTags("player").Run();
		if (Input.Pressed("flashlight"))
		{
			if (tr.Hit && tr.Body is not null && tr.GameObject.Components.Get<Weapon>() is null || tr.GameObject.Components.Get<Shotgun>() is null)
			{
				grabbedBody = tr.Body;
				grabbedOffset = aimTransform.ToLocal( tr.Body.Transform );
				PhysicsGameObject = tr.GameObject;
				PhysicsGameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
				PhysicsGameObject.Network.TakeOwnership();
			}
		}
		if (grabbedBody is null) return;
		if (Input.Down("flashlight") && grabbedBody is not null)
		{
			if (grabbedBody is null) return;
				var targetTx = aimTransform.ToWorld( grabbedOffset );
				grabbedBody.SmoothMove( targetTx, Time.Delta * 50, Time.Delta );
				PhysicsGameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
				PhysicsGameObject.Network.TakeOwnership();
				return;
		}
		else
		{
			PhysicsGameObject.Network.DropOwnership();
			grabbedBody = null;
			PhysicsGameObject = null;
		}
	}

}
