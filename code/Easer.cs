using Sandbox;
using Sandbox.Utility;

public sealed class Easer : Component
{
	[ActionGraphNode("LerpPos"), Pure]
	async Task LerpPos( float seconds, Vector3 to, Easing.Function easer )
	{
	TimeSince timeSince = 0;
	Vector3 from = Transform.Position;
 
	while ( timeSince < seconds )
	{
		var pos = Vector3.Lerp( from, to, easer( timeSince / seconds ) );
		Transform.Position = pos;
		await Task.Frame();
	}
	}

	[ActionGraphNode("StartLerp")]
	public void StartLerp(float seconds, Vector3 To)
	{
	 	_ = LerpPos( seconds, To, Easing.Linear );
	}
}
