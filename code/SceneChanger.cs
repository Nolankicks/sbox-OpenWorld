using Sandbox;

public sealed class SceneChanger : Component
{
	protected override void OnUpdate()
	{

	}

	public void ChangeScene(SceneFile sceneFile)
	{
		Game.ActiveScene.Load(sceneFile);
	}
}
