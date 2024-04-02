using Sandbox;
using Sandbox.Citizen;

public sealed class Weapon : Component
{
	[Property] public int Damage { get; set; }
	[Property] public int Ammo { get; set; }
	[Property] public int MaxAmmo { get; set; }
	[Property] public float FireRate { get; set; }
	[Property] public GameObject testObject { get; set; }
	public PlayerController PlayerController { get; set; }
	[Property] public Model ViewModel { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; }
	[Property] public float ReloadTime { get; set; }
	int ShotsFired = 0;
	public TimeSince TimeSinceReload { get; set; }
	protected override void OnStart()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		if (IsProxy) return;
		if (!PlayerController.IsFirstPerson)
		{
			var worldModel = new GameObject();
			worldModel.Components.Create<ModelRenderer>().Model = WorldModel;
			worldModel.Parent = PlayerController.Hold;
		}
		TimeSinceReload = ReloadTime;
	}
	protected override void OnUpdate()
	{
		if (Input.Pressed("attack1") && TimeSinceReload >= ReloadTime)
		{
			Fire();
		}

		if (Input.Pressed("reload"))
		{
			Reload();
		}
		Log.Info(Ammo);
	}

	void Fire()
	{
		if (Ammo > 0)
		{
			Ammo--;
			ShotsFired++;
			var ray = Scene.Camera.ScreenNormalToRay(0.5f);
			var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
			if (tr.Hit)
			{
				testObject.Clone(tr.HitPosition);
				PlayerController.AnimationHelper.Target.Set("b_attack", true);
			}
		}
	}
	void Reload()
	{
			var ammoAfterReload = MaxAmmo -= ShotsFired;
			if (ammoAfterReload >= 0)
			{
				Ammo = 30;
			}
			else
			{
				Ammo = MaxAmmo;
			}
			ShotsFired = 0;
	}
}
