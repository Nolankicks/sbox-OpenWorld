using Sandbox;

public sealed class ElevatorTrigger : Component, Component.ITriggerListener
{
    [Property] public GameObject Elevator { get; set; }
    private Vector3 lastPosition;
    private Vector3 elevatorVelocity;
    private CharacterController Player;

    protected override void OnUpdate()
    {
        var currentPosition = Elevator.Transform.World.Position;
        elevatorVelocity = (currentPosition - lastPosition) / Time.Delta;
        lastPosition = currentPosition;
        if (Player is not null)
        {
            var newZ = Player.Transform.Position.z + elevatorVelocity.z * Time.Delta;

            Player.GameObject.Transform.Position = new Vector3(Player.Transform.Position.x, Player.Transform.Position.y, newZ);
        }
    }

    void ITriggerListener.OnTriggerEnter(Sandbox.Collider other)
    {
        if (other.GameObject.Components.TryGet<CharacterController>(out var player, FindMode.EverythingInSelfAndParent))
        {
            Player = player;
        }
    }

    void ITriggerListener.OnTriggerExit(Sandbox.Collider other)
    {
        if (other.GameObject.Tags.Has("elevator")) return;
        Log.Info("exit");
        if (other.GameObject.Components.TryGet<CharacterController>(out var player, FindMode.EverythingInSelfAndParent))
        {
            Player = null;
        }
    }
}
