using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerController : Component
{
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public int WalkSpeed { get; set; } = 100;
	[Property] public int RunSpeed { get; set; } = 200;
	[Property] public int CrouchSpeed { get; set; } = 50;
	[Sync] public Vector3 WishVelocity { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	public bool IsCrouching { get; set; }
	[Sync] Angles eyeAngles { get; set; }
	private void MouseInput()
	{
		var e = eyeAngles;
		e += Input.AnalogLook;
		e.pitch = e.pitch.Clamp( -90, 90 );
		e.roll = 0.0f;
		eyeAngles = e;
	}
	protected override void OnUpdate()
	{
		if (!IsProxy)
		{
			MouseInput();
			Movement();
			Crouch();
			CamPos();
			UpdateAnimation();
			UpdateBodyShit();
		}
	}
	float MoveSpeed
	{
		get
		{
			if (IsCrouching)
			{
				return CrouchSpeed;
			}
			if (Input.Down("run"))
			{
				return RunSpeed;
			}
			else
			{
				return WalkSpeed;
			}
		}
	}
	float Friction
	{
		get
		{
			if (CharacterController.IsOnGround)
			{
				return 6.0f;
			}
			else
			{
				return 0.2f;
			}
		}
	}
	RealTimeSince timeSinceJump = 0;
	RealTimeSince timeSinceGround = 0;
	void Movement()
	{
		if (CharacterController is null)
		{
			return;
		}
		WishVelocity = Input.AnalogMove;
		Vector3 halfGrav = Scene.PhysicsWorld.Gravity * 0.5f;
		if (Input.Down("jump"))
		{
			CharacterController.Punch(Vector3.Up * 200);
			timeSinceJump = 0;
		}

	if (!WishVelocity.IsNearlyZero())
	{
		WishVelocity = new Angles(0, eyeAngles.yaw, 0).ToRotation() * WishVelocity;
		WishVelocity = WishVelocity.WithZ(0);
		WishVelocity.ClampLength(1);
		WishVelocity *= MoveSpeed;
		if (!CharacterController.IsOnGround)
		{
			WishVelocity.ClampLength(50);
		}
	}
	CharacterController.ApplyFriction(Friction);

	if (CharacterController.IsOnGround)
	{
		CharacterController.Accelerate(WishVelocity);
		CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
	}
	else
	{
		CharacterController.Velocity += halfGrav;
		CharacterController.Accelerate(WishVelocity);
	}
	CharacterController.Move();
	if (!CharacterController.IsOnGround)
	{
		CharacterController.Velocity += halfGrav;
	}
	else
	{
		CharacterController.Velocity = CharacterController.Velocity.WithZ(0);
	}
	if (!CharacterController.IsOnGround)
	{
		timeSinceGround = 0;
	}
	}
	bool UnCrouch()
	{
		if (!IsCrouching)
		{
			return true;
		}
		var tr = CharacterController.TraceDirection(Vector3.Up * 32);
		return !tr.Hit;
	}
	void CamPos()
	{
		var camera = Scene.GetAllComponents<CameraComponent>().Where(x => x.IsMainCamera).FirstOrDefault();
		if (camera is null) return;

		var targetPosEyePos = IsCrouching ? 32 : 64;
		var targetPos = Transform.Position + new Vector3(0, 0, targetPosEyePos);
		camera.Transform.Position = targetPos;
		camera.Transform.Rotation = eyeAngles;

	}
	void Crouch()
	{
		if (!Input.Down("duck"))
		{
			if (!UnCrouch()) return;
			CharacterController.Height = 64;
			IsCrouching = false;
		}
		else
		{
			CharacterController.Height = 32;
			IsCrouching = true;
		}
	}
	private void UpdateAnimation()
	{
		if ( AnimationHelper is null ) return;

		var wv = WishVelocity.Length;

		AnimationHelper.WithWishVelocity( WishVelocity );
		AnimationHelper.WithVelocity( CharacterController.Velocity );
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.DuckLevel = IsCrouching ? 1.0f : 0.0f;

		AnimationHelper.MoveStyle = wv < 160f ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;

		var lookDir = eyeAngles.ToRotation().Forward * 1024;
		AnimationHelper.WithLook( lookDir, 1, 0.5f, 0.25f );
	}
	public void UpdateBodyShit()
	{
		var target = AnimationHelper.Target;
		if (IsProxy)
		{
			target.RenderType = ModelRenderer.ShadowRenderType.On;
		}
		else
		{
			target.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
	}

}
