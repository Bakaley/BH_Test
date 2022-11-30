using System;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    public class IPAddressPanel : MonoBehaviour
    {
        [SerializeField] private BHNetworkManager bhNetworkManager;
        [SerializeField] private Button _connectButton;
        [SerializeField] private TMP_InputField _ipField;
        [SerializeField] private Button _backButton;

        private readonly string localHostString = "localhost";
        public event Action OnConnectionComplete;
        public event Action OnBackClicked;

        private void Awake()
        {
            _ipField.text = localHostString;
        }

        private void OnEnable()
        {
            _connectButton.onClick.AddListener(ConnectClickHandler);
            _backButton.onClick.AddListener(BackClickedHandler);
            bhNetworkManager.OnClientConnected += ClientConnectedHandler;
            bhNetworkManager.OnClientDisconnected += ClientDisconnectedHandler;
        }

        private void ClientConnectedHandler()
        {
            gameObject.SetActive(false);
            _connectButton.interactable = true;
            _backButton.interactable = true;
            OnConnectionComplete?.Invoke();
        }
        
        private void ClientDisconnectedHandler()
        {
            _connectButton.interactable = true;
            _backButton.interactable = true;
        }
        
        private void ConnectClickHandler()
        {
            string ipAddress = _ipField.text;
            bhNetworkManager.networkAddress = ipAddress;
            bhNetworkManager.StartClient();

            _connectButton.interactable = false;
            _backButton.interactable = false;
        }

        private void BackClickedHandler()
        {
            gameObject.SetActive(false);
            OnBackClicked?.Invoke();
            _connectButton.interactable = true;
        }

        private void OnDisable()
        {
            _connectButton.onClick.RemoveListener(ConnectClickHandler);
            _backButton.onClick.RemoveListener(BackClickedHandler);
            bhNetworkManager.OnClientConnected -= ClientConnectedHandler;
            bhNetworkManager.OnClientDisconnected -= ClientDisconnectedHandler;
        }
    }
}
