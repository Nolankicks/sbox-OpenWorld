using Sandbox;

public sealed class ItemTrigger : Component, Component.ITriggerListener
{
	[Property] public GameObject gameObject { get; set; }
	protected override void OnUpdate()
	{

	}
	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		other.GameObject.Components.TryGet<Inventory>(out var inventory);
		if (inventory != null)
		{
			Log.Info("Item picked up");
			inventory.RemoveItem(inventory.Items[0]);
		}
	}

	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{

	}
}
