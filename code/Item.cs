using Sandbox;

public sealed class Item : Component
{
	[Property] public Texture Icon { get; set; }
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }
	[Property] public GameObject itemToSpawn { get; set; }
	public Inventory Inventory { get; set; }
	public bool Ammo { get; set; }

	protected override void OnUpdate()
	{
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy );
	}
}
