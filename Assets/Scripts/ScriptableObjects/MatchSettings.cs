using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MatchSettings", menuName = "Configs/MatchSettings")]
    public class MatchSettings : ScriptableObject
    {
        [field: SerializeField] public int PointsToWin { get; private set; } = 3;
        [field: SerializeField] public int SecondsBetweenMatches { get; private set; } = 5;
        [field: SerializeField] public int SecondsBeforeMatchStart { get; private set; } = 3;

    }
}
