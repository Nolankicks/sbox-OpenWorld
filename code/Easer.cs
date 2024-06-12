using Sandbox;
using Sandbox.Utility;

public sealed class Easer : Component
{
	public enum EasingEnum
	{
		BounceIn,
		BounceInOut,
		EaseIn,
		EaseInOut,
		EaseOut,
		ExpoIn,
		ExpoInOut,
		ExpoOut,
		QuadraticIn,
		QuadraticInOut,
		QuadraticOut,
		SineEaseIn,
		SineEaseInOut,
		SineEaseOut,
		Linear,
	}


	[ActionGraphNode("LerpPos"), Pure]
	async Task LerpPos( float seconds, Vector3 to, Func<float, float> easer )
	{
		TimeSince timeSince = 0;
		Vector3 from = Transform.Position;
	 
		while ( timeSince < seconds )
		{
			var pos = Vector3.Lerp( from, to, easer( timeSince / seconds ) );
			Transform.Position = pos;
			await Task.Frame();
		}
	}

	[ActionGraphNode("StartLerp")]
	public void StartLerp(float seconds, Vector3 To, EasingEnum easingEnum = EasingEnum.Linear)
	{
	 	_ = LerpPos( seconds, To, GetEasing(easingEnum) );
	}

public Func<float, float> GetEasing(EasingEnum easingEnum)
{
    switch (easingEnum)
    {
        case EasingEnum.BounceIn:
            return Easing.BounceIn;
        case EasingEnum.BounceInOut:
            return Easing.BounceInOut;
        case EasingEnum.EaseIn:
            return Easing.EaseIn;
        case EasingEnum.EaseInOut:
            return Easing.EaseInOut;
        case EasingEnum.EaseOut:
            return Easing.EaseOut;
        case EasingEnum.ExpoIn:
            return Easing.ExpoIn;
        case EasingEnum.ExpoInOut:
            return Easing.ExpoInOut;
        case EasingEnum.ExpoOut:
            return Easing.ExpoOut;
        case EasingEnum.QuadraticIn:
            return Easing.QuadraticIn;
        case EasingEnum.QuadraticInOut:
            return Easing.QuadraticInOut;
        case EasingEnum.QuadraticOut:
            return Easing.QuadraticOut;
        case EasingEnum.SineEaseIn:
            return Easing.SineEaseIn;
        case EasingEnum.SineEaseInOut:
            return Easing.SineEaseInOut;
        case EasingEnum.SineEaseOut:
            return Easing.SineEaseOut;
		case EasingEnum.Linear:
			return Easing.Linear;
        default:
            return Easing.Linear;
    }
}
	
}
