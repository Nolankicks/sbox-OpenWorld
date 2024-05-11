using Sandbox;
namespace Kicks;
public sealed class Interactor : Component
{
	PhysicsBody grabbedBody;
	Transform grabbedOffset;
	[Property] GameObject PhysicsGameObject;
	public bool IsGrabbing => grabbedBody is not null;
	public PlayerController PlayerController;

	protected override void OnStart()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
	}
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		PlayerController.IsGrabbing = IsGrabbing;
		Transform aimTransform = Scene.Camera.Transform.World;
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 500).WithoutTags("player").Run();
		if (Input.Pressed("flashlight") && tr.Body is not null)
		{
			if (tr.Hit && tr.Body is not null && tr.GameObject.Components.Get<Weapon>() is null || tr.GameObject.Components.Get<Shotgun>() is null)
			{
				grabbedBody = tr.Body;
				grabbedOffset = aimTransform.ToLocal( tr.Body.Transform );
				PhysicsGameObject = tr.GameObject;
				PhysicsGameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
				PhysicsGameObject.Network.TakeOwnership();
			}
			else
			{
				return;
			}
		}
		if (PhysicsGameObject is null) return;
		if (grabbedBody is null) return;
		if (Input.Down("flashlight") && grabbedBody is not null && PhysicsGameObject is not null)
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
			if (grabbedBody is null) return;
			PhysicsGameObject.Network.DropOwnership();
			grabbedBody = null;
			PhysicsGameObject = null;
		}
	}
}
