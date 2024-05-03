using Sandbox;
using System;
[Icon("shuffle")]
public sealed class RandomMaterialGroup : Component
{
	public ModelRenderer Renderer { get; set; }
	protected override void OnStart()
	{
		Components.TryGet<ModelRenderer>(out var modelRenderer, FindMode.InSelf);
		if (modelRenderer is not null)
		{
			Renderer = modelRenderer;
		}
		if (Renderer is not null)
		{
			SetRandomMaterial(Renderer);
		}
	}
	
	public void SetRandomMaterial(ModelRenderer renderer)
	{
		if (renderer is null) return;
		var materialsCount = renderer.Model.MaterialGroupCount;
		if (materialsCount == 0) return;
		var randomMaterialIndex = Random.Shared.Int(0, materialsCount - 1);
		var materialGroup = renderer.Model.GetMaterialGroupName(randomMaterialIndex);
		renderer.MaterialGroup = materialGroup;
	}
}
