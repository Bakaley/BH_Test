using InGame;
using TMPro;
using UnityEngine;

namespace UI.InGame
{
    public class TableRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerName;
        [SerializeField] private TextMeshProUGUI _playerScore;

        public void Init(string playerName)
        {
            _playerName.text = playerName;
            _playerScore.text  = 0.ToString();
        }
        
        public void RefreshScore(int newValue)
        {
            _playerScore.text = newValue.ToString();
        }
    }
}
