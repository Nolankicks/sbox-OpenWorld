using Sandbox;

public sealed class Inventory : Component
{
	[Property] public List<GameObject> Items { get; set; } = new();
	[Property] public GameObject Gun { get; set; }
	protected override void OnStart()
	{
		AddItem(Gun);
	}

	public void AddItem(GameObject item)
	{
		var itemClone = item.Clone();
		Items.Add(itemClone);
		itemClone.Parent = GameObject;
	}

	public void RemoveItem(GameObject item)
	{
		if (item is null) return;
		item.Destroy();
		Items.Remove(item);
	}
}
