using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "LocalPlayerSettings", menuName = "Configs/LocalPlayerSettings")]
    public class LocalPlayerSettings : ScriptableObject
    {
        [field: SerializeField] public float MouseSensitivity { get; private set; } = 1;
        [field: SerializeField] public float CameraDampingModifier { get; private set; } = 0.35f;
    }
}
