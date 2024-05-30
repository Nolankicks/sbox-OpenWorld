using Kicks;
using Sandbox;

public sealed class Car : Component
{
	[Property] public bool IsDriving { get; set; } = false;
	[Property] public CharacterController CharacterController { get; set; }
	public Vector3 WishVelocity { get; set; }
	[Property] public float MoveSpeed { get; set; } = 1000;
	public PlayerController CurrentDriver { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	protected override void OnUpdate()
	{
		Rigidbody.Enabled = !IsDriving;
		if (IsDriving)
		{
			Drive();
			if (Input.Pressed("use"))
			{
				LeaveCar(CurrentDriver);
			}
		}
	}

	public void EnterCar(PlayerController playerController)
	{
		GameObject.Network.SetOwnerTransfer(OwnerTransfer.Takeover);
		GameObject.Network.TakeOwnership();
		playerController.AbleToMove = false;
		playerController.GameObject.Transform.LocalPosition = Vector3.Zero;
		GameObject.Transform.Rotation = new Angles(0, playerController.GameObject.Transform.Rotation.Yaw(), 0).ToRotation();
		playerController.GameObject.Parent = GameObject;
		CurrentDriver = playerController;
		IsDriving = true;
	}
	public void LeaveCar(PlayerController playerController)
	{
		playerController.AbleToMove = true;
		playerController.GameObject.Transform.Position = GameObject.Transform.Position + new Vector3(0, 0, 1);
		playerController.GameObject.Transform.Rotation = GameObject.Transform.Rotation;
		CurrentDriver = null;
		IsDriving = false;
		GameObject.Network.DropOwnership();
	}

	public void Drive()
	{
		if (Input.Pressed("use"))
		{
			LeaveCar(CurrentDriver);
		}
		var cc = CharacterController;
		WishVelocity = Input.AnalogMove.Normal.WithY(0);
		var halfGrav = Scene.PhysicsWorld.Gravity / 2;
		if (Input.Down("right"))
		{
			GameObject.Transform.Rotation *= Rotation.FromYaw(-1);
		}
		if (Input.Down("left"))
		{
			GameObject.Transform.Rotation *= Rotation.FromYaw(1);
		}
		if (!WishVelocity.IsNearlyZero())
		{
			WishVelocity = new Angles(0, GameObject.Transform.Rotation.Yaw(), 0).ToRotation() * WishVelocity;
			WishVelocity = WishVelocity.WithZ(0);
			WishVelocity.ClampLength(1);
			WishVelocity *= MoveSpeed;
			if (!cc.IsOnGround)
			{
				WishVelocity.ClampLength(50);
			}
		}

		if (cc.IsOnGround)
		{
			cc.Accelerate(WishVelocity);
			cc.Velocity = CharacterController.Velocity.WithZ(0);
		}
		else
		{
			cc.Velocity += halfGrav;
			cc.Accelerate(WishVelocity);
		}
		cc.ApplyFriction(5);
		cc.Move();
		if (!cc.IsOnGround)
		{
			cc.Velocity += halfGrav;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ(0);
		}
	}
}
