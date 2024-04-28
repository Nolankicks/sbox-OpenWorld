using Sandbox;
using System.Collections.Generic;
using System.Linq;

[Title( "Citizen Customisation" )]
public sealed class CitizenCustomisationComponent : Component
{
	[Property] SkinnedModelRenderer body { get; set; }

	[Property] bool UseClientClothes { get; set; } = true;

	[Property] [HideIf("UseClientClothes", true)] List<Clothing> Clothes { get; set; }
	protected override void OnStart()
	{
		if (body == null)
		{
			if ( Components.TryGet<SkinnedModelRenderer>( out var test ) )
			{
				body = test;
			}
			else
				return;
		}

		if ( UseClientClothes )
			LocalClothes();
		else
			ListClothes();
	}

	void LocalClothes()
	{
		var clothes = ClothingContainer.CreateFromLocalUser();
		clothes.Apply( body );
	}

	void ListClothes()
	{
		var container = new ClothingContainer();
		foreach ( var item in Clothes )
		{
			container.Clothing.Add( ( new ClothingContainer.ClothingEntry(item) ));
		}
		container.Apply( body );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		foreach(var item in Clothes )
		{
			Gizmo.Draw.Model(item.Model);
		}
	}

}
