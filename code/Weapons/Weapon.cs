using System;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using Sandbox.Utility;
namespace Kicks;
public sealed class Weapon : Component
{
	[Property] public string ShootBool { get; set; } = "b_attack";
	[Property] public string ReloadBool { get; set; } = "b_reload";
	[Property, Category("Weapon Properties")] public int Damage { get; set; }
	[Property] public string WeaponIndent { get; set; } 
	[Property, Category("Weapon Properties")] public int Ammo { get; set; }
	[Property, Category("Weapon Properties")] public int MaxAmmo { get; set; }
	[Property, Range(0.1f, 1), Category("Weapon Properties")] public float FireRate { get; set; }
	public PlayerController PlayerController { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelCamera { get; set; }
	[Property, Category("GameObjects")] public SkinnedModelRenderer ViewModelGun { get; set; }
	[Property, Category("GameObjects")] public GameObject DroppedItem { get; set; }
	[Property, Category("GameObjects")] public GameObject Arms { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelHolder { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; }
	[Property] public SkinnedModelRenderer armsRenderer { get; set; }
	[Property, Category("Weapon Properties")] public float ReloadTime { get; set; }
	[Property, Category("Weapon Properties")] public float Recoil { get; set; }
	[Property] public TestStruct TestStruct { get; set; }
	public GameObject WorldModelInstance { get; set; }

	[Property, Category("Weapon Properties")] public float Spread { get; set; } = 0.03f;
	[Property, Category("Prefabs")] public GameObject Decal { get; set; }
	/// <summary>
	/// The description of the weapon that will be shown in the item popup
	/// </summary>

	[Property] public string Name { get; set; }
	/// <summary>
	/// The description of the weapon that will be shown in the item popup
	/// </summary>

	[Property, TextArea] public string Description { get; set; }
	int ShotsFired = 0;
	private TimeSince TimeSinceReload = 0;
	private TimeSince TimeSinceFire;
	/// <summary>
	/// The sound that will play when the weapon is fired, if left empty it will not play a sound
	/// </summary>
	[Property, Category("Weapon Properties")] public SoundEvent FireSound { get; set; }
	/// <summary>
	/// Use this if you want to have a muzzle flash, if left empty it will not spawn a muzzle flash
	/// </summary>
	[Property, Category("Weapon Properties")] public bool b_twohanded { get; set; }
	[Property, Category("Weapon Properties")] public AmmoContainer.AmmoTypes AmmoType { get; set; }
	[Property, Category("Prefabs")] public GameObject MuzzleFlash { get; set; }
	[Property, Sync] public bool IsWeapon { get; set; }
	[Property, Category("Prefabs")] public GameObject BloodParticle { get; set; }
	[Property, Category("Prefabs")] public GameObject ImpactParticle { get; set; }
	[Sync] public bool IsAiming { get; set; }
	[Property] public int StartingAmmo { get; set; }
	public AmmoContainer AmmoContainer { get; set; }
	protected override void OnStart()
	{
		GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		Arms.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelGun.GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelCamera.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelHolder.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		TimeSinceReload = ReloadTime;
		TimeSinceFire = FireRate;
		StartingAmmo = Ammo;
	}
	protected override void OnUpdate()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		AmmoContainer = Scene.GetAllComponents<AmmoContainer>().FirstOrDefault( x => !x.IsProxy);
		if (IsProxy)
		{
			ViewModelCamera.Enabled = false;
		}
		if (IsWeapon)
		{
		DroppedItem.Enabled = false;
		if ( IsProxy ) return;
		ViewModelCamera.Enabled = true;
		Arms.Enabled = true;
		ViewModelGun.GameObject.Enabled = true;
		ViewModelGun.Set("b_twohanded", true);
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
		ViewModelGun.Set( "ironsights_fire_scale", IsAiming ? 0.2f : 0f );

		
		UpdateWorldModelShadowType();
		if (PlayerController.IsFirstPerson)
		{
			var selelctedAmmo = AmmoContainer.GetAmmo(AmmoType);
			if (Input.Pressed("reload") && MaxAmmo != 0 && ShotsFired != 0 && !IsProxy && selelctedAmmo > 0)
			{
				var ammoToSet = selelctedAmmo - ShotsFired;
				if (ammoToSet > selelctedAmmo)
				{
					Ammo = selelctedAmmo;
					AmmoContainer.SetAmmo(AmmoType, 0);
				}
				else
				{
					AmmoContainer.SetAmmo(AmmoType, ammoToSet);
					Ammo = StartingAmmo;
				}
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
			if (Ammo == 0)
			{
				ViewModelGun.Set("b_empty", true);
			}
			else
			{
				ViewModelGun.Set("b_empty", false);
			}
			if (Input.Pressed("attack1") && Ammo <= 0)
			{
				ViewModelGun.Set("b_attack_dry", true);
			}
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
			Arms.Enabled = false;
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
		if (PlayerController.IsGrabbing) return;
		
		if (Ammo > 0 && TimeSinceFire > FireRate)
		{
			PlayerController.eyeAngles += new Angles(-Recoil, GetRandomFloat(), 0);
			Ammo--;
			ShotsFired++;
			var ray = Scene.Camera.ScreenNormalToRay(0.5f);
			ray.Forward += Vector3.Random * Spread;
			var tr = Scene.Trace.Ray(ray, 5000).WithoutTags(Steam.SteamId.ToString()).Run();
			if (tr.Hit)
			{
				tr.GameObject.Components.TryGet<EnemyHealthComponent>( out var dummy, FindMode.EverythingInSelfAndParent);
				tr.GameObject.Components.TryGet<PlayerController>(out var player, FindMode.EverythingInSelfAndParent);
				var damageTaker = tr.GameObject.Components.Get<DamageTaker>(FindMode.EverythingInSelfAndParent);
				
				if (dummy is not null)
				{
					dummy.Hurt(Damage, GameObject.Parent.Id);
					BloodParticle.Clone(tr.HitPosition, Rotation.LookAt(-tr.Normal));
				}
				else if (player is not null)
				{
					player.TakeDamage(Damage, false);
				}
				else
				{
					ImpactParticle.Clone(tr.HitPosition, Rotation.LookAt(tr.Normal));
				}
				if (damageTaker is not null)
				{
					damageTaker.TakeDamage(Damage, GameObject.Parent.Id);
				}
				var decal = Decal.Clone(new Transform(tr.HitPosition + tr.Normal * 2.0f, Rotation.LookAt( -tr.Normal, Vector3.Random )));
				
		if (tr.Surface is null)
		{
    			Log.Info("Surface is null");
		}
		else
		{
    	var surfaceSound = tr.Surface.PlayCollisionSound(tr.HitPosition);
  		if (surfaceSound is null)
  		{
       		 Log.Info("surfaceSound is null");
   		}
   		 else
   		{
       		surfaceSound.Volume = 1;
    	}
}
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
				ViewModelGun.Set(ShootBool, true);
			}
			if (FireSound is not null)
			{
			FireSound.Volume = 0.1f;
			Sound.Play(FireSound, tr.StartPosition);
			}
			if (MuzzleFlash is not null)
			{
			var muzzle = ViewModelGun.GetAttachment("muzzle");
			var MuzzleFlashInstance = MuzzleFlash.Clone(muzzle.Value.Position, muzzle.Value.Rotation);
			MuzzleFlashInstance.Tags.Add("viewmodel");
			}
		}
}
}
