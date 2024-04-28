using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Services;
using System;
using System.Threading;
public sealed class FootstepSoundsComponent : Component
{ 
	[Property] SkinnedModelRenderer Source { get; set; }

	[Property] bool StaticSound { get; set; }
	[Property, ShowIf("StaticSound", true)] SoundEvent FootstepSound { get; set; }
	protected override void OnEnabled()
	{
		if ( Source is null )
			return;

		Source.OnFootstepEvent += OnEvent;
	}

	protected override void OnDisabled()
	{
		if ( Source is null )
			return;

		Source.OnFootstepEvent -= OnEvent;
	}

	private void OnEvent( SceneModel.FootstepEvent e )
	{
		if ( !StaticSound )
			PhysicalFootstep(e);
		else
			Sound.Play( FootstepSound, Transform.Position );
	}

	void PhysicalFootstep( SceneModel.FootstepEvent e )
	{
		var tr = Scene.Trace
			.Ray( e.Transform.Position + Vector3.Up * 20, e.Transform.Position + Vector3.Up * -20 )
			.Run();

		if ( !tr.Hit )
			return;

		var snd = Sound.Play( tr.Surface.Sounds.FootLand, Transform.Position );
		snd.Volume = 0.5f;
	}
}
