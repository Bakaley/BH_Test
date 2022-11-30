using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InGame;
using Mirror;
using ScriptableObjects;
using UI;
using UI.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class BHNetworkManager : NetworkManager
    {
        [Header("Lobby")]
        [Scene] [SerializeField] private string _menuScene;
        [SerializeField] private PlayerNetworkEntity _playerNetworkEntitySampler;
        [SerializeField] private UIManager _UIManager;
        [SerializeField] private LobbySettings _lobbySettings;

        [Header("Game")]
        [Scene] [SerializeField] private string _arenaScene;
        [SerializeField] private PlayerController _playerControllerSampler;

        private List<string> _playerNamesCachedList = new();
        private List<PlayerNetworkEntity> ConnectedPlayers { get; } = new List<PlayerNetworkEntity>();
        public List<PlayerController> InGamePlayers { get; } = new List<PlayerController>();
        
        public event Action OnClientConnected;
        public event Action OnClientDisconnected;

        private string MenuSceneName
        {
            get
            {
                string parsedName = Path.GetFileName(_menuScene);
                return parsedName.Substring(0, parsedName.IndexOf('.'));
            }
        }

        private List<string> PlayerNamesList
        {
            get
            {
                _playerNamesCachedList.Clear();
                ConnectedPlayers.ForEach(player => _playerNamesCachedList.Add(player.PlayerName));
                return _playerNamesCachedList; 
            }
        }

        public bool CheckReadyToStartGame(out string statusString)
        {
            if (ConnectedPlayers.Count < _lobbySettings.MinPlayers)
            {
                statusString = _lobbySettings.NotEnoughPlayers;
                return false;
            }

            if (PlayerNamesList.Count != PlayerNamesList.Distinct().Count())
            {
                statusString = _lobbySettings.PlayersHaveSameName;
                return false;
            }

            foreach (var player in ConnectedPlayers)
            {
                if (!player.Ready)
                {
                    statusString = _lobbySettings.PlayersAreNotReady;
                    return false;
                }
            }

            statusString = _lobbySettings.Ready;
            return true;
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            OnClientConnected?.Invoke();
        }
        
        public override void OnClientDisconnect()
        {
            if (SceneManager.GetActiveScene().name == MenuSceneName) {_UIManager.HandleDisconnect();}
            base.OnClientDisconnect();
            OnClientDisconnected?.Invoke();
        }
        
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }
            
            //Preventing from connecting in active game
            if (SceneManager.GetActiveScene().name != MenuSceneName)
            {
                conn.Disconnect();
                return;
            }
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if (SceneManager.GetActiveScene().name == MenuSceneName)
            {
                PlayerNetworkEntity playerNetworkEntity = Instantiate(_playerNetworkEntitySampler);
                NetworkServer.AddPlayerForConnection(conn, playerNetworkEntity.gameObject);
                
                if (ConnectedPlayers.Count == 0) playerNetworkEntity.HasHostRights = true;
                else playerNetworkEntity.HasHostRights = false;
                
                _UIManager.LobbyPanel.RpcInit(conn, ConnectedPlayers);
                
                ConnectedPlayers.Add(playerNetworkEntity);
                _UIManager.LobbyPanel.RpcSyncPlayerLists(ConnectedPlayers);
            }
        }
        
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            var player = conn.identity.GetComponent<PlayerNetworkEntity>();
            ConnectedPlayers.Remove(player);
            base.OnServerDisconnect(conn);
            if (SceneManager.GetActiveScene().name == MenuSceneName)
                _UIManager.LobbyPanel.RpcSyncPlayerLists(ConnectedPlayers);
        }

        public override void OnStopServer()
        {
            ConnectedPlayers.Clear();
        }

        public void StartGame()
        {
            ServerChangeScene(_arenaScene);
        }
        
        public void SpawnPlayers(List<Transform> locations)
        {
            int i = 0;
            foreach (var player in ConnectedPlayers)
            {
                var connection = player.connectionToClient;
                var playerInst = Instantiate(_playerControllerSampler, locations[i].transform.position,
                    locations[i].transform.rotation);
                playerInst.Init(player);
                NetworkServer.Destroy(connection.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(connection, playerInst.gameObject);
                InGamePlayers.Add(playerInst);
                i++;
            }
        }
    }
}
