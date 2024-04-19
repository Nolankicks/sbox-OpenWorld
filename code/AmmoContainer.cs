using Sandbox;
namespace Kicks;
public sealed class AmmoContainer : Component
{
	public enum AmmoTypes 
	{
		LightAmmo,
		ShotgunShells,
		HeavyAmmo,
	}

	[Property, Sync] public int LightAmmo { get; set; }
	[Property, Sync] public int ShotgunShells { get; set; }
	[Property, Sync] public int HeavyAmmo { get; set; }
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
}
