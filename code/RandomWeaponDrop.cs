using Sandbox;

public sealed class RandomWeaponDrop : Component
{
	[Property] public List<GameObject> RandomWeapons { get; set; }
	public Inventory Inventory { get; set; }

	protected override void OnUpdate()
	{
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy );
	}

	public void DropWeapon()
	{
		var weapon = Game.Random.FromList(RandomWeapons);
		var clone = weapon.Clone(Transform.Position + Vector3.Up * 10);
		clone.NetworkSpawn(null);
	}
}
