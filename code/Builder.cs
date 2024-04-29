using Sandbox;

public sealed class Builder : Component
{
	[Property] public Model wall { get; set; }
	[Property] public bool CanPlace { get; set; }
	[Property] public GameObject Box { get; set; }
	public Inventory Inventory { get; set; }
	[Property] public Angles PlaceRotation { get; set; }
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault(x => !x.IsProxy);
		if (Input.Pressed("build"))
		{
			CanPlace = !CanPlace;
		}
		if (CanPlace)
		{
			Place();
			Inventory.DisableAllWeapons();
		}
		else
		{
			Inventory.EnableAllWeapons();
		}
	}

	void Place()
	{
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 500).WithoutTags("player").Run();
		if (tr.Hit)
		{
			var wall = BBox.FromPositionAndSize(Vector3.Zero, new Vector3(25, 100, 100));
			Gizmo.Transform = new Transform(tr.HitPosition.SnapToGrid(25) + Vector3.Up * 50, PlaceRotation);
			Gizmo.Draw.Color = Color.Blue;
			Gizmo.Draw.LineBBox(wall);
			
		}
		if (Input.Pressed("attack1"))
		{
			var box = Box.Clone(new Transform(tr.HitPosition.SnapToGrid(25) + Vector3.Up * 50));
		}
	}
}
