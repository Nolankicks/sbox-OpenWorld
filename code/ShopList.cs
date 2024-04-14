#nullable enable
using Sandbox;
using System.Collections.Generic;

public class ShopItems
{
    public List<GameObject> ItemsToBuy { get; set; } = new();
	public List<Texture> ItemTextures { get; set; } = new();
	public List<int> ItemPrices { get; set; } = new();
}
