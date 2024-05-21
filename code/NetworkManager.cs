using Sandbox.Network;
using System;
using System.Threading.Tasks;
namespace Kicks;
public sealed class NetworkManager : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;
	[Property] public bool ManualSpawns { get; set; } = false;
	[Property, ShowIf("ManualSpawns", true)] public List<SpawnPoint> SpawnPoints { get; set; } = new();

	/// <summary>
	/// The prefab to spawn for the player to control.
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		if ( Scene.IsEditor )
			return;

		if ( StartServer && !GameNetworkSystem.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			GameNetworkSystem.CreateLobby();
		}
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

		if ( PlayerPrefab is null )
			return;

		//
		// Find a spawn location for this player
		//
		if ( ManualSpawns)
		{
			if (SpawnPoints.Count == 0)
			{
				Log.Error( "No spawn points available" );
				return;
			}
			else
			{
				var selectedPoint = Game.Random.FromList( SpawnPoints );
				var spawn = selectedPoint.Transform.World.WithScale( 1 );
				var player = PlayerPrefab.Clone( spawn, name: $"Player - {channel.DisplayName}" );
				player.Components.TryGet<PlayerController>( out var playerVar );
				if (playerVar is not null)
				{
					playerVar.eyeAngles = spawn.Rotation.Angles();
				}
				player.NetworkSpawn( channel );
			}
		}
		else
		{
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var player = PlayerPrefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );
		player.Components.TryGet<PlayerController>( out var playerVar );
		if (playerVar is not null)
		{
			playerVar.eyeAngles = startLocation.Rotation.Angles();
		}
		player.NetworkSpawn( channel );
		}
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		var spawns = Scene.GetAllComponents<SpawnPoint>().ToList();
		if ( spawns.Count == 0 )
		{
			return Transform.World;
		}
		else
		{
			var selectedPoint = Game.Random.FromList( spawns );
			return selectedPoint.Transform.World;
		}
	}
}
