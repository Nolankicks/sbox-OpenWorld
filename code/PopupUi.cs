using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Sandbox;
namespace Kicks;

public sealed class PopupUi : Component
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }
	public delegate void PickUpActionDelegate( PlayerController PlayerController, Inventory Inventory );
	public delegate void ShopActionDelegate( PlayerController PlayerController, Inventory Inventory, ShopUi shopUi, ShopItems shopItems, int index);
	[Property] public PickUpActionDelegate PickUpAction { get; set; }
	[Property] public ShopActionDelegate ShopAction { get; set; }
	[Property] public Inputs selectedInput { get; set; }
	public PlayerController playerController { get; set; }
	public Inventory Inventory { get; set; }
	[Property] public Texture Icon { get; set; }
	[Property] public bool ShowPopUp { get; set; } = true;
	[Property, Category("Structs")] public ShopItems ShopItems { get; set; }
	public Texture Glyph { get; set; }
	protected override void OnUpdate()
	{
			playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
			Glyph = Input.GetGlyph(InputHandler.GetInputString(selectedInput), InputGlyphSize.Small, false);
			Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault(x => !x.IsProxy);
			if (PickUpAction is null) return;
	}

	[ActionGraphNode("AddItem"), Pure]
	[Title( "Add Item" ), Group( "PopupUi" ), Icon( "exposure_plus_1" )]
	public void AddItem(GameObject gameObject)
	{
		Inventory inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault(x => !x.IsProxy);
		var slot = inventory.Items.FindIndex(x => x is null);
		if (slot == -1) return;
		inventory.AddItem(gameObject, slot);
	}
	[ActionGraphNode("RemoveItem"), Pure]
	public void RemoveItem(GameObject gameObject)
	{
		Inventory inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault(x => !x.IsProxy);
		inventory.RemoveItem(gameObject);
	}
	[ActionGraphNode("GetInventory"), Pure]
	public Inventory GetInventory()
	{
		return Scene.GetAllComponents<Inventory>().FirstOrDefault(x => !x.IsProxy);
	}
	[ActionGraphNode("GetNextSlot"), Pure]
	public int GetNextSlot()
	{
		Inventory inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault(x => !x.IsProxy);
		return inventory.Items.FindIndex(x => x is null);
	}
}
