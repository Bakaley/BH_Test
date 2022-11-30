using System;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    public class MainMenuPanel : MonoBehaviour
    {
        [SerializeField] private BHNetworkManager bhNetworkManager;
        [SerializeField] private Button _hostLobbyButton;
        [SerializeField] private Button _joinLobbyButton;
        [SerializeField] private Button _backButton;

        public event Action OnHostClicked;
        public event Action OnJoinClicked;
        public event Action OnBackClicked;
        
        private void OnEnable()
        {
            _hostLobbyButton.onClick.AddListener(HostLobbyClickHandler);
            _joinLobbyButton.onClick.AddListener(JoinLobbyClickHandler);
            _backButton.onClick.AddListener(BackClickedHandler);
        }

        private void HostLobbyClickHandler()
        {
            bhNetworkManager.StartHost();
            gameObject.SetActive(false);
            OnHostClicked?.Invoke();
        }
        
        private void JoinLobbyClickHandler()
        {
            gameObject.SetActive(false);
            OnJoinClicked?.Invoke();
        }
        
        private void BackClickedHandler()
        {
            gameObject.SetActive(false);
            OnBackClicked?.Invoke();
        }

        private void OnDisable()
        {
            _hostLobbyButton.onClick.RemoveListener(HostLobbyClickHandler);
            _joinLobbyButton.onClick.RemoveListener(JoinLobbyClickHandler);
            _backButton.onClick.RemoveListener(BackClickedHandler);
        }
    }
}
