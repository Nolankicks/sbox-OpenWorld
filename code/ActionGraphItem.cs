using Kicks;
using Sandbox;
[Icon("directions_walk")]
public sealed class ActionGraphItem : Component
{
	[Property] public GameObject DropppedItem { get; set; }
	[Property] public bool InInventory { get; set; }
	[Property] public GameObject Object { get; set; }
	public delegate void OnUseDel(PlayerController playerController, Inventory inventory, AmmoContainer ammoContainer);
	[Property] public OnUseDel OnUse { get; set; }
	public Inventory Inventory { get; set; }
	public AmmoContainer AmmoContainer { get; set; }
	public PlayerController PlayerController { get; set; }
	protected override void OnStart()
	{
		var allObjects = GameObject.GetAllObjects(false);
		foreach(var gb in allObjects)
		{
			gb.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		}
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy);
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy);
		AmmoContainer = Scene.GetAllComponents<AmmoContainer>().FirstOrDefault( x => !x.IsProxy);
	}
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		if (InInventory)
		{
			Object.Enabled = true;
			DropppedItem.Enabled = false;
			Use();
		}
		else
		{
			Object.Enabled = false;
			DropppedItem.Enabled = true;
		}
	}

	public void DropItem()
	{
		var allObjects = GameObject.GetAllObjects(false);
		foreach(var gb in allObjects)
		{
			gb.Network.DropOwnership();
		}
		GameObject.Parent = null;
		InInventory = false;
		Inventory.RemoveItem(GameObject, false);
		//Idk if I need to refresh this shit but I will anyway 🤓☝️
		var tr = Scene.Trace.Ray(Scene.Camera.ScreenNormalToRay(0.5f), 200).WithoutTags("player").Run();
		GameObject.Transform.Position = tr.EndPosition + Vector3.Up * 20;
		Network.Refresh();
	}
	public void PickUp()
	{
		var allObjects = GameObject.GetAllObjects(false);
		foreach(var gb in allObjects)
		{
			gb.Network.TakeOwnership();
		}
		GameObject.Parent = PlayerController.GameObject;
		GameObject.Transform.LocalPosition = new Vector3(0, 0, 70);
		Inventory.AddItem(GameObject, Inventory.GetNextSlot(), false);
		InInventory = true;
		Network.Refresh();
	}
	public void Use()
	{
		if (Input.Pressed("attack1"))
		{
			OnUse?.Invoke(PlayerController, Inventory, AmmoContainer);
		}
	}
	}
