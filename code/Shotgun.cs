using Sandbox;
using Sandbox.UI;

public sealed class Shotgun : Component
{
	[Property] public SkinnedModelRenderer ViewModelGun { get; set; }
	[Property] public SkinnedModelRenderer arms { get; set; }
	public PlayerController PlayerController { get; set; }
	[Property] public GameObject ViewModelCamera { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public GameObject Decal { get; set; }
	[Property] public int Damage { get; set; }
	[Property] public int Ammo { get; set; }
	[Property] public int MaxAmmo { get; set; }
	public TimeSince TimeSinceFire;
	public TimeSince timeSinceReload = 1.5f;
	[Property] public float FireRate { get; set; } = 0.5f;
	[Property] public SoundEvent FireSound { get; set; }
	public int StartingAmmo { get; set; }
	public int ShotsFired = 0;
	[Property] public bool IsAiming { get; set; }
	protected override void OnStart()
	{
		if (IsProxy) return;
		StartingAmmo = Ammo;
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		TimeSinceFire = FireRate;
		if (PlayerController.IsFirstPerson)
		{
			ViewModelCamera.Enabled = true;
	
		
		}
	}
	protected override void OnUpdate()
	{
		if (IsProxy) return;
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

	void Shoot()
	{
		if (IsProxy) return;
		if (Ammo > 0)
		{
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		ray.Forward += Vector3.Random * 0.05f;
		var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
		ViewModelGun.Set("b_attack", true);
		if (tr.Hit)
		{
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
