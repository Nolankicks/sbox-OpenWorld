using Sandbox;
using Sandbox.Utility;

public sealed class RagdollTest : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public GameObject Target { get; set; }
	protected override void OnStart()
	{
		_ = TestShitter(5, Target.Transform.Position, Easing.Linear);
	}

	async Task TestShitter( float seconds, Vector3 to, Easing.Function function )
	{
		TimeSince timeSince = 0;
		Vector3 from = Transform.Position;
	 
		while ( timeSince < seconds )
		{
			var pos = Vector3.Lerp( from, to, function( timeSince / seconds ) );
			Transform.Position = pos;
			await Task.Frame();
		}
		var modelPhys = Renderer.Components.Create<ModelPhysics>();
		modelPhys.Renderer = Renderer;
		modelPhys.Model = Renderer.Model;
	}
}
