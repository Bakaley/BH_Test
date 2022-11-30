using Cinemachine;
using Mirror;
using ScriptableObjects;
using UnityEngine;

namespace InGame
{
    public class CameraController : NetworkBehaviour
    {
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private CinemachineVirtualCamera _cinemachineCamera;
        
        private LocalPlayerSettings _settings;
        private PlayerControlActions _playerInput;
    
        private readonly float _upAngleClamp = 340;
        private readonly float _downAngleClamp = 40;
        private float _mouseX, _mouseY;

        private bool _inited = false;
    
        public void Init(PlayerControlActions playerInput, LocalPlayerSettings settings)
        {
            _settings = settings;
            _playerInput = playerInput;
            _inited = true;
        }

        public override void OnStartAuthority()
        {
            _cinemachineCamera.gameObject.SetActive(true);
        }

        void LateUpdate()
        {
            if (isLocalPlayer && _inited) CameraControl();
        }

        private void CameraControl()
        {
            Vector2 mouseDelta = _playerInput.Player.MouseLook.ReadValue<Vector2>();
            
            _cameraTarget.transform.rotation *= Quaternion.AngleAxis(mouseDelta.x * _settings.MouseSensitivity, Vector3.up);
            _cameraTarget.transform.rotation *= Quaternion.AngleAxis(-mouseDelta.y * _settings.MouseSensitivity, Vector3.right);
            
            var angles = _cameraTarget.transform.localEulerAngles;
            angles.z = 0;
            
            //Clamping the Up/Down rotation
            if (angles.x > 180 && angles.x < _upAngleClamp) angles.x = _upAngleClamp;
            else if(angles.x < 180 && angles.x > _downAngleClamp) angles.x = _downAngleClamp;
            
            transform.rotation = Quaternion.Euler(0, _cameraTarget.transform.rotation.eulerAngles.y, 0);
            _cameraTarget.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
        }
    }
}
