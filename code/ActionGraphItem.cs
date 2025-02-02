using System.Diagnostics;
using Kicks;
using Sandbox;
using Sandbox.Network;
using Sandbox.UI;
using Sandbox.Utility;
[Icon( "directions_walk" )]
public sealed class ActionGraphItem : Component
{
	[Property, Sync] public bool AbleToDrop { get; set; } = true;
	[Property] public GameObject DropppedItem { get; set; }
	public bool Able { get; set; } = true;
	[Property, Sync] public bool InInventory { get; set; }
	[Property] public GameObject Object { get; set; }
	public delegate void ActionDel( Kicks.PlayerController playerController, Inventory inventory, AmmoContainer ammoContainer, ActionGraphItem actionGraphItem );
	[Property, Description( "Called when the item is in the inventory" )] public ActionDel OnUse { get; set; }
	[Property] public ActionDel OnPickUp { get; set; }
	[Property] public ActionDel OnDrop { get; set; }
	public Inventory Inventory { get; set; }
	public AmmoContainer AmmoContainer { get; set; }
	public Kicks.PlayerController PlayerController { get; set; }
	[Property] public bool UsesAmmo { get; set; }
	[Property, ShowIf( "UsesAmmo", true ), Sync] public int Ammo { get; set; }
	public bool AbleToFire { get; set; }
	[Property, ShowIf( "UsesAmmo", true ), Sync] public int MaxAmmo { get; set; }
	[Property] public AmmoContainer.AmmoTypes AmmoType { get; set; }
	public int ShotsFired { get; set; }

	protected override void OnStart()
	{
		var allObjects = GameObject.GetAllObjects( false );
		foreach ( var gb in allObjects )
		{
			gb.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		}
		PlayerController = Scene.GetAllComponents<Kicks.PlayerController>().FirstOrDefault( x => !x.IsProxy );
		Inventory = Scene.GetAllComponents<Inventory>().FirstOrDefault( x => !x.IsProxy );
		AmmoContainer = Scene.GetAllComponents<AmmoContainer>().FirstOrDefault( x => !x.IsProxy );
	}
	[ActionGraphNode( "SetAdsAnims" )]
	public static void SetAdsAnims( SkinnedModelRenderer ViewModelGun, CameraComponent camera, bool IsAiming )
	{
		if ( IsAiming )
		{
			Log.Info( "Aiming" );
		}
		ViewModelGun.Set( "ironsights", IsAiming ? 2 : 0 );
		ViewModelGun.Set( "ironsights_fire_scale", IsAiming ? 0.2f : 0f );
	}
	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			if ( InInventory )
			{
				Use();
				foreach ( var gb in Object?.GetAllObjects( false ) )
				{
					if ( gb is null ) return;
					gb.Enabled = true;
				}
				foreach ( var gb in DropppedItem?.GetAllObjects( false ) )
				{
					if ( gb is null ) return;
					gb.Enabled = false;
				}
				Components.TryGet<PopupUi>( out var popupUi, FindMode.EverythingInSelfAndDescendants );
				if ( popupUi is not null )
				{
					popupUi.ShowPopUp = false;
				}
			}
			else
			{
				foreach ( var gb in Object?.GetAllObjects( false ) )
				{
					gb.Enabled = false;
				}
				foreach ( var gb in DropppedItem?.GetAllObjects( false ) )
				{
					gb.Enabled = true;
				}
				Components.TryGet<PopupUi>( out var popupUi, FindMode.EverythingInSelfAndDescendants );
				if ( popupUi is not null )
				{
					popupUi.ShowPopUp = true;
				}
			}

		}
		else
		{
			if ( InInventory )
			{
				foreach ( var gb in DropppedItem?.GetAllObjects( false ) )
				{
					gb.Enabled = false;
				}
			}
			else
			{
				foreach ( var gb in DropppedItem?.GetAllObjects( false ) )
				{
					gb.Enabled = true;
				}
			}
			foreach ( var gb in Object?.GetAllObjects( false ) )
			{
				gb.Enabled = false;
			}
		}
	}

	public void DropItem( Inventory inventory )
	{
		if ( !AbleToDrop ) return;
		var allObjects = GameObject.GetAllObjects( false );
		var tr = Scene.Trace.Ray( Scene.Camera.ScreenNormalToRay( 0.5f ), 50 ).WithoutTags( "player" ).Run();
		GameObject.Parent = null;
		inventory.RemoveItem( GameObject, false );
		//Idk if I need to refresh this shit but I will anyway 🤓☝️
		Object.Transform.LocalPosition = Vector3.Zero;
		GameObject.Transform.Position = tr.EndPosition + Vector3.Up * 15;
		InInventory = false;
		OnDrop?.Invoke( PlayerController, inventory, AmmoContainer, this );
		if ( GameNetworkSystem.IsActive )
		{
			foreach ( var gb in allObjects )
			{
				gb.Network.DropOwnership();
			}
		}
	}
	public void PickUp( Inventory inventory )
	{
		var allObjects = GameObject.GetAllObjects( false );
		if ( GameNetworkSystem.IsActive )
		{
			foreach ( var gb in allObjects )
			{
				gb.Network.TakeOwnership();
			}
			OnPickUp?.Invoke( PlayerController, inventory, AmmoContainer, this );
		}

		GameObject.Parent = Inventory.GameObject;
		GameObject.Transform.LocalPosition = Vector3.Zero;
		GameObject.Transform.LocalRotation = Rotation.Identity;
		InInventory = true;
		inventory.AddItem( GameObject, inventory.GetNextSlot(), false );
		Network.Refresh();

	}
	public void Use()
	{
		OnUse?.Invoke( PlayerController, Inventory, AmmoContainer, this );
	}
	public float GetRandomFloat()
	{
		return Random.Shared.Float( -1, 1 );
	}
	public TimeSince timeSinceFire = 1000;

	public async Task AwaitFire( float Time )
	{
		AbleToFire = false;
		await GameTask.DelaySeconds( Time );
		AbleToFire = true;
	}

	[ActionGraphNode( "ActionGraphTrace" ), Impure, Icon( "sports_handball" )]
	public void ActionGraphTrace( int Damage, float Spread, int TraceLength, out bool TraceHit, out GameObject gameObject, out Vector3 hitPos, out Vector3 traceNormal, out Vector3 traceStartPos, float Recoil, float FireRate, out bool AbleToFire, bool RefreshTimeSince = true )
	{
		if ( (Ammo > 0 || !UsesAmmo) && !IsProxy && timeSinceFire >= FireRate )
		{
			AbleToFire = true;
			if ( RefreshTimeSince )
			{
				timeSinceFire = 0;
			}
			var ray = Game.ActiveScene.Camera.ScreenNormalToRay( 0.5f );
			ray.Forward += Vector3.Random * Spread;
			var tr = Scene.Trace.Ray( ray, TraceLength ).WithoutTags( Steam.SteamId.ToString() ).Run();
			PlayerController.eyeAngles += new Angles( -Recoil, GetRandomFloat(), 0 );
			if ( tr.Hit )
			{
				TraceHit = true;
				Ammo--;
				ShotsFired++;
				tr.GameObject.Components.TryGet<EnemyHealthComponent>( out var dummy, FindMode.EverythingInSelfAndParent );
				tr.GameObject.Components.TryGet<Kicks.PlayerController>( out var player, FindMode.EverythingInSelfAndParent );
				var damageTaker = tr.GameObject.Components.Get<DamageTaker>( FindMode.EverythingInSelfAndParent );
				traceStartPos = tr.StartPosition;
				hitPos = tr.HitPosition;
				gameObject = tr.GameObject;
				traceNormal = tr.Normal;
				if ( dummy is not null )
				{
					dummy.Hurt( Damage, GameObject.Parent.Id );
				}
				else if ( player is not null )
				{
					player.TakeDamage( Damage, false );
				}
				if ( damageTaker is not null )
				{
					damageTaker.TakeDamage( Damage, GameObject.Parent.Id );
				}

				if ( tr.Surface is null )
				{
					Log.Info( "Surface is null" );
				}
				else
				{
					var surfaceSound = tr.Surface.PlayCollisionSound( tr.HitPosition );
					if ( surfaceSound is null )
					{
						Log.Info( "surfaceSound is null" );
					}
					else
					{
						surfaceSound.Volume = 1;
					}
				}
				if ( tr.Body is not null )
				{
					tr.Body.ApplyImpulseAt( tr.HitPosition, tr.Direction * 200.0f * tr.Body.Mass.Clamp( 0, 200 ) );
				}
				var damage = new DamageInfo( Damage, GameObject, GameObject, tr.Hitbox );
				damage.Position = tr.HitPosition;
				damage.Shape = tr.Shape;
				foreach ( var damageAble in tr.GameObject.Components.GetAll<IDamageable>() )
				{
					damageAble.OnDamage( damage );
				}
			}
			else
			{
				TraceHit = false;
				gameObject = null;
				hitPos = Vector3.Zero;
				traceNormal = Vector3.Zero;
				traceStartPos = Vector3.Zero;
			}
		}
		else
		{
			TraceHit = false;
			Log.Info( "No Ammo" );
			gameObject = null;
			hitPos = Vector3.Zero;
			traceNormal = Vector3.Zero;
			AbleToFire = false;
			traceStartPos = Vector3.Zero;
		}

	}

	[ActionGraphNode( "ActionGraphReload" ), Icon( "replay" )]
	public void ActionGraphReload( AmmoContainer.AmmoTypes ammoType, AmmoContainer ammoContainer, int StartingAmmo )
	{
		if ( !UsesAmmo ) return;
		var selectedAmmo = ammoContainer.GetAmmo( ammoType );
		if ( Input.Pressed( "reload" ) && MaxAmmo != 0 && ShotsFired != 0 && !IsProxy && selectedAmmo > 0 && UsesAmmo )
		{
			var ammoToSet = selectedAmmo - ShotsFired;
			if ( ammoToSet > selectedAmmo )
			{
				Ammo = selectedAmmo;
				AmmoContainer.SetAmmo( ammoType, 0 );
			}
			else
			{
				AmmoContainer.SetAmmo( ammoType, ammoToSet );
				Ammo = StartingAmmo;
			}
			ShotsFired = 0;
		}
	}

	[ActionGraphNode( "SpawnMuzzleFlash" )]
	public void SpawnMuzzleFlash( SkinnedModelRenderer Gun, GameObject MuzzleFlash )
	{
		if ( MuzzleFlash is not null )
		{
			var muzzle = Gun.GetAttachment( "muzzle" );
			if ( muzzle is not null )
			{
				var MuzzleFlashInstance = MuzzleFlash.Clone( muzzle.Value.Position, muzzle.Value.Rotation );
				MuzzleFlashInstance.Tags.Add( "viewmodel" );
			}
		}
	}

	[ActionGraphNode( "Set ironsights" )]
	public void SetIronSights( SkinnedModelRenderer Gun )
	{
		var IsAiming = Input.Down( "attack2" );
		Gun.Set( "ironsights", IsAiming ? 2 : 0 );
		Gun.Set( "ironsights_fire_scale", IsAiming ? 0.2f : 0f );
	}

	[ActionGraphNode( "Play Sound" )]
	public void PlaySound( SoundEvent FireSound, Vector3 startPos )
	{
		if ( FireSound is not null )
		{
			Sound.Play( FireSound, startPos );
		}
	}
}
