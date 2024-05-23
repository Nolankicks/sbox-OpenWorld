using System;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Network;
using Sandbox.UI;
using Sandbox.Utility;
namespace Kicks;
public sealed class Weapon : Component
{
	[Property, Sync] public bool AbleToDrop { get; set; } = true;
	[Property] public string ShootBool { get; set; } = "b_attack";
	[Property] public string ReloadBool { get; set; } = "b_reload";
	[Property, Category("Weapon Properties")] public int Damage { get; set; }
	[Property] public string WeaponIndent { get; set; } 
	[Property, Category("Weapon Properties")] public int Ammo { get; set; }
	[Property] public bool UsesAmmo { get; set; } = true;
	[Property, Category("Weapon Properties")] public int MaxAmmo { get; set; }
	[Property, Range(0.1f, 1), Category("Weapon Properties")] public float FireRate { get; set; }
	[Property, Category("Weapon Properties")] public int TraceLength { get; set; } = 5000;
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

	[Property, Category("Weapon Properties")] public AmmoContainer.AmmoTypes AmmoType { get; set; }
		/// <summary>
	/// Use this if you want to have a muzzle flash, if left empty it will not spawn a muzzle flash
	/// </summary>
	[Property, Category("Prefabs")] public GameObject MuzzleFlash { get; set; }
	[Property, Sync] public bool IsWeapon { get; set; }
	[Property, Category("Prefabs")] public GameObject BloodParticle { get; set; }
	[Property, Category("Prefabs")] public GameObject ImpactParticle { get; set; }
	[Sync] public bool IsAiming { get; set; }
	[Property] public int StartingAmmo { get; set; }
	public AmmoContainer AmmoContainer { get; set; }
	public delegate void OnFireDel(PlayerController playerController, Weapon weapon, AmmoContainer ammoContainer);
	[Property] public OnFireDel OnFire { get; set; }
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
		if (!IsProxy)
		{
			if (IsWeapon)
			{
				Actions();
				foreach (var gb in ViewModelCamera?.GetAllObjects(false))
				{
					if (gb is null) return;
					gb.Enabled = true;
				}
				foreach (var gb in DroppedItem?.GetAllObjects(false))
				{
					if (gb is null) return;
					gb.Enabled = false;
				}
			}
			else
			{
				foreach (var gb in ViewModelCamera?.GetAllObjects(false))
				{
					gb.Enabled = false;
				}
				foreach (var gb in DroppedItem?.GetAllObjects(false))
				{
					gb.Enabled = true;
				}
			}
		}
		else
		{
			if (IsWeapon)
			{
				foreach (var gb in DroppedItem?.GetAllObjects(false))
				{
					gb.Enabled = false;
				}
			}
			else
			{
				foreach (var gb in DroppedItem?.GetAllObjects(false))
				{
					gb.Enabled = true;
				}
			}
			foreach (var gb in ViewModelCamera?.GetAllObjects(false))
			{
				if (gb is null) return;
				gb.Enabled = false;
			}
		}
	}

	void Actions()
	{
		ViewModelCamera.Enabled = true;
		Arms.Enabled = true;
		ViewModelGun.GameObject.Enabled = true;
		ViewModelGun.Set("b_twohanded", true);
		if (Input.Down("attack1") && TimeSinceReload > 2.5)
		{
			Fire();
		}
		
		IsAiming = Input.Down("attack2");
		ViewModelGun.Set( "ironsights", IsAiming ? 2 : 0 );
		ViewModelGun.Set( "ironsights_fire_scale", IsAiming ? 0.2f : 0f );
		UpdateWorldModelShadowType();
		if (PlayerController.IsFirstPerson)
		{
			var selelctedAmmo = AmmoContainer.GetAmmo(AmmoType);
			if (Input.Pressed("reload") && MaxAmmo != 0 && ShotsFired != 0 && !IsProxy && selelctedAmmo > 0 && UsesAmmo)
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
			ViewModelGun.Set("aim_pitch", PlayerController.eyeAngles.pitch);
			ViewModelGun.Set("aim_yaw", PlayerController.eyeAngles.yaw);
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
			ViewModelGun.Set("aim_yaw", PlayerController.eyeAngles.yaw);
			ViewModelGun.Set("aim_pitch", PlayerController.eyeAngles.pitch);
			
		}
	}
	protected override void OnDisabled()
	{
		if (!IsProxy)
		{
			ViewModelGun.Set("b_grounded", false);
		}
	}
	public void DropItem(Inventory inventory)
	{
		if (!AbleToDrop) return;
		var allObjects = GameObject.GetAllObjects(false);
		var tr = Scene.Trace.Ray(Scene.Camera.ScreenNormalToRay(0.5f), 50).WithoutTags("player").Run();
		GameObject.Parent = null;
		inventory.RemoveItem(GameObject, false);
		IsWeapon = false;
		//Idk if I need to refresh this shit but I will anyway 🤓☝️
		DroppedItem.Transform.LocalPosition = Vector3.Zero;
		GameObject.Transform.Position = tr.EndPosition;
		Network.Refresh();
		if (GameNetworkSystem.IsActive)
		{
		foreach(var gb in allObjects)
		{
			gb.Network.DropOwnership();
		}
		}
	}

	public void PickUp(Inventory inventory)
	{
		var allObjects = GameObject.GetAllObjects(false);
		if (GameNetworkSystem.IsActive)
		{
		foreach(var gb in allObjects)
		{
			gb.Network.TakeOwnership();
		}
		}
		
		GameObject.Parent = PlayerController.GameObject;
		GameObject.Transform.LocalPosition = new Vector3(0, 0, 70);
		IsWeapon = true;
		inventory.AddItem(GameObject, inventory.GetNextSlot(), false);
		Network.Refresh();

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
		OnFire?.Invoke(PlayerController, this, AmmoContainer);
		if (Ammo > 0 && TimeSinceFire > FireRate)
		{
			PlayerController.eyeAngles += new Angles(-Recoil, GetRandomFloat(), 0);
			if (UsesAmmo)
			{
			Ammo--;
			ShotsFired++;
			}
			var ray = Scene.Camera.ScreenNormalToRay(0.5f);
			ray.Forward += Vector3.Random * Spread;
			var tr = Scene.Trace.Ray(ray, TraceLength).WithoutTags(Steam.SteamId.ToString()).Run();
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
				decal.NetworkSpawn(null);
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
			Sound.Play(FireSound, tr.StartPosition);
			}
			if (MuzzleFlash is not null)
			{
			var muzzle = ViewModelGun.GetAttachment("muzzle");
			if (muzzle is not null)
			{
			var MuzzleFlashInstance = MuzzleFlash.Clone(muzzle.Value.Position, muzzle.Value.Rotation);
			MuzzleFlashInstance.Tags.Add("viewmodel");
			}
			}
		}
		else
		{
			ViewModelGun.Set("b_attack", false);
		}
}
}
