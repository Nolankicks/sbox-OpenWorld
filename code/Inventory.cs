using Sandbox;
using Sandbox.Citizen;
using System;
public sealed class Inventory : Component
{
	[Property] public List<GameObject> Items;
	[Property] public List<Texture> ItemTextures;
	[Property] public List<GameObject> StartingItems { get; set; } = new();
	[Property] public GameObject Gun { get; set; }
	public PlayerController PlayerController { get; set; }
	[Property] public Texture TestTexture { get; set; }
 	public int ActiveSlot = 0;
	public int Slots => 9;
	protected override void OnStart()
	{
		if (IsProxy) return;
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
		Items = new List<GameObject>(new GameObject[9]);
		ItemTextures = new List<Texture>(new Texture[9]);
		Log.Info(Items.Count);
		AddItem(Gun, 0);
	}

	public void AddItem(GameObject item, int Slot)
	{
		if (IsProxy) return;
		if (item is null) return;
		var itemClone = item.Clone();
		Items[Slot] = itemClone;
		itemClone.Parent = GameObject;
		AddTexture(itemClone.Components.Get<IconComponent>().Icon, Slot);
	}
	public void AddTexture(Texture texture, int Slot)
	{
		if (IsProxy) return;
		ItemTextures[Slot] = texture;
	}
	public void RemoveItem(GameObject item)
	{
		if (IsProxy) return;
		if (item is null) return;
		var index = Items.FindIndex(x => x == item);
		Items[index] = null;
		item.Destroy();
	}

	protected override void OnUpdate()
{
	if (IsProxy) return;
	ChangeItems();
	ChangeSlots();
	foreach (var item in Items)
	{
		if (item is not null)
		{
			item.Components.TryGet<IconComponent>(out var icon);
			if (icon is not null)
			{
				ItemTextures[Items.FindIndex(x => x == item)] = icon.Icon;
			}
		}
	}
	if (Items[ActiveSlot] is null) return;
	Items[ActiveSlot].Components.TryGet<Weapon>(out var weapon);
}
	void ChangeSlots()
	{
		if (IsProxy) return;
		if (Input.Pressed("Slot1"))
		{
			ActiveSlot = 0;
		}
		if (Input.Pressed("Slot2"))
		{
			ActiveSlot = 1;
		}
		if (Input.Pressed("Slot3"))
		{
			ActiveSlot = 2;
		}
		if (Input.Pressed("Slot4"))
		{
			ActiveSlot = 3;
		}
		if (Input.Pressed("Slot5"))
		{
			ActiveSlot = 4;
		}
		if (Input.Pressed("Slot6"))
		{
			ActiveSlot = 5;
		}
		if (Input.Pressed("Slot7"))
		{
			ActiveSlot = 6;
		}
		if (Input.Pressed("Slot8"))
		{
			ActiveSlot = 7;
		}
		if (Input.Pressed("Slot9"))
		{
			ActiveSlot = 8;
		}
		if (Input.Pressed("Slot0"))
		{
			ActiveSlot = 9;
		}
		 if (Input.MouseWheel.y != 0)
    {
        ActiveSlot = (ActiveSlot + Math.Sign(Input.MouseWheel.y)) % Slots;
        if (ActiveSlot < 0) ActiveSlot += Slots;
    }
	}
	void ChangeItems()
	{
		if (IsProxy) return;
	foreach (GameObject weapon in Items)
		{
			if (weapon is not null && weapon == Items[ActiveSlot] && !IsProxy)
			{
				weapon.Enabled = true;
				Network.Refresh();
			}
			if (weapon is not null && weapon != Items[ActiveSlot] && !IsProxy)
			{
				weapon.Enabled = false;
				Network.Refresh();
			}
		}
	}
}
