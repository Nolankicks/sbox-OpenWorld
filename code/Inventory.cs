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
		if (IsProxy) return;
		Items = new List<GameObject>(new GameObject[9]);
		ItemTextures = new List<Texture>(new Texture[9]);
		Log.Info(Items.Count);
		AddItem(Gun, 0);
	}

	public void AddItem(GameObject item, int Slot, bool Spawn = true)
	{
		if (IsProxy) return;
		if (Spawn)
		{
		if (item is null) return;
		var itemClone = item.Clone();
		Items[Slot] = itemClone;
		itemClone.Parent = GameObject;
		var weapon = itemClone.Components.Get<Weapon>();
		weapon.IsWeapon = true;
		AddTexture(itemClone.Components.Get<IconComponent>().Icon, Slot);
		}
		else
		{
			Items[Slot] = item;
			item.Parent = GameObject;
			AddTexture(item.Components.Get<IconComponent>().Icon, Slot);
		}
	}
	public void AddTexture(Texture texture, int Slot)
	{
		if (IsProxy) return;
		ItemTextures[Slot] = texture;
	}
	public void RemoveItem(GameObject item, bool Destroy = true)
	{
		if (IsProxy) return;
		if (Destroy)
		{
		if (item is null) return;
		var index = Items.FindIndex(x => x == item);
		Items[index] = null;
		ItemTextures[index] = null;
		item.Destroy();		
		}
		else
{
    var index = Items.FindIndex(x => x == item);
    if (index != -1)
    {
        Items[index] = null;
        ItemTextures[index] = null;
    }
}
	}

	protected override void OnUpdate()
{
	if (IsProxy) return;
	ChangeItems();
	ChangeSlots();
	Pickup();
	Drop();
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
	foreach (GameObject weapon in Items)
		{
			if (weapon is not null && weapon == Items[ActiveSlot] && !IsProxy)
			{
				weapon.Enabled = true;
			}
			if (weapon is not null && weapon != Items[ActiveSlot] && !IsProxy)
			{
				weapon.Enabled = false;
			}
		}
	}
	void Pickup()
	{
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 300).WithoutTags("player").Run();
		if (tr.Hit && Input.Pressed("use"))
		{
			tr.GameObject.Parent.Components.TryGet<Weapon>(out var item);
			tr.GameObject.Parent.Components.TryGet<IconComponent>(out var icon);
			if (item is not null && icon is not null)
			{
				//Add Ownership, yay
				item.GameObject.Network.TakeOwnership();
				item.GameObject.Network.TakeOwnership();
				item.ViewModelCamera.Network.TakeOwnership();
				item.ViewModelGun.GameObject.Network.TakeOwnership();
				item.Arms.Network.TakeOwnership();
				item.ViewModelHolder.Network.TakeOwnership();
				item.GameObject.Transform.Rotation = Rotation.Identity;
				//Idk if I need to refresh this shit but I will anyway 🤓☝️
				Network.Refresh();
				var slot = Items.FindIndex(x => x is null);
				AddItem(item.GameObject, slot, false);
				AddTexture(icon.Icon, slot);
				item.GameObject.Parent = GameObject;
				item.IsWeapon = true;
				item.GameObject.Transform.Position = Transform.Position;
			}
		}
	}

	void Drop()
	{
		if (ActiveSlot < 0 || ActiveSlot >= Items.Count || Items[ActiveSlot] is null) return;
		if (Input.Pressed("menu"))
		{
		var item = Items[ActiveSlot];
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 50).WithoutTags("player").Run();
		item.Components.TryGet<Weapon>( out var weapon );
		item.Components.TryGet<Shotgun>( out var shotgun );
		item.Components.TryGet<IconComponent>(out var icon);
		if (weapon is not null && icon is not null)
		{
		RemoveItem(item, false);
		//God I hate this, i'm too dumb for the foreach shit
		weapon.GameObject.Network.TakeOwnership();
		weapon.GameObject.Network.TakeOwnership();
		weapon.ViewModelCamera.Network.TakeOwnership();
		weapon.ViewModelGun.GameObject.Network.TakeOwnership();
		weapon.Arms.Network.TakeOwnership();
		weapon.ViewModelHolder.Network.TakeOwnership();
		weapon.GameObject.Parent = null;
		weapon.IsWeapon = false;
		//Idk if I need to refresh this shit but I will anyway 🤓☝️
		Network.Refresh();
		weapon.GameObject.Transform.Position = tr.Hit ? tr.HitPosition : tr.EndPosition;
		RemoveItem(item, false);
		}
		else if (shotgun is not null && icon is not null)
		{
		var itemClone = shotgun.ItemPrefab.Clone(tr.EndPosition);
		var rb = itemClone.Components.Get<Rigidbody>();
		RemoveItem(item, false);
		}
		else
		{
			Log.Error("Item is not a weapon");
		}
	}
	}
}
