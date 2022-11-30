using UnityEngine;

namespace UI.Lobby
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private EnterNamePanel _enterNamePanel;
        [SerializeField] private MainMenuPanel _mainMenuPanel;
        [SerializeField] private IPAddressPanel _ipAddressPanel;
        [SerializeField] private LobbyPanel _lobbyPanel;

        public LobbyPanel LobbyPanel => _lobbyPanel;

        public void HandleDisconnect()
        {
            if(_lobbyPanel.isActiveAndEnabled) _lobbyPanel.Disconnect();
        }

        private void Start()
        {
            _enterNamePanel.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _enterNamePanel.OnConfirmClick += EnterNamePanelConfirmHandler;
            _mainMenuPanel.OnHostClicked += HostClickedHandler;
            _mainMenuPanel.OnJoinClicked += JoinClickedHandler;
            _mainMenuPanel.OnBackClicked += ToNameEnteringHandler;
            _ipAddressPanel.OnConnectionComplete += IPConnectionCompleteHandler;
            _ipAddressPanel.OnBackClicked += ToMainMenuHandler;
            _lobbyPanel.ToMainMenu += ToMainMenuHandler;
        }

        private void EnterNamePanelConfirmHandler()
        {
            _mainMenuPanel.gameObject.SetActive(true);
        }

        private void HostClickedHandler()
        {
            //manual activation of NetworkIdentities leads to sync error during connection
            //_lobbyPanel as NetworkIdentity automatically becomes active after connection
            
            //this method created for explanation only and can be deleted
            
            // _lobbyPanel.gameObject.SetActive(true);
        }

        private void JoinClickedHandler()
        {
            _ipAddressPanel.gameObject.SetActive(true);
        }

        private void IPConnectionCompleteHandler()
        {
            //manual activation of NetworkIdentities leads to sync error during connection
            //_lobbyPanel as NetworkIdentity automatically becomes active after connection
            
            //this method created for explanation only and can be deleted

            // _lobbyPanel.gameObject.SetActive(true);
        }

        private void ToNameEnteringHandler()
        {
            _enterNamePanel.gameObject.SetActive(true);
        }
        
        private void ToMainMenuHandler()
        {
            _mainMenuPanel.gameObject.SetActive(true);
        }
        
        private void OnDisable()
        {
            _enterNamePanel.OnConfirmClick -= EnterNamePanelConfirmHandler;
            _mainMenuPanel.OnJoinClicked -= HostClickedHandler;
            _mainMenuPanel.OnHostClicked -= JoinClickedHandler;
            _mainMenuPanel.OnBackClicked -= ToNameEnteringHandler;
            _ipAddressPanel.OnConnectionComplete -= IPConnectionCompleteHandler;
            _ipAddressPanel.OnBackClicked -= ToMainMenuHandler;
            _lobbyPanel.ToMainMenu -= ToMainMenuHandler;
        }
    }
}
