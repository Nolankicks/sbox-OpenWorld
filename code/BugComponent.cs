using Sandbox;

public sealed class BugComponent : Component
{
	[Property] public GameObject GameObject1 { get; set; }
	[Property] public GameObject GameObject2 { get; set; }
	protected override void OnUpdate()
	{

	}
}
