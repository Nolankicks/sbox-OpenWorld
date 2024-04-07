using System;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;

public sealed class Weapon : Component
{
	[Property] public int Damage { get; set; }
	[Property] public int Ammo { get; set; }
	[Property] public int MaxAmmo { get; set; }
	[Property, Range(0.1f, 1)] public float FireRate { get; set; }
	[Property] public GameObject testObject { get; set; }
	public PlayerController PlayerController { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelCamera { get; set; }
	[Property, Category("GameObjects")] public SkinnedModelRenderer ViewModelGun { get; set; }
	[Property, Category("GameObjects")] public GameObject DroppedItem { get; set; }
	[Property, Category("GameObjects")] public GameObject Arms { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelHolder { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; }
	[Property] public float ReloadTime { get; set; }
	[Property] public float Recoil { get; set; }
	public GameObject WorldModelInstance { get; set; }
	[Property] public float Spread { get; set; } = 0.03f;
	[Property] public GameObject Decal { get; set; }
	[Property] public GameObject ItemPrefab { get; set; }
	[Property] public string Name { get; set; }
	[Property, TextArea] public string Description { get; set; }
	int ShotsFired = 0;
	private TimeSince TimeSinceReload = 0;
	private TimeSince TimeSinceFire;
	[Property] public SoundEvent FireSound { get; set; }
	[Property] public GameObject MuzzleFlash { get; set; }
	[Property, Sync] public bool IsWeapon { get; set; }
	[Property] public GameObject Particle { get; set; }
	[Sync] public bool IsAiming { get; set; }
	public int StartingAmmo { get; set; }
	protected override void OnStart()
	{
		GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		Arms.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelGun.GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelCamera.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelHolder.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		if (IsWeapon)
		{
		if (IsProxy) return;
		DroppedItem.Enabled = false;
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		StartingAmmo = Ammo;
		TimeSinceFire = FireRate;
		if (!PlayerController.IsFirstPerson)
		{
			var worldModel = new GameObject();
			var modelRenderer = worldModel.Components.Create<ModelRenderer>();
			modelRenderer.Model = WorldModel;
			WorldModelInstance = worldModel;
			worldModel.Parent = PlayerController.Hold;
			worldModel.Transform.LocalPosition = new(4.653f, 0.688f, -4.365f);
		}
		else
		{
			ViewModelGun.Set("b_deploy", true);
		}
		TimeSinceReload = ReloadTime;
		TimeSinceFire = FireRate;
		Log.Info("Weapon started");
		}
		else
		{
			ViewModelCamera.Enabled = false;
			ViewModelGun.GameObject.Enabled = false;
			DroppedItem.Enabled = true;
		}
	}
	protected override void OnUpdate()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		if (IsWeapon)
		{
		DroppedItem.Enabled = false;
		ViewModelGun.GameObject.Enabled = true;
		if ( IsProxy ) return;
		
		if (Input.Down("attack1") && TimeSinceReload > 2.5)
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

		
		Log.Info(Ammo);
		UpdateWorldModelShadowType();
		if (PlayerController.IsFirstPerson)
		{
			if (Input.Pressed("reload") && MaxAmmo != 0 && ShotsFired != 0 && !IsProxy)
			{
				
				Ammo = MaxAmmo -= ShotsFired;
				Ammo = 30;
				ViewModelGun.Set("b_reload", true);
				ShotsFired = 0;
				TimeSinceReload = 0;
			}
			else
			{
				ViewModelGun.Set("b_reload", false);
			}
			if (Input.Pressed("jump") && !IsProxy)
			{
				ViewModelGun.Set("b_jump", true);
			}
			if (!PlayerController.CharacterController.IsOnGround && !IsProxy)
			{
				ViewModelGun.Set("b_grounded", false);
			}
			else
			{
				ViewModelGun.Set("b_grounded", true);
			}
			ViewModelGun.Set("move_groundspeed", PlayerController.CharacterController.Velocity.Length);
			ViewModelCamera.Enabled = IsProxy ? false : true;
		}
		else
		{
			ViewModelCamera.Enabled = false;
		}
		}
		else
		{
			ViewModelCamera.Enabled = false;
			ViewModelGun.GameObject.Enabled = false;
			DroppedItem.Enabled = true;
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
	[Broadcast]
	void GameObjectDestroy(GameObject obj)
	{
		obj.Destroy();
	}
	public float GetRandomFloat()
	{
		return Random.Shared.Float(-1, 1);
	}
	void Fire()
	{
		if (Ammo > 0 && TimeSinceFire > FireRate)
		{
			PlayerController.eyeAngles += new Angles(-Recoil, GetRandomFloat(), 0);
			Ammo--;
			ShotsFired++;
			var ray = Scene.Camera.ScreenNormalToRay(0.5f);
			ray.Forward += Vector3.Random * Spread;
			var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
			if (tr.Hit)
			{
				tr.GameObject.Components.TryGet<Dummy>( out var dummy);
				if (dummy is not null)
				{
					dummy.Hurt(Damage);
					Particle.Clone(tr.HitPosition, rotation: Rotation.LookAt(-tr.Normal));
				}
				Decal.Clone(tr.HitPosition + tr.Normal, Rotation.LookAt(-tr.Normal));
				if ( tr.Body is not null )
		{
			tr.Body.ApplyImpulseAt( tr.HitPosition, tr.Direction * 200.0f * tr.Body.Mass.Clamp( 0, 200 ) );
		}
		var damage = new DamageInfo(Damage, GameObject, GameObject, tr.Hitbox);
		damage.Position = tr.HitPosition;
		damage.Shape = tr.Shape;
		foreach (var damageAble in tr.GameObject.Components.GetAll<IDamageable>())
		{
			damageAble.OnDamage(damage);
		}
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

	}
}
