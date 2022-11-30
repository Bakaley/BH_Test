using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InGame;
using Mirror;
using Network;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.InGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TableScore : NetworkBehaviour
    {
        [SerializeField] private TableRow _rowExample;
        [SerializeField] private VerticalLayoutGroup _table;

        private CanvasGroup _canvasGroup;
        private PlayerControlActions _playerInput;
        
        //since we cannot use SyncDictionary<PlayerController, int> because of reference type in key position
        //https://github.com/vis2k/Mirror/issues/1931
        //we will use just player's name + score pair instead of player + score
        
        private readonly SyncDictionary<string, int> _scoreTable = new SyncDictionary<string, int>();
        private Dictionary<string, TableRow> _tablePositions = new Dictionary<string, TableRow>();
        
        public SyncDictionary<string, int> ScoreTable => _scoreTable;

        [Server]
        public void Init(List<PlayerController> inGamePlayers)
        {
            foreach (var player in inGamePlayers)
            {
                _scoreTable.Add(player.PlayerName, 0);
            }
        }

        public override void OnStartClient()
        {
            _playerInput = new PlayerControlActions();
            _playerInput.Player.ShowMatchInfo.started += ShowTable();
            _playerInput.Player.ShowMatchInfo.canceled += HideTable();
            _playerInput.Enable();
            
            foreach (var pair in _scoreTable)
            {
                var row = Instantiate(_rowExample, _table.transform);
                row.Init(pair.Key);
                _tablePositions.Add(pair.Key, row);
            }

            _scoreTable.Callback += ScoreChangedHandler;
        }
        
        public override void OnStopClient()
        {
            _playerInput.Player.ShowMatchInfo.started -= ShowTable();
            _playerInput.Player.ShowMatchInfo.canceled -= HideTable();
            _playerInput.Disable();
        }
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        private Action<InputAction.CallbackContext> ShowTable()
        {
            return _ =>
            {
                _canvasGroup.alpha = 1;
            };
        }

        private Action<InputAction.CallbackContext> HideTable()
        {
            return _ =>
            {
                _canvasGroup.alpha = 0;
            };
        }
        
        private void ScoreChangedHandler(SyncDictionary<string, int>.Operation op, string playerName, int value)
        {
            switch (op)
            {
                case SyncIDictionary<string, int>.Operation.OP_SET:
                    _tablePositions[playerName].RefreshScore(value);
                    break;
            }
        }
    }
}
