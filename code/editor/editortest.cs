using Sandbox;
using Editor;
using static Editor.Button;
namespace Kicks;
[CustomEditor( typeof( TestStruct ) )]
public class TestEditor : ControlWidget
{
	public Color HighlightColor = Theme.Yellow;
	public TestEditor( SerializedProperty property ) : base( property )
	{
		SetSizeMode( SizeMode.Default, SizeMode.Default );

		Layout = Layout.Column();
		Layout.Spacing = 2;
		Cursor = CursorShape.Finger;
	}


	protected override void PaintControl()
	{var rect = LocalRect.Shrink( 8, 0 );
		var alpha = Paint.HasMouseOver ? 1f : 0.7f;

		Paint.SetBrush( Theme.ButtonDefault.Darken( 0.2f ) );
		Paint.DrawRect( LocalRect, 2 );
		Paint.SetPen( Theme.ControlText.WithAlphaMultiplied( alpha ) );
		var r = Paint.DrawIcon( rect, "track_changes", 17, TextFlag.LeftCenter );
		rect.Left += r.Width + 8;
		Paint.SetPen( Theme.ControlText.WithAlphaMultiplied( alpha ) );
		Paint.DrawText( rect, "OpenEditor", TextFlag.LeftCenter );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LeftMouseButton )
		{
			var editor = new Widget();
			editor.WindowTitle = "Test Editor";
			editor.Visible = true;
			editor.Size = new Vector2( 400, 400 );
			editor.ConstrainToScreen();
			var test = new TestWidget( editor );

		}
	}
}

public class TestWidget : GraphicsView
{
	private SceneObject _obj;

	private SceneCamera _camera;
	private SceneLight _light;
	public TestWidget( Widget parent ) : base( parent )
	{
		
		Layout = Layout.Column();
		Layout.Margin = 10;
		{
			var button = Layout.Add( new global::Editor.Button( "Close" ) );
		}
	}
}
