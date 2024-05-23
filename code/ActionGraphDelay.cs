using Sandbox;
[Icon("timer")]
public sealed class ActionGraphDelay : Component
{
	[Property] public bool Delay { get; set; } = false;
	protected override void OnUpdate()
	{

	}

	public void SetDelay(float delay)
	{
		_ = DelayAction(delay);
	}

	async Task DelayAction(float delay)
	{
		try
		{
		Delay = true;
		await Task.DelaySeconds(delay);
		Delay = false;
		}
		catch (TaskCanceledException)
		{
			return;
		}

	}
}
