using Sandbox;

public sealed class Chest : Component
{
	[Property] public List<GameObject> ChestItems { get; set; } = new();
	public Inventory Inventory { get; set; }
	[Property] public bool IsOpen { get; set; }
	protected override void OnStart()
	{
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy );
	}

	public void TakeItem(GameObject Item)
	{
		if (ChestItems.Contains(Item))
		{
			ChestItems.Remove(Item);
			Inventory.AddItem(Item, Inventory.GetNextSlot());
		}
	}

	public void ToggleUi()
	{
		IsOpen = !IsOpen;
	}
}
