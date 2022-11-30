using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ServerPlayerSettings", menuName = "Configs/ServerPlayerSettings")]
    public class ServerPlayerSettings : ScriptableObject
    {
        [field: SerializeField] public float PlayerMovementSpeedModifier { get; private set; } = 2000;
        [field: SerializeField] public float DashDestination { get; private set; } = 20;
        [field: SerializeField] public float DashMovementSpeed { get; private set; } = 5000;
        [field: SerializeField] public float JumpImpulse { get; private set; } = 600;
        [field: SerializeField] public float GravityForce { get; private set; } = -60;
        [field: SerializeField] public float DashMovementThreshold { get; private set; } = .1f;
        [field: SerializeField] public float RaycastGroundCheckLength { get; private set; } = .1f;
        [field: SerializeField] public float DashCD { get; private set; } = 1f;
        [field: SerializeField] public float PaintingDuration { get; private set; } = 3f;
        
    }
}
