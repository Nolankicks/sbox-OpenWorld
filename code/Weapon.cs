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
	[Property] public GameObject ViewModelCamera { get; set; }
	[Property] public SkinnedModelRenderer ViewModelGun { get; set; }
	[Property] public Model WorldModel { get; set; }
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; }
	[Property] public float ReloadTime { get; set; }
	public GameObject WorldModelInstance { get; set; }
	int ShotsFired = 0;
	public TimeSince TimeSinceReload { get; set; }
	public TimeSince TimeSinceFire { get; set; }
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
		}
		TimeSinceReload = ReloadTime;
		TimeSinceFire = FireRate;
	}
	protected override void OnUpdate()
	{
		if (Input.Down("attack1") && TimeSinceReload >= ReloadTime && TimeSinceFire >= FireRate && Ammo > 0)
		{
			Fire();
		}

		if (Input.Pressed("reload"))
		{
			Reload();
		}
		Log.Info(Ammo);
		UpdateWorldModelShadowType();
		if (PlayerController.IsFirstPerson)
		{
			ViewModelCamera.Enabled = true;
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
		if (Ammo > 0)
		{
			Ammo--;
			ShotsFired++;
			var ray = Scene.Camera.ScreenNormalToRay(0.5f);
			var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
			if (tr.Hit)
			{
				testObject.Clone(tr.HitPosition);	
		}	
			PlayerController.AnimationHelper.Target.Set("b_attack", true);
			if (PlayerController.IsFirstPerson)
			{
				ViewModelGun.Set("b_attack", true);
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
			PlayerController.AnimationHelper.Target.Set("b_reload", true);
			if (PlayerController.IsFirstPerson)
			{
				ViewModelGun.Set("b_reload", true);
			}
	}
}
