using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Network;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace InGame
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private LocalPlayerSettings _localSettings;
        [SerializeField] private ServerPlayerSettings _serverSettings;
        [SerializeField] private Animator _animator;
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private List<int> _paintableMatNumbers = new List<int>() {0, 4};
        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _hittedColor;
        
        private Rigidbody _rigidbody;
        private Collider _collider;
        private CameraController _cameraController;
        private Camera _mainCamera;
        
        private Vector3 _movementDirection;
        private Vector3 _dashDirection;
        private PlayerControlActions _playerInput;

        private float _dashCurrentCD;
        private Coroutine _dashCoroutine;
        private bool _invulnerable;
        private float _paintingDurationLeft;
        private bool _ableToMove;

        private readonly string _motionParamName = "MotionParam";

        [SyncVar] private string _playerName;
        public string PlayerName => _playerName;

        public event Action<PlayerController, PlayerController> OnDashHit;
        
        private bool OnGround => Physics.Raycast(transform.position, Vector3.down,
            _collider.bounds.extents.y + _serverSettings.RaycastGroundCheckLength);

        public void Init(PlayerNetworkEntity player)
        {
            _playerName = player.PlayerName;
        }

        public override void OnStartLocalPlayer()
        {
            Physics.gravity = new Vector3(0, _serverSettings.GravityForce, 0);
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        
            _playerInput = new PlayerControlActions();
            _playerInput.Player.Dash.performed += Dash();
            _playerInput.Player.Jump.performed += Jump();

            _playerInput.Enable();
            
            _mainCamera = Camera.main;
            
            if(isLocalPlayer) _cameraController.Init(_playerInput, _localSettings);
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _cameraController = GetComponent<CameraController>();
        }
        
        private void Update()
        {
            if (isLocalPlayer && _ableToMove)
            {
                ReadInput();
            }
        }

        private void FixedUpdate()
        {
            if (isLocalPlayer && _ableToMove)
            {
                Movement();
            }
        }

        private void ReadInput()
        {
            var inputDir = _playerInput.Player.Movement.ReadValue<Vector2>();
            _movementDirection  = _mainCamera.transform.right * inputDir.x
                                  + _mainCamera.transform.forward * inputDir.y;
            _movementDirection.y = 0;
            
            Debug.Log(inputDir.magnitude);
            _animator.SetFloat(_motionParamName, inputDir.magnitude);
        }

        private void Movement()
        {
            if (_dashCoroutine == null)
            {
                Vector3 velocity = new Vector3(
                    _movementDirection.x * _serverSettings.PlayerMovementSpeedModifier * Time.fixedDeltaTime,
                    _rigidbody.velocity.y,
                    _movementDirection.z * _serverSettings.PlayerMovementSpeedModifier * Time.fixedDeltaTime);
                _rigidbody.velocity = velocity;
            }
            else
            {
                Vector3 velocity = new Vector3(
                    _dashDirection.x * _serverSettings.DashMovementSpeed * Time.fixedDeltaTime,
                    _rigidbody.velocity.y,
                    _dashDirection.z * _serverSettings.DashMovementSpeed * Time.fixedDeltaTime);
                _rigidbody.velocity = velocity;
            }
        }

        private Action<InputAction.CallbackContext> Jump()
        {
            return _ =>
            {
                if(OnGround) _rigidbody.AddForce(Vector3.up * _serverSettings.JumpImpulse, ForceMode.Impulse);
            };
        }

        private Action<InputAction.CallbackContext> Dash()
        {
            return _ =>
            {
                if(_movementDirection.magnitude > _serverSettings.DashMovementThreshold) CmdAttemptToDash();
            };
        }

        #region DashLogic
        [Command]
        private void CmdAttemptToDash()
        {
            if (ValidateDash())
            {
                StartCoroutine(StartDashCD());
                RpcDash();
            }
        }

        [Server]
        private IEnumerator StartDashCD()
        {
            _dashCurrentCD = _serverSettings.DashCD;
            while (_dashCurrentCD > 0)
            {
                yield return new WaitForEndOfFrame();
                _dashCurrentCD -= Time.deltaTime;
            }
        }

        [Server]
        private bool ValidateDash()
        {
            return _dashCurrentCD <= 0;
        }

        [TargetRpc]
        private void RpcDash()
        {
            _dashCoroutine = StartCoroutine(DashCoroutine());
        }
        
        [Client]
        private IEnumerator DashCoroutine()
        {
            _dashDirection = _movementDirection;
            float dashMaxDuration = _serverSettings.DashDestination / (_serverSettings.DashMovementSpeed * Time.fixedDeltaTime);
            
            float timePassed = 0;
            while (timePassed < dashMaxDuration)
            {
                timePassed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            _dashCoroutine = null;
        }
        #endregion

        #region HitLogic
        [Client]
        private void OnCollisionEnter(Collision collision)
        {
            if (_dashCoroutine != null && collision.gameObject.TryGetComponent(out PlayerController other))
            {
                //sending collision info on server
                CmdHitHandling(other, this);
            }
        }

        [Command]
        private void CmdHitHandling(PlayerController hittedPlayer, PlayerController hittedBy)
        {
            if (!hittedPlayer._invulnerable)
            {
                OnDashHit?.Invoke(hittedPlayer, hittedBy);
            }
        }
        
        [ClientRpc]
        public void RpcMarkPlayerAsHitted()
        {
            StartCoroutine(HittedPaintingCoroutine());
        }
        
        private IEnumerator HittedPaintingCoroutine()
        {
            _paintingDurationLeft = _serverSettings.PaintingDuration;
            _invulnerable = true;
            foreach (int matNumber in _paintableMatNumbers)
            {
                _skinnedMeshRenderer.materials[matNumber].color = _hittedColor;
            }
            while (_paintingDurationLeft > 0)
            {
                yield return new WaitForEndOfFrame();
                _paintingDurationLeft -= Time.deltaTime;
            }
            foreach (int matNumber in _paintableMatNumbers)
            {
                _skinnedMeshRenderer.materials[matNumber].color = _defaultColor;
            }
            _invulnerable = false;
        }
        #endregion

        #region betweenMatchesLogic
        [ClientRpc]
        public void TeleportToSpawnPoint(Vector3 newPos, Quaternion newRot)
        {
            _rigidbody.position = newPos;
            _rigidbody.rotation = newRot;
        }
        
        [ClientRpc]
        public void LockMovement()
        {
            _ableToMove = false;
            _rigidbody.velocity = Vector3.zero;
            _animator.SetFloat(_motionParamName, 0);
        }
        
        [ClientRpc]
        public void UnlockMovement()
        {
            _ableToMove = true;
        }
        #endregion

        private void OnDisable()
        { 
            if(isLocalPlayer) _playerInput.Disable();
        }
    }
}
