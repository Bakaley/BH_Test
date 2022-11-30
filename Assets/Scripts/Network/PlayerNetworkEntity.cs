using System;
using Mirror;
using UI;
using UI.Lobby;

namespace Network
{
    public class PlayerNetworkEntity : NetworkBehaviour
    {
        [SyncVar (hook =  nameof(NameChangeHandler))]
        private string _playerName;
        [SyncVar(hook = nameof(IsReadyChangeHandler))]
        private bool _isReady;
        
        public string PlayerName => _playerName;
        public bool HasHostRights { get; set; } = false;
        public bool Ready => _isReady;

        public event Action<bool> OnReadyStateChanged;
        public event Action<string> OnNameChanged;
        
        public override void OnStartAuthority()
        {
            CmdSetPlayerName(EnterNamePanel.DisplayName);
        }

        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        [Command]
        public void CmdSwitchStateReady(bool newState)
        {
            _isReady = newState;
        }
        
        private void IsReadyChangeHandler(bool oldValue, bool newValue)
        {
            OnReadyStateChanged?.Invoke(newValue);
        }

        private void NameChangeHandler(string oldValue, string newValue)
        {
            OnNameChanged?.Invoke(newValue);
        }
        
        [Command]
        private void CmdSetPlayerName(string newName)
        {
            _playerName = newName;
        }
    }
}
