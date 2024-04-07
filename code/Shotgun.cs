using Sandbox;
using Sandbox.UI;

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
	[Property, Category("GameObjects")] public SkinnedModelRenderer ViewModelGun { get; set; }
	[Property, Category("GameObjects")] public SkinnedModelRenderer arms { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelCamera { get; set; }
	[Property, Category("GameObjects")] public GameObject pickUpObject { get; set; }
	[Property, Category("GameObjects")] public GameObject ViewModelHolder { get; set; }
	protected override void OnStart()
	{
		GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		arms.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelGun.GameObject.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelCamera.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ViewModelHolder.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		if (IsProxy) return;
		StartingAmmo = Ammo;
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		Interactor = Scene.GetAllComponents<Interactor>().FirstOrDefault( x => !x.IsProxy);
		TimeSinceFire = FireRate;
		if (PlayerController.IsFirstPerson && !IsWeapon)
		{
			ViewModelCamera.Enabled = true;
	
		
		}
	}
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		if (IsWeapon)
		{
			pickUpObject.Enabled = false;
			if (Input.Pressed("attack1") && TimeSinceFire > FireRate)
		{
			for (int i = 0; i < 4; i++)
			{
				Shoot();
				ShotsFired++;
			}
			Sound.Play(FireSound, Transform.Position + Vector3.Up * 64);
			TimeSinceFire = 0;
		}
		if (Input.Down("attack2"))
		{
			IsAiming = true;
		}
		else
		{
			IsAiming = false;
		}

		Reload();
		if (PlayerController.IsFirstPerson)
		{
			ViewModelCamera.Enabled = true;
			//ViewModelGun.Set("move_groundspeed", PlayerController.CharacterController.Velocity.Length);
		if (!PlayerController.CharacterController.IsOnGround && !IsProxy)
		{
			ViewModelGun.Set("b_grounded", false);
		}
		else
		{
			ViewModelGun.Set("b_grounded", true);
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
		else
		{
			ViewModelCamera.Enabled = false;
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
		ViewModelGun.Set("b_attack", true);
		if (tr.Hit)
		{
			Decal.Clone(tr.HitPosition + tr.Normal, Rotation.LookAt(-tr.Normal));
			tr.GameObject.Parent.Components.TryGet<Dummy>( out var dummy );
			if (dummy is not null)
			{
				dummy.Hurt(Damage);
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

	void Reload()
	{
		if (Input.Pressed("reload") && MaxAmmo != 0 && ShotsFired != 0 && !IsProxy)
			{
				Ammo = MaxAmmo -= ShotsFired;
				Ammo = StartingAmmo;
				ViewModelGun.Set("b_reload", true);
				ShotsFired = 0;
				timeSinceReload = 0;
			}
	}
}
