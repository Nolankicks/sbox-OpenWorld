using Sandbox;

public sealed class GameManager : Component
{
	protected override void OnStart()
	{
		
	}

	public void ChangeScene(SceneFile scene)
	{
		Game.ActiveScene.Load(scene);
	}
}
