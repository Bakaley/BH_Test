using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    public class PlayerUIPlaceholder : MonoBehaviour
    {
        [SerializeField] private Image _checkImage;
        [SerializeField] private TextMeshProUGUI _playerNameField;

        private readonly string _defaultText = "Waiting for players";
        private PlayerNetworkEntity _player;
    
        public void SetPlayer(PlayerNetworkEntity player)
        {
            if (player == null)
            {
                if (_player)
                {
                    _player.OnNameChanged -= PlayerNameChangedHandler;
                    _player.OnReadyStateChanged -= PlayerReadyStateChangedHandler;
                }
                SetReady(false);
                _player = null;
                ClearName();
            }
            else
            {
                _player = player;
                _player.OnNameChanged += PlayerNameChangedHandler;
                _player.OnReadyStateChanged += PlayerReadyStateChangedHandler;
                SetReady(_player.Ready);
                FillName(_player.PlayerName);
            }
        }

        private void FillName(string playerName)
        {
            _playerNameField.text = playerName;
        }

        private void ClearName()
        {
            _playerNameField.text = _defaultText;
        }
    
        private void PlayerReadyStateChangedHandler(bool newState)
        {
            SetReady(newState);
        }

        private void SetReady(bool newState)
        {
            _checkImage.gameObject.SetActive(newState);
        }

        private void PlayerNameChangedHandler(string name)
        {
            FillName(name);
        }
    }
}
