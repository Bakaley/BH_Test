using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using Network;
using ScriptableObjects;
using TMPro;
using UI.InGame;
using UnityEngine;
using Random = System.Random;

namespace InGame
{
    public class MatchManager : NetworkBehaviour
    {
        private BHNetworkManager _bhTestNetworkManager;

        [SerializeField] private MatchSettings _matchSettings;
        [SerializeField] private List<Transform> _spawnPoints;
        [SerializeField] private TableScore _tableScore;
        [SerializeField] private ArenaUIManager _arenaUIManager;
        private List<PlayerController> _players;

        private List<NetworkConnectionToClient> _loadedClients = new ();
        
        public override void OnStartServer()
        {
            _bhTestNetworkManager = NetworkManager.singleton as BHNetworkManager;
            _bhTestNetworkManager.SpawnPlayers(ShuffleList(_spawnPoints));
            _tableScore.Init(_bhTestNetworkManager.InGamePlayers);
            _players = _bhTestNetworkManager.InGamePlayers;

            foreach (var player in _players)
            {
                player.OnDashHit += DashHitHandle;
            }

            StartMatchAfterAllClientsLoaded();
        }

        public override void OnStartClient()
        {
            MarkClientAsLoaded(connectionToClient);
        }

        [Command(requiresAuthority = false)]
        private void MarkClientAsLoaded(NetworkConnectionToClient conn)
        {
            _loadedClients.Add(conn);
        }

        private async Task StartMatchAfterAllClientsLoaded()
        {
            while (_loadedClients.Count != _players.Count)
            {
                await Task.Yield();
            }
            PrepareMatch(false);
        }

        private void PrepareMatch(bool shufflePlayers = true)
        {
            if(shufflePlayers) RedistributePlayersBetweenSpawnPoints();
            _tableScore.ScoreTable.Keys.ToList().ForEach(key => _tableScore.ScoreTable[key] = 0);
            LockPlayersMovement();
            StartCoroutine(PreparationCountDown(_matchSettings.SecondsBeforeMatchStart));
        }

        private void StartMatch()
        {
            UnlockPlayersMovement();
        }

        private void RedistributePlayersBetweenSpawnPoints()
        {
            List<Transform> points = ShuffleList(_spawnPoints);
            int i = 0;
            foreach (var player in _players)
            {
                player.TeleportToSpawnPoint(points[i].transform.position, points[i].transform.rotation);
                i++;
            }
        }

        private List<Transform> ShuffleList(List<Transform> list)
        {
            Random rand = new Random();
            return list.OrderBy(_ => rand.Next()).ToList();
        }

        private void DashHitHandle(PlayerController hittedPlayer, PlayerController hittedBy)
        {
            hittedPlayer.RpcMarkPlayerAsHitted();
            _tableScore.ScoreTable[hittedBy.PlayerName]++;
            if (_tableScore.ScoreTable[hittedBy.PlayerName] >= _matchSettings.PointsToWin)
            {
                _arenaUIManager.RpcShowWinner(hittedBy.PlayerName);
                StartCoroutine(BetweenMatchesCountDown(_matchSettings.SecondsBetweenMatches));
            }
        }
        
        private IEnumerator BetweenMatchesCountDown(float time)
        {
            _arenaUIManager.RpcShowBetweenMatchesCountDown(time);
            yield return new WaitForSeconds(time);
            PrepareMatch();
        }

        private IEnumerator PreparationCountDown(float time)
        {
            _arenaUIManager.RpcShowPreparationCountDown(time);
            yield return new WaitForSeconds(time);
            StartMatch();
        }
        
        private void LockPlayersMovement()
        {
            _players.ForEach(player => player.LockMovement());
        }

        private void UnlockPlayersMovement()
        {
            _players.ForEach(player => player.UnlockMovement());
        }
    }
}
