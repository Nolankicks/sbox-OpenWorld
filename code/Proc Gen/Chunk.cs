using Sandbox;

public sealed class Chunk : Component
{
	[Property] public GameObject player { get; set; }
	[Property] public ModelRenderer modelRenderer { get; set; }
	protected override void OnStart()
	{
		modelRenderer.Enabled = false;
	}
	protected override void OnUpdate()
	{
		if (Vector3.DistanceBetween(player.Transform.Position, Transform.Position) > 1500)
	{
			modelRenderer.Enabled = false;
	}
		else
	{
			modelRenderer.Enabled = true;
	}
	}
}
