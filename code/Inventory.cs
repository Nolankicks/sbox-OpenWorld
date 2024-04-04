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
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
		Items = new List<GameObject>(new GameObject[9]);
		ItemTextures = new List<Texture>(new Texture[9]);
		Log.Info(Items.Count);
		foreach (var item in StartingItems)
		{
			AddItem(item, Items.FindIndex(x => x is null));
		}
		
	}

	public void AddItem(GameObject item, int Slot)
	{
		if (item is null) return;
		var itemClone = item.Clone();
		Items[Slot] = itemClone;
		itemClone.Parent = GameObject;
	}
	public void RemoveItem(GameObject item)
	{
		if (item is null) return;
		var index = Items.FindIndex(x => x == item);
		Items[index] = null;
		item.Destroy();
	}

	protected override void OnUpdate()
{
    for (int i = 0; i < Items.Count; i++)
    {
		if (Items[i] is not null)
		{
		if (i == ActiveSlot)
        {
            Items[i].Enabled = true;
        }
        else
        {
            Items[i].Enabled = false;
        }
		}
    }
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
	if (weapon is not null)
    {
        PlayerController.AnimationHelper.HoldType = weapon.HoldType;
    }
	else
	{
		PlayerController.AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
	}
}
	void ChangeSlots()
	{
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
}
