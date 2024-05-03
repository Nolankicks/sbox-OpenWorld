using Sandbox;
using System;
using System.Collections.Generic;
[Icon("shuffle")]
public sealed class RandomModel : Component
{
	[Property] public List<Model> RandomModels { get; set; }
	protected override void OnStart()
	{
		if (RandomModels.Count == 0) return;
		Components.TryGet<ModelRenderer>(out var modelRenderer, FindMode.InSelf);
		Components.TryGet<ModelCollider>(out var modelCollider, FindMode.InSelf);
		var selectedModel = Game.Random.FromList(RandomModels);
		if (selectedModel is null) return;
		if (modelRenderer is not null)
		{
			modelRenderer.Model = selectedModel;
		}
		if (modelCollider is not null)
		{
			modelCollider.Model = selectedModel;
		}
	}
}
