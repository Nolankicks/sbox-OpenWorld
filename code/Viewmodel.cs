using Sandbox;
using Kicks;
public sealed class ViewModel : Component
{
	public PlayerController Player { get; set; }
	[Property] public SkinnedModelRenderer Gun { get; set; }
	float lastPitch;
	float lastYaw;
	float yawInertia;
	float pitchInertia;
	bool UseInteria = false;
	protected override void OnStart()
	{
		Player = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
	}

	void ApplyInertia()
	{
		var camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault( x => !x.IsProxy );
		var cameraRot = camera.Transform.Rotation;

		if ( !UseInteria )
		{
			lastPitch = cameraRot.Pitch();
			lastYaw = cameraRot.Yaw();
			yawInertia = 0;
			pitchInertia = 0;
			UseInteria = true;
		}
		var pitch = cameraRot.Pitch();
		var yaw = cameraRot.Yaw();
		pitchInertia = Angles.NormalizeAngle( pitch - lastPitch );
		yawInertia = Angles.NormalizeAngle( yaw - lastYaw );
		lastPitch = pitch;
		lastYaw = yaw;
		Gun.Set( "aim_yaw_inertia", yawInertia * 2 );
		Gun.Set( "aim_pitch_inertia", pitchInertia * 2 );
	}

	void GroundSpeed()
	{
		Gun.Set( "move_groundspeed", Player.GameObject.Components.Get<CharacterController>().Velocity.Length );
	}
	Vector3 LocalPos = Vector3.Zero;
	protected override void OnUpdate()
	{
		GameObject.Enabled = !GameObject.Parent.IsProxy;
		if ( GameObject.Parent.IsProxy ) return;
		if ( Components.TryGet<Weapon>( out var weapon, FindMode.EverythingInSelfAndParent ) )
		{
			if ( weapon.IsWeapon )
			{
				GameObject.Parent.Parent = Player.Eye;
			}
			else
			{
				GameObject.Parent.Parent = null;
			}
		}
		else if ( Components.TryGet<ActionGraphItem>( out var actionGraphItem, FindMode.EverythingInSelfAndParent ) )
		{
			if ( actionGraphItem.InInventory )
			{
				GameObject.Parent.Parent = Player.Eye;
			}
			else
			{
				GameObject.Parent.Parent = null;
			}
		}
		LocalPos = LocalPos.LerpTo( LocalPos, Time.Delta * 10f );
		if ( Player.AbleToMove )
		{
			ApplyInertia();
			GroundSpeed();
		}
	}

	protected override void OnEnabled()
	{
		if ( GameObject.Parent.IsProxy || !Gun.IsValid() ) return;
		Gun.Set( "b_deploy_dry", true );
	}
}
