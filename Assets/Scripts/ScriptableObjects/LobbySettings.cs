using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "LobbySettings", menuName = "Configs/LobbySettings")]
    public class LobbySettings : ScriptableObject
    {
        [field: SerializeField] public int MinPlayers { get; private set; } = 2;
        [field: SerializeField] public string NotEnoughPlayers { get; private set; } = "Для начала игры необходимо как минимум 2 игрока";
        [field: SerializeField] public string PlayersHaveSameName { get; private set; } = "Игроки не могут иметь одинаковые имена, матч не начнётся!";
        [field: SerializeField] public string PlayersAreNotReady { get; private set; } = "Для начала игры все игроки должны быть готовы";
        [field: SerializeField] public string Ready { get; private set; } = "Можно начинать";

    }
}
