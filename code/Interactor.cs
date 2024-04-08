using Sandbox;

public sealed class Interactor : Component
{
	PhysicsBody grabbedBody;
	Transform grabbedOffset;
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
			}
		}
		if (grabbedBody is null) return;
		if (Input.Down("flashlight") && grabbedBody is not null)
		{
				var targetTx = aimTransform.ToWorld( grabbedOffset );
				grabbedBody.SmoothMove( targetTx, Time.Delta * 50, Time.Delta );
				return;
		}
		else
		{
			grabbedBody = null;
		}
	}

}
