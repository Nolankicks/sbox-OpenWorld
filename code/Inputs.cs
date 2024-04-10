using System;

namespace YourNamespace
{
	public enum Inputs
	{
		E,
		V,
		G,
		C,
	}

	public class InputHandler
	{
		public static string GetInputString( Inputs inputs )
		{
			switch (inputs)
			{
				case Inputs.E:
					return "use";
				case Inputs.V:
					return "voice";
				case Inputs.G:
					return "drop";
				case Inputs.C:
					return "View";
				default:
					return "use";
			}
		}
	}
}
