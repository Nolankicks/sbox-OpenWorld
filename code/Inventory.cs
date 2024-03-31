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
		Items.Add(item);
		var itemClone = item.Clone();
		itemClone.Parent = GameObject;
	}

	public void RemoveItem(GameObject item)
	{
		item.Destroy();
		Items.Remove(item);
	}
}
