using System.Collections.Generic;
using Sandbox;
[Icon("shuffle"), Description("Contains a function that drops a random item from a list of GameObjects, call DropRandomItem() to drop a random item")]
public sealed class RandomItemDrop : Sandbox.Component
{
	[Property] public List<GameObject> RandomItems { get; set; } = new();

	public void DropRandomItem()
	{
		if (RandomItems.Count == 0) return;
		var randomItem = Game.Random.FromList(RandomItems);
		if (randomItem is not null)
		{
			var item = randomItem.Clone();
			item.Transform.Position = Transform.Position + Vector3.Up * 20;
			item.Transform.Rotation = Transform.Rotation;
			item.NetworkSpawn();
		}
	}
}
