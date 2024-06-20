using System.IO;
using System.Runtime.CompilerServices;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Engine;
using Sandbox.Utility;
namespace Kicks;
public sealed class PlayerController : Component
{
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public int WalkSpeed { get; set; } = 100;
	[Property] public int RunSpeed { get; set; } = 200;
	[Property] public int CrouchSpeed { get; set; } = 50;
	[Sync] public Vector3 WishVelocity { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	public delegate void OnDeathDel(PlayerController playerController, Inventory inventory, AmmoContainer ammoContainer);
	[Property] public OnDeathDel OnDeath { get; set; }
	[Property, Sync] public int Wood { get; set; }
	[Property, Sync] public int Stone { get; set; }
	[Property, Sync] public int Metal { get; set; }
	[Property] public GameObject Hold { get; set; }
	[Property] public GameObject Eye { get; set; }
	[Sync] public bool IsFirstPerson { get; set; } = true;
	[Sync] public bool IsCrouching { get; set; }
	[Property, Sync] public Angles eyeAngles { get; set; }
	[Sync] public bool IsGrabbing { get; set; } = false;
	[Property] public Interactor Interactor { get; set; }
	[Property, Sync] public float Health { get; set; } = 100;
	[Property, Sync] public bool ShowShopUi { get; set; } = false;
	[Property] public Action OnJump { get; set; }
	[Property, Sync] public bool AbleToMove { get; set; } = true;
	[Property] public SceneFile SceneFile { get; set; }
	[Property] public PopupUi PopupUi { get; set; }
	[Property] public int Coins { get; set; }
	[Property] public bool FindSpawnPoint { get; set; } = false;
	[Property] public bool MoveCamera { get; set; } = true;
	public CameraComponent Camera { get; set; }
	public AmmoContainer AmmoContainer;
	public Item CurrentItem;
	public Inventory Inventory;
	protected override void OnEnabled()
	{
		AnimationHelper.Target.OnFootstepEvent += OnFootStep;
		OnJump += PlayJumpSound;
	}

	protected override void OnDisabled()
	{
		AnimationHelper.Target.OnFootstepEvent -= OnFootStep;
		OnJump -= PlayJumpSound;
	}
	protected override void OnStart()
	{
		Camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault(x => x.IsMainCamera);
		eyeAngles = Transform.Rotation.Angles();
		var steamId = Steam.SteamId.ToString();
		GameObject.Tags.Add(steamId);
		CharacterController.IgnoreLayers.Add(steamId);
		AmmoContainer = Scene.GetAllComponents<AmmoContainer>().FirstOrDefault(x => !x.IsProxy);

		if (FindSpawnPoint)
		{
			var spawnPoint = Game.Random.FromList(Scene.GetAllComponents<SpawnPoint>().ToList());
			Transform.World = spawnPoint.Transform.World;
			eyeAngles = spawnPoint.Transform.Rotation.Angles();
		}
	}
	void PlayJumpSound()
	{
		var tr = Scene.Trace.Ray(Transform.Position + Vector3.Up * 20, Transform.Position + Vector3.Up * -20).Run();
		if (!tr.Hit) return;
		if (tr.Surface is null) return;
		var sound = tr.Surface.Sounds.FootLaunch;
		var soundeevent = Sound.Play(sound);
		soundeevent.Volume = 0.5f;
	}
	private void MouseInput()
	{
		var e = eyeAngles;
		e += Input.AnalogLook;
		e.pitch = e.pitch.Clamp(-85, 90);
		e.roll = 0.0f;
		eyeAngles = e;
	}
	protected override void OnUpdate()
	{
		if (!IsProxy && MoveCamera)
		{
			MouseInput();
			CamPos();
			if (!IsProxy)
			{
				var eyePos = Eye.Transform.Position;
				eyePos = AnimationHelper.Target.Transform.Position + Vector3.Up * (IsCrouching ? 32 : 64);
				Eye.Transform.Position = eyePos;
				Eye.Transform.Rotation = eyeAngles.ToRotation();
			}
		}
	}
	protected override void OnFixedUpdate()
	{
		UpdateAnimation();
		if (!IsProxy && AbleToMove)
		{

			Movement();
			Crouch();
			Transform.Rotation = Rotation.Slerp(Transform.Rotation, new Angles(0, eyeAngles.yaw, 0).ToRotation(), Time.Delta * 5);

		}

	}


	public void AddCoins(int amount)
	{
		Coins += amount;
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
	float Friction()
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
	TimeSince timeSinceFootstep;
	void OnFootStep(SceneModel.FootstepEvent footstepEvent)
	{
		if (timeSinceFootstep > 0.2f) return;
		var tr = Scene.Trace.Ray(footstepEvent.Transform.Position + Vector3.Up * 20, footstepEvent.Transform.Position + Vector3.Up * -20).Run();
		if (!tr.Hit) return;
		if (tr.Surface is null) return;
		timeSinceFootstep = 0;
		var sound = footstepEvent.FootId == 0 ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;
		var soundeevent = Sound.Play(sound);
		soundeevent.Volume = footstepEvent.Volume;
		timeSinceGround = 0;
	}
	RealTimeSince timeSinceJump = 0;
	RealTimeSince timeSinceGround = 1;
	void Movement()
	{
		var cc = CharacterController;
		if (cc is null)
		{
			return;
		}
		WishVelocity = Input.AnalogMove.Normal;
		Vector3 halfGrav = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;
		if (Input.Down("jump") && cc.IsOnGround && timeSinceJump > 0.1f)
		{
			CharacterController.Punch(Vector3.Up * 300);
			OnJump?.Invoke();
			AnimationHelper.TriggerJump();
			timeSinceJump = 0;
		}

		if (!WishVelocity.IsNearlyZero())
		{
			WishVelocity = new Angles(0, eyeAngles.yaw, 0).ToRotation() * WishVelocity;
			WishVelocity = WishVelocity.WithZ(0);
			WishVelocity.ClampLength(1);
			WishVelocity *= MoveSpeed;
			if (!cc.IsOnGround)
			{
				WishVelocity.ClampLength(50);
			}
		}
		cc.ApplyFriction(Friction());

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
		CharacterController.Move();
		if (!cc.IsOnGround)
		{
			cc.Velocity += halfGrav;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ(0);
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
		if (IsFirstPerson)
		{
			camera.FieldOfView = Preferences.FieldOfView;
			var targetPosEyePos = IsCrouching ? 32 : 64;
			var targetPos = Transform.Position + new Vector3(0, 0, targetPosEyePos);
			camera.Transform.Position = targetPos;
			camera.Transform.Rotation = eyeAngles;
		}
		else
		{
			var lookDir = eyeAngles.ToRotation();
			//Set the camera rotation
			camera.Transform.Rotation = lookDir;
			var center = Transform.Position + Vector3.Up * 64;
			//Trace to see if the camera is inside a wall
			var tr = Scene.Trace.Ray(center, center - (eyeAngles.Forward * 300)).WithoutTags("player", "barrier").Run();
			if (tr.Hit)
			{
				camera.Transform.Position = tr.EndPosition + tr.Normal * 2 + Vector3.Up * 10;
			}
			else
			{
				camera.Transform.Position = center - (eyeAngles.Forward * 300) + Vector3.Up * 10;
			}
		}
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
	protected override void OnPreRender()
	{
		UpdateBodyShit();

		if (IsProxy)
			return;


	}
	private void UpdateAnimation()
	{
		if (AnimationHelper is null) return;

		var wv = WishVelocity.Length;

		AnimationHelper.WithWishVelocity(WishVelocity);
		AnimationHelper.WithVelocity(CharacterController.Velocity);
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.DuckLevel = IsCrouching ? 1.0f : 0.0f;

		AnimationHelper.MoveStyle = wv < 160f ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;

		var lookDir = eyeAngles.ToRotation().Forward * 1024;
		AnimationHelper.WithLook(lookDir, 1, 0.5f, 0.25f);
	}
	public void UpdateBodyShit()
	{
		var target = AnimationHelper.Target;
		var cloths = target.Components.GetAll<ModelRenderer>(FindMode.InChildren);
		if (IsProxy || !IsFirstPerson)
		{
			target.RenderType = ModelRenderer.ShadowRenderType.On;
			foreach (var cloth in cloths)
			{
				cloth.RenderType = ModelRenderer.ShadowRenderType.On;
			}
		}
		else
		{
			target.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
			foreach (var cloth in cloths)
			{
				cloth.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
			}
		}
	}

	[Broadcast]
	public void TakeDamage(float damage, bool ChangeScene = true)
	{
		if (IsProxy) return;
		Health -= damage;
		if (Health <= 0)
		{
			if (ChangeScene)
			{
				Kill();
			}
			else
			{
				Kill(true);
			}
		}
	}

	[Broadcast]
	public void Heal(float amount)
	{
		if (IsProxy) return;
		if (Health < 120)
		{
			Health += amount;
		}
	}

	[ActionGraphNode("Take Damage Node"), Pure]
	public void TakeDamageNode(float damage)
	{
		TakeDamage(damage);
	}

	[ActionGraphNode("Heal Node"), Pure]
	public void HealNode(float amount)
	{
		Heal(amount);
	}
	void Kill(bool Respawn = false)
	{
		if (!IsProxy)
		{
			OnDeath?.Invoke(this, Inventory, AmmoContainer);
			if (Respawn)
			{
				var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToList();
				var selectedPoint = Game.Random.FromList(spawnPoints);
				Transform.World = selectedPoint.Transform.World;
				Health = 100;
				AmmoContainer.ResetAmmo();
			}
			else
			{
				Game.ActiveScene.Load(SceneFile);
			}
		}
	}
}
