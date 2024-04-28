using Sandbox;

/// <summary>
/// This is a component - in your library!
/// </summary>
[Title( "Gravity Controller" )]
public class GravityController : Component
{
	[Property] [Range(0.0f, 10000, 0.5f, false, true)] float Strength { get; set; } = 850.0f;
    protected override void OnUpdate()
    {
        base.OnUpdate();
		if (Scene.PhysicsWorld.Gravity != Transform.Rotation.Down * Strength)
		{
			Scene.PhysicsWorld.Gravity = Transform.Rotation.Down * Strength;
            foreach (var item in Scene.PhysicsWorld.Bodies)
            {
				item.Sleeping = false;
            }
        }
		
    }

    protected override void DrawGizmos()
    {
        base.DrawGizmos();
		Gizmo.Draw.Arrow( Vector3.Zero, Vector3.Down * Strength / 50.0f, 4, 1 );
    }
}
