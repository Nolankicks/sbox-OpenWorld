
using Editor;
using Kicks;
using Sandbox;
[CustomEditor( typeof( ShopItems ) )]
public class ShopItemsControlWidget : ControlWidget
{
	public ShopItemsControlWidget( SerializedProperty property ) : base( property )
	{
		Layout = Layout.Column();
		PaintBackground = false;
		if ( property.IsNull )
		{
			property.SetValue( new ShopItems() );
		}
		var obj = property.GetValue<ShopItems>()?.GetSerialized();
		var controlSheet = new ControlSheet();
		controlSheet.AddObject( obj );
		Layout.Add( controlSheet );
	}
}
