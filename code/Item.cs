using Sandbox;

public sealed class Item : Component
{
	[Property] public Texture Icon { get; set; }
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }
	[Property] public GameObject itemToSpawn { get; set; }
	public Inventory Inventory { get; set; }

	protected override void OnUpdate()
	{
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy );
		Trace();
	}

	void Trace()
	{
		if (IsProxy) return;
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 300).WithoutTags("player").Run();
		if (tr.Hit && Input.Pressed("use"))
		{
			var slot = Inventory.Items.FindIndex(x => x is null);
			Log.Info(slot);
			Inventory.AddItem(itemToSpawn, slot);
			GameObject.Destroy();
		}
	}
}
