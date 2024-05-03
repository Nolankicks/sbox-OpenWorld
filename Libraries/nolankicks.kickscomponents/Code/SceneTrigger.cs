using Sandbox;

public sealed class SceneTrigger : Component, Component.ITriggerListener
{
	[Property] public SceneFile SceneFile { get; set; }
	protected override void OnUpdate()
	{

	}

	void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
	{
		if (other.GameObject.Parent.Tags.Has("player") || other.GameObject.Tags.Has("boat"))
		{
			Game.ActiveScene.Load(SceneFile);
		}
	}

	void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
	{

	}
}
