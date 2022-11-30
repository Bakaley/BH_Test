using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    public class EnterNamePanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _confirmButton;

        public event Action<string> OnNameChanged;
        public event Action OnConfirmClick;
        
        public static string DisplayName { get; private set; }
        private readonly string _prefsKeyPlayerName = "PlayerName";

        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(ConfirmClickHandler);
            _inputField.onValueChanged.AddListener(ValidatePlayerName);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (PlayerPrefs.HasKey(_prefsKeyPlayerName))
            {
                string savedName = PlayerPrefs.GetString(_prefsKeyPlayerName);
                _inputField.text = savedName;
                ValidatePlayerName(savedName);
            }
        }

        private void ValidatePlayerName(string name)
        {
            _confirmButton.interactable = !String.IsNullOrEmpty(name);
        }
        

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(ConfirmClickHandler);
            _inputField.onValueChanged.RemoveListener(ValidatePlayerName);
        }

        private void ConfirmClickHandler()
        {
            SaveName();
            gameObject.SetActive(false);
            OnConfirmClick?.Invoke();
        }

        private void SaveName()
        {
            DisplayName = _inputField.text;
            PlayerPrefs.SetString(_prefsKeyPlayerName, DisplayName);
            OnNameChanged?.Invoke(DisplayName);
        }
    }
}
