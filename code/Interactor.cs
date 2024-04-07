using Sandbox;

public sealed class Interactor : Component
{
	public WorldPanel Panel { get; set; }
	protected override void OnUpdate()
	{
		Trace();
		if (Panel is not null)
		{
			if (Panel.Components.Get<InteractionHudPanel>() is not null) return;
			//var hudPanel = Panel.Components.Create<InteractionHudPanel>();
		}
	}

	void Trace()
	{
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 5000).WithoutTags("player").Run();
		{
			if (tr.Hit)
			{
				if (Panel is not null) return;
				tr.GameObject.Components.TryGet<Item>(out var item);
				var newGO = new GameObject();
				var panel = newGO.Components.Create<WorldPanel>();
				panel.LookAtCamera = true;
				panel.Transform.Position = tr.EndPosition + tr.Normal * 2;
				Panel = panel;
			}
			else
			{
				if (Panel is not null)
				{
					Panel.GameObject.Destroy();
					Panel = null;
				}
			}
		}
	}
}
