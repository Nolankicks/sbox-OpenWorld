using Sandbox;

public sealed class PointsComponent : Component
{
	[Property, Sync] public int Points { get; set; }

	protected override void OnStart()
	{
		_ = SetPointValues();
	}


	public void AddPoints(int points)
	{
		if (IsProxy) return;
		Points += points;
	}

	public void RemovePoints(int points)
	{
		if (IsProxy) return;
		Points -= points;
	}

	public void ResetPoints()
	{
		if (IsProxy) return;
		Points = 0;
	}

	public async Task SetPointValues()
	{
		if (IsProxy) return;
		while (true)
		{
			try
			{
			Sandbox.Services.Stats.SetValue("mostpoints", Points);
			Log.Info("Send Point Values");
			await Task.DelayRealtimeSeconds(3);
			}
			catch (TaskCanceledException)
			{
				return;
			}
		}
	}
}
