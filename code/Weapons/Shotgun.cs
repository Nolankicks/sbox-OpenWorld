using System.Linq;
using Sandbox;
using Sandbox.UI;
using Kicks;
public sealed class Shotgun : Component
{
	public PlayerController PlayerController { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public GameObject ItemPrefab { get; set; }
	[Property] public GameObject Decal { get; set; }
	[Property] public int Damage { get; set; }
	[Property] public int Ammo { get; set; }
	[Property] public int MaxAmmo { get; set; }
	[Property] public string Name { get; set; }
	[Property, TextArea] public string Description { get; set; }
	public Interactor Interactor { get; set; }
	public TimeSince TimeSinceFire;
	public TimeSince timeSinceReload = 1.5f;
	[Property] public float FireRate { get; set; } = 0.5f;
	[Property] public SoundEvent FireSound { get; set; }
	public int StartingAmmo { get; set; }
	public int ShotsFired = 0;
	[Sync] public bool IsAiming { get; set; }
	[Property, Sync] public bool IsWeapon { get; set; }
	[Property] public GameObject ImpactEffect { get; set; }
	[Property, Category("GameObjects")] public SkinnedModelRenderer ViewModelGun { get; set; }
	[Property] public AmmoContainer.AmmoTypes AmmoType { get; set; }
	[Property, Category("GameObjects")] public SkinnedModelRenderer arms { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelCamera { get; set; }
	[Property, Category("GameObjects")] public GameObject pickUpObject { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelHolder { get; set; }
	public AmmoContainer AmmoContainer { get; set; }
	protected override void OnStart()
	{
		GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		arms.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelGun.GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelCamera.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelHolder.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		Interactor = Scene.GetAllComponents<Interactor>().FirstOrDefault( x => !x.IsProxy);
		AmmoContainer = Scene.GetAllComponents<AmmoContainer>().FirstOrDefault( x => !x.IsProxy);
		if (IsProxy) return;
		StartingAmmo = Ammo;
		TimeSinceFire = FireRate;
		if (PlayerController.IsFirstPerson && !IsWeapon)
		{
			ViewModelCamera.Enabled = true;
		}
	}
		protected override void OnUpdate()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		if (IsProxy)
		{
			ViewModelCamera.Enabled = false;
		}
		if (IsWeapon)
		{
		pickUpObject.Enabled = false;
		ViewModelGun.GameObject.Enabled = true;
		if ( IsProxy ) return;
		
		if (Input.Pressed("attack1") && timeSinceReload > 2.5)
		{
			for (int i = 0; i < 8; i++)
			{
				Shoot();
				ShotsFired++;
			}
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
				ViewModelGun.Set("b_realod", true);
				ShotsFired = 0;
				timeSinceReload = 0;
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
			pickUpObject.Enabled = true;
		}
	}

	void Shoot()
	{
		if (PlayerController.IsGrabbing) return;
		if (Ammo > 0)
		{
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		ray.Forward += Vector3.Random * 0.05f;
		var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
		Ammo--;
		ViewModelGun.Set("b_attack", true);
		if (tr.Hit)
		{
			var decal = Decal.Clone(new Transform(tr.HitPosition + tr.Normal * 2.0f, Rotation.LookAt( -tr.Normal, Vector3.Random )));
			decal.SetParent(tr.GameObject);
			tr.GameObject.Parent.Components.TryGet<EnemyHealthComponent>( out var dummy );
			tr.GameObject.Components.TryGet<DamageTaker>(out var damageTaker);
			if (dummy is not null)
			{
				dummy.Hurt(Damage, GameObject.Parent.Id);
			}
			else
			{
				ImpactEffect.Clone(tr.HitPosition, Rotation.LookAt(tr.Normal));
			}
			if (damageTaker is not null)
			{
				damageTaker.TakeDamage(Damage, GameObject.Parent.Id);
			}
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
		}

	}
}
}
