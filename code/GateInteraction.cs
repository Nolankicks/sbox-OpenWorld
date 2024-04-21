using Kicks;
using Sandbox;

public sealed class GateInteraction : Component
{
	[Property] public GameObject LeftGate { get; set; }
	[Property] public GameObject RightGate { get; set; }
	[Property] public GameObject Boat { get; set; }
	[Property] public Vector3 MoveAngles { get; set; }
	[Property] public CharacterController Controller { get; set; }
	protected override void OnUpdate()
	{

	}

	public void OpenGate()
	{
		LeftGate.Transform.Rotation = new Angles(0, -90, 0).ToRotation();
		RightGate.Transform.Rotation = new Angles(0, -90, 0).ToRotation();
	}

	public void CloseGate()
	{
		LeftGate.Transform.Rotation = new Angles(0, 180, 0).ToRotation();
		RightGate.Transform.Rotation = new Angles(0, 0, 0).ToRotation();
	
	}

	public void TransformCamera()
	{
		Scene.Camera.Transform.Rotation = new Angles(0, -180, 0).ToRotation();
		Scene.Camera.Transform.Position -= new Vector3(0, 0, 30);
	}

}
