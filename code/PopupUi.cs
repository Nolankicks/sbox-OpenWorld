using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Sandbox;
namespace Kicks;
[Title( "Action Graph Popup" ), Icon( "check" )]
public sealed class PopupUi : Component
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }
	public InputHint inputHint { get; set; }
	public delegate void PickUpActionDelegate( PlayerController PlayerController, Inventory Inventory, AmmoContainer ammoContainer, InputHint inputHint );
	[Property] public PickUpActionDelegate PickUpAction { get; set; }
	[Property, InputAction] public string selectedInput { get; set; } = "use";
	public PlayerController playerController { get; set; }
	public Inventory Inventory { get; set; }
	[Property] public Texture Icon { get; set; }
	[Property] public bool ShowPopUp { get; set; } = true;
	[Property, Category( "Structs" )] public ShopItems ShopItems { get; set; }
	public Texture Glyph { get; set; }
	protected override void OnUpdate()
	{
		playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
		inputHint = Scene.GetAllComponents<InputHint>().FirstOrDefault( x => !x.IsProxy );
		Glyph = Input.GetGlyph( selectedInput, InputGlyphSize.Large, false );
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy );
		if ( PickUpAction is null ) return;
	}
	[ActionGraphNode( "GetNextSlot" ), Pure]
	public int GetNextSlot()
	{
		Inventory inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy );
		return inventory.Items.FindIndex( x => x is null );
	}
	protected override void OnDestroy()
	{
		ShowPopUp = false;
	}

	public void UnParent( GameObject gameObject )
	{
		gameObject.Parent = null;
	}
}
