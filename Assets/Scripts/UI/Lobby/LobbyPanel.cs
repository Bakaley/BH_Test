using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    public class LobbyPanel : NetworkBehaviour
    {
        [SerializeField] private BHNetworkManager _bhNetworkManager;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private int _maxGridSize = 8;
        [SerializeField] private PlayerUIPlaceholder _playerUIPlaceholderSampler;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private TextMeshProUGUI _statusString;

        private PlayerNetworkEntity _localPlayer;

        private Dictionary<PlayerUIPlaceholder, PlayerNetworkEntity> _playerUIPlaceholders = new();

        public event Action ToMainMenu;

        [TargetRpc]
        public void RpcInit(NetworkConnection target, List<PlayerNetworkEntity> players)
        {
            ClearTable();
            foreach (var player in players)
            {
                AddPlayer(player);
            }
            _localPlayer = NetworkClient.localPlayer.GetComponent<PlayerNetworkEntity>();
            if(_localPlayer.HasHostRights) _startGameButton.gameObject.SetActive(true);
            else _startGameButton.gameObject.SetActive(false);
            _startGameButton.interactable = false;
            _localPlayer.OnReadyStateChanged += OnPlayerStateChanged;
        }
    
        [ClientRpc]
        public void RpcSyncPlayerLists(List<PlayerNetworkEntity> players)
        {
            ClearTable();
            foreach (var player in players)
            {
                AddPlayer(player);
            }
            if(_localPlayer.HasHostRights) CmdRefreshStartGameButton();
        }
        
        public void Disconnect()
        { 
            if(_localPlayer) _localPlayer.OnReadyStateChanged -= OnPlayerStateChanged;
            gameObject.SetActive(false);
            ToMainMenu?.Invoke();
            ClearTable();
        }
        
        private void Awake()
        {
            for (int i = 0; i < _maxGridSize; i++)
            {
                var placeholder = Instantiate(_playerUIPlaceholderSampler, _grid.transform);
                _playerUIPlaceholders.Add(placeholder, null);
            }
        }
    
        private void OnEnable()
        {
            _readyButton.onClick.AddListener(ReadyClickHandler);
            _startGameButton.onClick.AddListener(StartGameClickHandler);
            _exitButton.onClick.AddListener(ExitClickHandler);
        }

        private void ExitClickHandler()
        {
            if (isServer && isClient) NetworkManager.singleton.StopHost();
            else NetworkManager.singleton.StopClient();

            Disconnect();
        }
    
        private void ReadyClickHandler()
        {
            _localPlayer.CmdSwitchStateReady(!_localPlayer.Ready);
        }
    
        private void StartGameClickHandler()
        {
            _bhNetworkManager.StartGame();
        }

        private void OnPlayerStateChanged(bool newValue)
        {
            CmdRefreshStartGameButton();
        }
    
        [Command(requiresAuthority = false)]
        private void CmdRefreshStartGameButton()
        {
            string newStatus;
            bool result = _bhNetworkManager.CheckReadyToStartGame(out newStatus);
            RpcRefreshLobbyStatus(result, newStatus);
        }

        [ClientRpc]
        private void RpcRefreshLobbyStatus(bool ready, string newStatus)
        {
            if(_localPlayer.HasHostRights) _startGameButton.interactable = ready;
            _statusString.text = newStatus;
        }

        private void ClearTable()
        {
            _playerUIPlaceholders.Keys.ToList().ForEach(key => key.SetPlayer(null));
            _playerUIPlaceholders.Keys.ToList().ForEach(key => _playerUIPlaceholders[key] = null);
        }

        private void AddPlayer(PlayerNetworkEntity player)
        {
            PlayerUIPlaceholder placeholder = FindFirstEmpty();
            placeholder.SetPlayer(player);
            _playerUIPlaceholders[placeholder] = player;
        }
    
        private PlayerUIPlaceholder FindFirstEmpty()
        {
            foreach (var pair in _playerUIPlaceholders)
            {
                if (pair.Value == null) return pair.Key;
            }
            return null;
        }

        private void OnDisable()
        {
            _readyButton.onClick.RemoveListener(ReadyClickHandler);
            _startGameButton.onClick.RemoveListener(StartGameClickHandler);
            _exitButton.onClick.RemoveListener(ExitClickHandler);
        }
    }
}
