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
	protected override void OnStart()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		if (!PlayerController.IsFirstPerson)
		{
			var worldModel = new GameObject();
			worldModel.Components.Create<ModelRenderer>().Model = WorldModel;
			worldModel.Parent = PlayerController.Hold;
		}
	}
	protected override void OnUpdate()
	{
		if (Input.Pressed("attack1"))
		{
			Fire();
		}
	}

	void Fire()
	{
		if (Ammo > 0)
		{
			Ammo--;
			var ray = Scene.Camera.ScreenNormalToRay(0.5f);
			var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
			if (tr.Hit)
			{
				testObject.Clone(tr.HitPosition);
				PlayerController.AnimationHelper.Target.Set("b_attack", true);
			}
		}
	}
}
