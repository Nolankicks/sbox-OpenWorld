#nullable enable
using Sandbox;
using System.Collections.Generic;
public class ShopItems
{
	public List<string> ItemNames { get; set; } = new();
    public List<Action> actions { get; set; } = new();
	public List<Texture> ItemTextures { get; set; } = new();
	public List<int> ItemPrices { get; set; } = new();
}
