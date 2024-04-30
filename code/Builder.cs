using Kicks;
using Sandbox;

public sealed class Builder : Component
{
	[Property] public Model wall { get; set; }
	[Property] public bool CanPlace { get; set; }
	[Property] public GameObject Box { get; set; }
	[Property] public GameObject Wall { get; set; }
	[Property] public GameObject Floor { get; set; }
	public Inventory Inventory { get; set; }
	[Property] public Angles PlaceRotation { get; set; }
	public PlayerController playerController { get; set; }
	public int offsetX { get; set; }
	public int offsetY { get; set; }
	public int offsetZ { get; set; }
	public bool WallBool = true;
	protected override void OnUpdate()
	{
		playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault(x => !x.IsProxy);
		if (IsProxy) return;
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault(x => !x.IsProxy);
		if (Input.Pressed("build"))
		{
			CanPlace = !CanPlace;
		}
		if (CanPlace)
		{
			Place();
			if (Input.Pressed("arrowup"))
			{
				offsetY = offsetY + 25;
			}
			if (Input.Pressed("arrowdown"))
			{
				offsetY = offsetY - 25;
			}
			if (Input.Pressed("arrowleft"))
			{
				offsetX = offsetX - 25;
			}
			if (Input.Pressed("arrowright"))
			{
				offsetX = offsetX + 25;
			}
			Inventory.DisableAllWeapons();
			if (Input.Pressed("reload"))
			{
				PlaceRotation = PlaceRotation + new Angles(0, 90, 0);
			}
			if (Input.Pressed("switch"))
			{
				WallBool = !WallBool;
				offsetX = 0;
				offsetY = 0;
				offsetZ = 0;
			}
			if (Input.Pressed("attack2"))
			{
				offsetZ = offsetZ + 25;
			}
			if (WallBool)
			{
				Box = Wall;
			}
			else
			{
				Box = Floor;
			}
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
			if (WallBool)
			{
			var wall = BBox.FromPositionAndSize(Vector3.Zero, new Vector3(25, 100, 100));
			Gizmo.Transform = new Transform(new Vector3(tr.HitPosition.x + offsetX, tr.HitPosition.y + offsetY, tr.HitPosition.z + offsetZ).SnapToGrid(25) + Vector3.Up * 50, PlaceRotation);
			Gizmo.Draw.Color = Color.Blue;
			Gizmo.Draw.LineBBox(wall);
			if (Input.Pressed("attack1"))
			{
			var box = Box.Clone(new Transform(new Vector3(tr.HitPosition.x + offsetX, tr.HitPosition.y + offsetY, tr.HitPosition.z + offsetZ).SnapToGrid(25) + Vector3.Up * 50, PlaceRotation + new Angles(0, 90, 0)));
				offsetX = 0;
				offsetY = 0;
				offsetZ = 0;
			}
			}
			else
			{
			var wall = BBox.FromPositionAndSize(Vector3.Zero, new Vector3(100, 100, 25));
			Gizmo.Transform = new Transform(new Vector3(tr.HitPosition.x + offsetX, tr.HitPosition.y + offsetY, tr.HitPosition.z + offsetZ).SnapToGrid(25) + Vector3.Up * 13, PlaceRotation);
			Gizmo.Draw.Color = Color.Blue;
			Gizmo.Draw.LineBBox(wall);
			if (Input.Pressed("attack1"))
		{
			var box = Box.Clone(new Transform(new Vector3(tr.HitPosition.x + offsetX, tr.HitPosition.y + offsetY, tr.HitPosition.z + offsetZ).SnapToGrid(25) + Vector3.Up * 13, PlaceRotation + new Angles(0, 90, 0)));
				offsetX = 0;
				offsetY = 0;
				offsetZ = 0;
		}
			}

			
		}

	}
}
