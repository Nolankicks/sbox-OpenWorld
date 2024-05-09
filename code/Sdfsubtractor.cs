using Sandbox;
using Sandbox.Sdf;

public sealed class SDFGun : Component
{
	public Sdf3DWorld World { get; set; }
	[Property] public float SphereSize { get; set; } = 100;
	[Property] public bool AddBool { get; set; } = true;
	[Property] public Sdf3DVolume sdf3DVolume { get; set; }
	[Property] public bool CanAdd { get; set; } = false;
	protected override void OnUpdate()
	{
		if (IsProxy) return;
		World = Scene.GetAllComponents<Sdf3DWorld>().FirstOrDefault(x => x.Tags.Has("sdf"));
		var tr = Scene.Trace.Ray(Scene.Camera.ScreenNormalToRay(0.5f), 500).WithoutTags("player").Run();
		if (tr.Hit)
		{
		Gizmo.Transform = new Transform(tr.HitPosition.SnapToGrid(32), Rotation.Identity);
		if (CanAdd)
		{
		if (AddBool)
		{
			Gizmo.Draw.Color = Color.Green;
		}
		else
		{
			Gizmo.Draw.Color = Color.Red;
		}
		}
		else
		{
			Gizmo.Draw.Color = Color.Red;
		}
		
		Gizmo.Draw.LineSphere(Vector3.Zero, SphereSize);
		}
		if (Input.Pressed("attack1"))
		{
			if (CanAdd)
			{
			if (AddBool)
			{
				_ = Add();
			}
			else
			{
				_ = Subtract();
			}
			}
			else
			{
				_ = Subtract();
			}
			
		}

		if (Input.Pressed("Flashlight"))
		{
			AddBool = !AddBool;
		}
	}
	public async Task Add()
	{
		if (World is null) return;
		World.Network.TakeOwnership();
		var tr = Scene.Trace.Ray(Scene.Camera.ScreenNormalToRay(0.5f), 500).WithoutTags("player").Run();
		var sphere = new SphereSdf3D(Vector3.Zero, SphereSize).Transform(new Transform(tr.HitPosition, Rotation.Identity));
		await World.AddAsync(sphere, sdf3DVolume);
	}

	public async Task Subtract()
	{
		if (World is null) return;
		World.Network.TakeOwnership();
		var tr = Scene.Trace.Ray(Scene.Camera.ScreenNormalToRay(0.5f), 500).WithoutTags("player").Run();
		var sphere = new SphereSdf3D(Vector3.Zero, SphereSize).Transform(new Transform(tr.HitPosition, Rotation.Identity));
		await World.SubtractAsync(sphere);
	}
}
