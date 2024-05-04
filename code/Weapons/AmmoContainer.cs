using Sandbox;
namespace Kicks;
public sealed class AmmoContainer : Component
{
	public enum AmmoTypes 
	{
		LightAmmo,
		ShotgunShells,
		HeavyAmmo,
		NoAmmo
	}

	[Property, Sync] public int LightAmmo { get; set; }
	[Property, Sync] public int ShotgunShells { get; set; }
	[Property, Sync] public int HeavyAmmo { get; set; }
	[Property, Sync] public int StartingLightAmmo { get; set; }
	[Property, Sync] public int StartingShotgunShells { get; set; }
	[Property, Sync] public int StartingHeavyAmmo { get; set; }

	protected override void OnStart()
	{
		StartingLightAmmo = LightAmmo;
		StartingShotgunShells = ShotgunShells;
		StartingHeavyAmmo = HeavyAmmo;
	}
	public int GetAmmo(AmmoTypes ammo)
	{
		switch (ammo)
		{
			case AmmoTypes.LightAmmo:
				return LightAmmo;
			case AmmoTypes.ShotgunShells:
				return ShotgunShells;
			case AmmoTypes.HeavyAmmo:
				return HeavyAmmo;
			default:
				return 0;
		}
	}
	public void SetAmmo(AmmoTypes ammo, int amount)
	{
		switch (ammo)
		{
			case AmmoTypes.LightAmmo:
				LightAmmo = amount;
				break;
			case AmmoTypes.ShotgunShells:
				ShotgunShells = amount;
				break;
			case AmmoTypes.HeavyAmmo:
				HeavyAmmo = amount;
				break;
		}
	}

	protected override void OnUpdate()
	{
		if (LightAmmo <= 0)
		{
			LightAmmo = 0;
		}
		if (ShotgunShells <= 0)
		{
			ShotgunShells = 0;
		}
		if (HeavyAmmo <= 0)
		{
			HeavyAmmo = 0;
		}
	}

	public void ResetAmmo()
	{
		LightAmmo = StartingLightAmmo;
		ShotgunShells = StartingShotgunShells;
		HeavyAmmo = StartingHeavyAmmo;
	}
}
