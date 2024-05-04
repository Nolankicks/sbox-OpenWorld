using Sandbox;
using Sandbox.Citizen;
using System;
using Kicks;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.SymbolStore;
using System.Threading.Tasks;

public sealed class Inventory : Component
{
	[Property] public List<GameObject> Items;
	[Property] public List<Texture> ItemTextures;
	[Property] public List<GameObject> StartingItems { get; set; } = new();
	[Property] public GameObject Gun { get; set; }
	public PlayerController PlayerController { get; set; }
	[Property] public Texture TestTexture { get; set; }
	public AmmoContainer AmmoContainer { get; set; }
	public InputHint inputHint { get; set; }
	[Property, Sync] public bool AllWeaponsEnabled { get; set; } = true;
	[Property] public List<GameObject> ItemsToSpawnWith { get; set; } = new();
 	public int ActiveSlot = 0;
	public int Slots => 9;
	protected override void OnStart()
	{
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
		AmmoContainer = Scene.GetAllComponents<AmmoContainer>().FirstOrDefault(x => !x.IsProxy);
		inputHint = Scene.GetAllComponents<InputHint>().FirstOrDefault(x => !x.IsProxy);
		Items = new List<GameObject>(new GameObject[9]);
		ItemTextures = new List<Texture>(new Texture[9]);
		if (IsProxy) return;
		Log.Info(Items.Count);
		AddItem(Gun, 0);
		_ = SpawnItems();
	}

	public async Task SpawnItems()
	{
		if (IsProxy) return;
		await Task.DelaySeconds(3);
		foreach (var item in ItemsToSpawnWith)
		{
			var slot = Items.FindIndex(x => x is null);
			AddItem(item, slot);
			await Task.DelaySeconds(0.1f);
		}
	}

	public void AddItem(GameObject item, int Slot, bool Spawn = true)
	{
		if (IsProxy) return;
		if (Slot > Items.Count) return;
		if (Spawn)
		{
		if (item is null) return;
		var itemClone = item.Clone();
		Items[Slot] = itemClone;
		itemClone.Parent = GameObject;
		itemClone.Components.TryGet<Weapon>( out var weapon );
		itemClone.Components.TryGet<Shotgun>( out var shotgun );
		if (weapon is not null)
		{
			weapon.IsWeapon = true;
		}
		else if (shotgun is not null)
		{
			shotgun.IsWeapon = true;
		}
		AddTexture(itemClone.Components.Get<IconComponent>().Icon, Slot);
		}
		else
		{
			Items[Slot] = item;
			item.Parent = GameObject;
			AddTexture(item.Components.Get<IconComponent>().Icon, Slot);
		}
	}

	public void DisableAllWeapons()
	{
		if (IsProxy) return;
		AllWeaponsEnabled = false;
		foreach (var weapon in Items)
		{
			if (weapon is not null)
			{
				weapon.Components.TryGet<SkinnedModelRenderer>(out var model);
				if (model is not null)
				{
					model.Set("b_holster", true);
				}
				weapon.Enabled = false;
			}
		}
	}
	public void EnableAllWeapons()
	{
		if (IsProxy) return;
		AllWeaponsEnabled = true;
		foreach (var weapon in Items)
		{
			if (weapon is not null)
			{
				weapon.Enabled = true;
			}
		}
	}
	public int GetNextSlot()
	{
		return Items.FindIndex(x => x is null);
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
	Trace();
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
		if (AllWeaponsEnabled)
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
	}
	void Pickup()
	{
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 300).WithoutTags("player").Run();
		if (tr.Hit && Input.Pressed("use"))
		{
			tr.GameObject.Parent.Components.TryGet<Weapon>(out var item);
			tr.GameObject.Parent.Components.TryGet<IconComponent>(out var icon);
			tr.GameObject.Parent.Components.TryGet<Shotgun>(out var shotgun);
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
				var slot = Items.FindIndex(x => x is null);
				AddItem(item.GameObject, slot, false);
				AddTexture(icon.Icon, slot);
				item.GameObject.Parent = GameObject;
				item.IsWeapon = true;
				item.GameObject.Transform.Position = Transform.Position;
				//Idk if I need to refresh this shit but I will anyway 🤓☝️
				Network.Refresh();
			}
			else if (shotgun is not null && icon is not null)
			{
				//Add Ownership, yay
				shotgun.GameObject.Network.TakeOwnership();
				shotgun.GameObject.Network.TakeOwnership();
				shotgun.ViewModelCamera.Network.TakeOwnership();
				shotgun.ViewModelGun.GameObject.Network.TakeOwnership();
				shotgun.arms.GameObject.Network.TakeOwnership();
				shotgun.ViewModelHolder.Network.TakeOwnership();
				shotgun.GameObject.Transform.Rotation = Rotation.Identity;
				Network.Refresh();
				var slot = Items.FindIndex(x => x is null);
				AddItem(shotgun.GameObject, slot, false);
				AddTexture(icon.Icon, slot);
				shotgun.GameObject.Parent = GameObject;
				shotgun.IsWeapon = true;
				shotgun.GameObject.Transform.Position = Transform.Position;
				//Idk if I need to refresh this shit but I will anyway 🤓☝️
				Network.Refresh();
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
			refreshThing("weapon");
		}
		else if (shotgun is not null && icon is not null)
		{
			refreshThing("shotgun");
		}
		else
		{
			return;
		}
		void refreshThing(string itemName)
		{
			// Assign name of item to variable
			Type itemType = Type.GetType(itemName);
			if (itemType != null)
			{
				// Create an item object
				dynamic itemInstance = Activator.CreateInstance(itemType);
				itemInstance.GameObject.Parent = null;
				itemInstance.IsWeapon = false;
				itemInstance.GameObject.Transform.Position = tr.Hit ? tr.HitPosition : tr.EndPosition;
				RemoveItem(itemInstance, false);
				itemInstance.GameObject.Network.DropOwnership();
				itemInstance.ViewModelCamera.Network.DropOwnership();
				itemInstance.ViewModelGun.GameObject.Network.DropOwnership();
				itemInstance.arms.GameObject.Network.DropOwnership();
				itemInstance.ViewModelHolder.Network.DropOwnership();

				Network.Refresh();
			}
			else
			{
				Console.WriteLine("Error with item" + itemName + "in function 'refreshThing'");
			}
		}
	}
	}
	void PopupUi(PopupUi popupUi, Inputs inputAction)
	{
		if (popupUi is null) return;
		if (Input.Pressed(InputHandler.GetInputString(inputAction)))
		{
			popupUi.PickUpAction?.Invoke(PlayerController, this, AmmoContainer, inputHint);
		}
	}

	void Trace()
	{
		try
		{
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 300).WithoutTags("player").Run();
		if (!tr.Hit) return;
		var popup = tr.GameObject.Components.Get<PopupUi>(FindMode.EverythingInSelfAndParent);
		if (Items is null) return;
		if (popup is not null)
		{
			PopupUi(popup, popup.selectedInput);
		}
		}
		catch (System.InvalidOperationException)
		{
			return;
		}
		
	}

	public void AddAmmo(AmmoContainer.AmmoTypes ammoType, AmmoContainer ammoContainer, int ammo)
{
    if (IsProxy) return;
	var currentAmmo = ammoContainer.GetAmmo(ammoType);
	var setAmmo = currentAmmo + ammo;
	ammoContainer.SetAmmo(ammoType, setAmmo);

}


	/*
  
    else
    {
        Log.Error("Weapon not found");
    }*/
}
