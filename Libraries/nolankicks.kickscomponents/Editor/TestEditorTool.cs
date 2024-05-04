using Editor;
using Sandbox;
using System.Xml;


[EditorTool] // this class is an editor tool
[Title( "Rocket" )] // title of your tool
[Icon( "rocket_launch" )] // icon name from https://fonts.google.com/icons?selected=Material+Icons
[Shortcut( "editortool.rocket", "u" )] // keyboard shortcut
public class MyRocketTool : EditorTool
{
	public override void OnEnabled()
	{
	}

	public override void OnDisabled()
	{
	}

public override void OnUpdate()
{
	var tr = Scene.Trace.Ray( Gizmo.CurrentRay, 5000 )
					.UseRenderMeshes( true )
					.UsePhysicsWorld( false )
					.Run();
}
}
