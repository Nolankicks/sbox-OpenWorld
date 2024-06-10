using Sandbox;
using Sandbox.Utility;

public sealed class DoorComponent : Component
{
	public Angles TargetRotation { get; set; } = new Angles(0, 90, 0);	public Dictionary<GameObject, Vector3> PreviousPositions { get; set; } = new Dictionary<GameObject, Vector3>();
	public Vector3 Velocity { get; set; }
	[Property] public bool negativeAngles { get; set; } = false;
	public async Task LerpDoorTask(float seconds, Angles targetRotation, Easing.Function easer)
	{
		TimeSince timeSinceLerp = 0;
		Angles startRotation = Transform.Rotation.Angles();

		while (timeSinceLerp < seconds)
		{
			var newAngles = Angles.Lerp(startRotation, targetRotation, easer(timeSinceLerp / seconds));
			Transform.Rotation = newAngles.ToRotation();
			await Task.Frame();
		}
		var newRot = TargetRotation;
		if (negativeAngles)
		{
			newRot.yaw = newRot.yaw == -90 ? 0 : -90;
		}
		else
		{
			newRot.yaw = newRot.yaw == 90 ? 0 : 90;
		}
		TargetRotation = newRot;
	}

	public void LerpDoor()
	{
		_ = LerpDoorTask(1.0f, TargetRotation, Easing.ExpoOut);
	}

}
