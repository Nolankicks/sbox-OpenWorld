using Sandbox;
[Icon("timer")]
public sealed class ActionGraphDelay : Component
{
	[Property] public bool Delay { get; set; } = false;
	protected override void OnUpdate()
	{

	}

	public async void SetDelay(float delay)
	{
		await DelayAction(delay);
	}

	async Task DelayAction(float delay)
	{
		Delay = true;
		await GameTask.DelaySeconds(delay);
		Delay = false;
		}
}
