using Sandbox;
[Icon("my_location")]
public sealed class ItemPlacer : Component
{
	[Property] public GameObject ItemToPlace { get; set; }
	[Property] public Model ModelToPlace { get; set; }
	[Property] public Angles Rotation { get; set; }
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		if (ModelToPlace is null) return;
		var tr = Scene.Trace.Ray(Scene.Camera.ScreenNormalToRay(0.5f), 500).WithoutTags("player").Run();
		if (!tr.Hit) return;
		Gizmo.Draw.Model(ModelToPlace, new Transform(tr.HitPosition, Rotation));
		if (Input.Pressed("attack1"))
		{
			var itemClone = ItemToPlace.Clone(tr.HitPosition, Rotation);
			itemClone.NetworkSpawn(null);
		}
		if (Input.Down("attack2"))
		{
			Rotation = new Angles(Rotation.pitch, Rotation.yaw + 1, Rotation.roll);
		}
	}
}
