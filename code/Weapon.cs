using Sandbox;
using Sandbox.Citizen;

public sealed class Weapon : Component
{
	[Property] public int Damage { get; set; }
	[Property] public int Ammo { get; set; }
	[Property] public int MaxAmmo { get; set; }
	[Property, Range(0.1f, 1)] public float FireRate { get; set; }
	[Property] public GameObject testObject { get; set; }
	public PlayerController PlayerController { get; set; }
	[Property] public GameObject ViewModelCamera { get; set; }
	[Property] public SkinnedModelRenderer ViewModelGun { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; }
	[Property] public float ReloadTime { get; set; }
	public GameObject WorldModelInstance { get; set; }
	[Property] public GameObject Decal { get; set; }
	int ShotsFired = 0;
	public TimeSince TimeSinceReload { get; set; }
	public TimeSince TimeSinceFire { get; set; }
	[Property] public SoundEvent FireSound { get; set; }
	[Property] public GameObject MuzzleFlash { get; set; }
	[Property] public Texture ItemTexture { get; set; }
	[Property] public bool IsAiming { get; set; }
	protected override void OnStart()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		if (IsProxy) return;
		if (!PlayerController.IsFirstPerson)
		{
			var worldModel = new GameObject();
			var modelRenderer = worldModel.Components.Create<ModelRenderer>();
			modelRenderer.Model = WorldModel;
			WorldModelInstance = worldModel;
			worldModel.Parent = PlayerController.Hold;
			worldModel.Transform.LocalPosition = new(4.653f, 0.688f, -4.365f);
		}
		TimeSinceReload = ReloadTime;
		TimeSinceFire = FireRate;
	}
	protected override void OnUpdate()
	{
		if (Input.Down("attack1") && TimeSinceReload > 2.5f)
		{
			Fire();
		}
		if (Input.Down("attack2"))
		{
			IsAiming = true;
		}
		else
		{
			IsAiming = false;
		}
		ViewModelGun.Set( "ironsights", IsAiming ? 2 : 0 );
		ViewModelGun.Set( "ironsights_fire_scale", IsAiming ? 0.3f : 0f );

		Reload();
		Log.Info(Ammo);
		UpdateWorldModelShadowType();
		if (PlayerController.IsFirstPerson)
		{
			ViewModelCamera.Enabled = true;
			ViewModelGun.Set("move_groundspeed", PlayerController.CharacterController.Velocity.Length);
			if (PlayerController.CharacterController.IsOnGround)
			{
				ViewModelGun.Set("b_grounded", true);
			}
			else
			{
				ViewModelGun.Set("b_grounded", false);
			}
			if (Input.Pressed("jump"))
			{
				ViewModelGun.Set("b_jump", true);
			}
			else
			{
				ViewModelGun.Set("b_jump", false);
			}
			ViewModelGun.Set("b_twohanded", true);
		}
		else
		{
			ViewModelCamera.Enabled = false;
		}
	}

	void UpdateWorldModelShadowType()
	{
		if (WorldModelInstance is null) return;
		WorldModelInstance.Components.TryGet<ModelRenderer>( out var modelRenderer );
		if (modelRenderer is not null)
		{
		var ShadowType = PlayerController.IsFirstPerson ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
		modelRenderer.RenderType = ShadowType;
		}
	}

	void Fire()
	{
		if (Ammo > 0 && TimeSinceFire > FireRate)
		{
			Ammo--;
			ShotsFired++;
			var ray = Scene.Camera.ScreenNormalToRay(0.5f);
			var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
			if (tr.Hit)
			{
				Decal.Clone(tr.HitPosition + tr.Normal, Rotation.LookAt(-tr.Normal));
			}	
		
			TimeSinceFire = 0;
			PlayerController.AnimationHelper.Target.Set("b_attack", true);
			if (PlayerController.IsFirstPerson)
			{
				ViewModelGun.Set("b_attack", true);
			}
			Sound.Play(FireSound, tr.StartPosition);
			var muzzle = ViewModelGun.GetAttachment("muzzle");
			var MuzzleFlashInstance = MuzzleFlash.Clone(muzzle.Value.Position, muzzle.Value.Rotation);
			MuzzleFlashInstance.Tags.Add("viewmodel");
		}	
}
	void Reload()
	{
		if (Input.Pressed("reload") && MaxAmmo != 0 && ShotsFired != 0 && !IsProxy)
			{
				Ammo = MaxAmmo -= ShotsFired;
				Ammo = 30;
				ViewModelGun.Set("b_reload", true);
				ShotsFired = 0;
				TimeSinceReload = 0;
			}
	}
}
