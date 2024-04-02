using Sandbox;
using Sandbox.Citizen;
using System;
public sealed class Inventory : Component
{
	[Property] public List<GameObject> Items { get; set; } = new List<GameObject>(9);
	[Property] public GameObject Gun { get; set; }
	public PlayerController PlayerController { get; set; }
	public int ActiveSlot = 0;
	public int Slots = 9;
	protected override void OnStart()
	{
		AddItem(Gun);
		PlayerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
	}

	public void AddItem(GameObject item)
	{
		if (item is null) return;
		var itemClone = item.Clone();
		Items.Add(itemClone);
		itemClone.Parent = GameObject;
	}

	public void RemoveItem(GameObject item)
	{
		if (item is null) return;
		item.Destroy();
		Items.Remove(item);
	}

	protected override void OnUpdate()
{
    for (int i = 0; i < Items.Count; i++)
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
    Log.Info(ActiveSlot);
    if (Input.MouseWheel.y != 0)
    {
        ActiveSlot = (ActiveSlot + Math.Sign(Input.MouseWheel.y)) % Slots;
        if (ActiveSlot < 0) ActiveSlot += Slots;
    }

    if (ActiveSlot >= Items.Count) 
	{
		PlayerController.AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		return;
	}
    Items[ActiveSlot].Components.TryGet<Weapon>(out var weapon);
    
    if (weapon is not null)
    {
        PlayerController.AnimationHelper.HoldType = weapon.HoldType;
    }
}
}
