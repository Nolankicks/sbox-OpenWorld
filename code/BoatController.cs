using Sandbox;

public sealed class BoatController : Component
{
	[Property] public CharacterController Controller { get; set; }
	[Property] public bool IsUsing { get; set; }
	public Vector3 WishVelo { get; set; }
	protected override void OnUpdate()
	{
		if (IsUsing)
		{
			Move();
		}
		Grav();
	}

	void Move()
	{
		if (Input.Down("forward"))
		{
			var cc = Controller;
			WishVelo = Scene.Camera.Transform.Rotation.Forward;
			WishVelo *= 500;
			WishVelo = WishVelo.WithZ(0);
			WishVelo.ClampLength(1);
			cc.Accelerate(WishVelo);
			cc.Move();
		}
		if (Input.Down("backward"))
		{
			var cc = Controller;
			WishVelo = Scene.Camera.Transform.Rotation.Backward;
			WishVelo *= 500;
			WishVelo = WishVelo.WithZ(0);
			WishVelo.ClampLength(1);
			cc.Accelerate(WishVelo);
			cc.Move();
		}
	}

	void Grav()
	{
		var cc = Controller;
		var halfGrav = Scene.PhysicsWorld.Gravity * Time.Delta;
		cc.Velocity += halfGrav;
	}
}
