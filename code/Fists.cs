using Kicks;
using Sandbox;

public sealed class Fists : Component
{
	[Property] public SkinnedModelRenderer fists { get; set; }
	[Property] public GameObject ViewModelCamera { get; set; }
	public PlayerController playerController { get; set; }
	[Property] public int Damage { get; set; } = 10;

	protected override void OnStart()
	{
		fists.Set("b_attack", false);
		fists.Set("b_deploy", true);
		playerController = Scene.GetAllComponents<PlayerController>().FirstOrDefault( x => !x.IsProxy );
	}

	protected override void OnUpdate()
	{
		if (!IsProxy)
		{
			if (Input.Pressed("attack1"))
			{
				Attack();
			}
			UpdateAnimations();
		}
		if (IsProxy)
		{
			ViewModelCamera.Enabled = false;
		}
	}
	protected override void OnEnabled()
	{
		if (!IsProxy)
		{
			fists.Set("b_deploy", true);
		}
	}
	protected override void OnDisabled()
	{
		if (!IsProxy)
		{
			fists.Set("b_attack", false);
		}
	}
	void Attack()
	{
		fists.Set("b_attack", true);
		var ray = Scene.Camera.ScreenNormalToRay(0.5f);
		var tr = Scene.Trace.Ray(ray, 300).WithoutTags("player").Run();
		if (tr.Hit)
		{
			tr.GameObject.Components.TryGet<EnemyHealthComponent>( out var enemy, FindMode.EverythingInSelfAndParent );
			tr.GameObject.Components.TryGet<DamageTaker>( out var damageTaker, FindMode.EverythingInSelfAndParent );	
			if (enemy is not null)
			{
				enemy.Hurt(Damage, GameObject.Parent.Id);
			}
			else if (damageTaker is not null)
			{
				damageTaker.TakeDamage(Damage, GameObject.Parent.Id);
			}
		if ( tr.Body is not null )
		{
			tr.Body.ApplyImpulseAt( tr.HitPosition, tr.Direction * 200.0f * tr.Body.Mass.Clamp( 0, 200 ) );
		}
		var damage = new DamageInfo(Damage, GameObject, GameObject, tr.Hitbox);
		damage.Position = tr.HitPosition;
		damage.Shape = tr.Shape;
		foreach (var damageAble in tr.GameObject.Components.GetAll<IDamageable>())
		{
			damageAble.OnDamage(damage);
		}
		}
	}

	void UpdateAnimations()
	{
			if (Input.Pressed("jump") && !IsProxy)
			{
				fists.Set("b_jump", true);
			}

			if (!playerController.CharacterController.IsOnGround && !IsProxy)
			{
				fists.Set("b_grounded", false);
			}
			else
			{
				fists.Set("b_grounded", true);
			}
			fists.Set("move_groundspeed", playerController.CharacterController.Velocity.Length);
	}

	
}
