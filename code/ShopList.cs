#nullable enable
using Sandbox;
using System.Collections.Generic;
namespace Kicks;
public class ShopItems
{
	public List<string> ItemNames { get; set; } = new();
	public delegate void ShopActionDelegate( PlayerController PlayerController, Inventory Inventory, AmmoContainer ammoContainer );
	public List<ShopActionDelegate> ShopActions { get; set; } = new();
	public List<Texture> ItemTextures { get; set; } = new();
	public List<int> ItemPrices { get; set; } = new();

	public ShopItems()
	{
		ItemNames = new List<string>();
		ShopActions = new List<ShopActionDelegate>();
		ItemTextures = new List<Texture>();
		ItemPrices = new List<int>();
	}

	public ShopItems( List<string> itemNames, List<ShopActionDelegate> shopActions, List<Texture> itemTextures, List<int> itemPrices )
	{
		ItemNames = itemNames;
		ShopActions = shopActions;
		ItemTextures = itemTextures;
		ItemPrices = itemPrices;
	}
}
