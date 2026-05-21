namespace EasyPeasyFirstPersonController
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Mobile Input Manager - Menghandle input dari joystick dan UI buttons untuk platform Android/iOS
    /// Pada PC (Editor), script ini akan auto-disable dan InputManagerOld akan digunakan
    /// Attach script ini ke GameObject yang sama dengan FirstPersonController
    /// </summary>
    public class MobileInputManager : MonoBehaviour, IInputManager
    {
        [Header("Joystick References")]
        [SerializeField] private Joystick movementJoystick;
        [SerializeField] private Joystick lookJoystick;

        [Header("Button References")]
        [SerializeField] private MobileButton jumpButton;
        [SerializeField] private MobileButton sprintButton;
        [SerializeField] private MobileButton crouchButton;

        [Header("Mobile Settings")]
        [SerializeField] private bool enableMobileInput = true;
        [SerializeField] private float lookSensitivityMultiplier = 1f;
        [SerializeField] private bool autoDisableOnPC = true;
        [SerializeField] private bool autoFindSceneControls = true;

        [Header("Auto Find Scene Object Names")]
        [SerializeField] private string movementJoystickObjectName = "MovementJoystick";
        [SerializeField] private string lookJoystickObjectName = "LookJoystick";
        [SerializeField] private string jumpButtonObjectName = "JumpButton";
        [SerializeField] private string sprintButtonObjectName = "SprintButton";
        [SerializeField] private string crouchButtonObjectName = "CrouchButton";

        private Vector2 _moveInput = Vector2.zero;
        private Vector2 _lookInput = Vector2.zero;
        private bool _jumpPressed = false;
        private bool _sprintPressed = false;
        private bool _crouchPressed = false;

        private InputManagerOld _inputManagerOld;
        private float _nextAutoResolveTime;

        public Vector2 moveInput { get => _moveInput; }
        public Vector2 lookInput { get => _lookInput; }
        public bool jump { get => _jumpPressed; }
        public bool sprint { get => _sprintPressed; }
        public bool crouch { get => _crouchPressed; }
        public bool slide { get => _crouchPressed; } // Slide menggunakan input yang sama dengan crouch

        private void Awake()
        {
            // Cache reference ke InputManagerOld jika ada
            _inputManagerOld = GetComponent<InputManagerOld>();

            // Jika platform PC dan autoDisable aktif, disable script ini dan enable InputManagerOld
            if (autoDisableOnPC && !IsMobilePlatform())
            {
                enabled = false;
                if (_inputManagerOld != null)
                    _inputManagerOld.enabled = true;
                return;
            }

            // Disable InputManagerOld pada platform mobile
            if (IsMobilePlatform() && _inputManagerOld != null)
            {
                _inputManagerOld.enabled = false;
            }
        }

        private void Start()
        {
            if (!enabled)
                return;

            TryResolveSceneReferences();

            // Validasi references
            ValidateReferences();

            // Disable mobile input jika setting disable
            if (!enableMobileInput)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (!enableMobileInput)
                return;

            if (autoFindSceneControls && Time.unscaledTime >= _nextAutoResolveTime && HasMissingReferences())
            {
                TryResolveSceneReferences();
                _nextAutoResolveTime = Time.unscaledTime + 1f;
            }

            // Cek apakah joysticks tersedia
            if (movementJoystick == null || lookJoystick == null)
            {
                Debug.LogError("MobileInputManager: Joystick references tidak lengkap!");
                enabled = false;
                return;
            }

            // Get movement input dari joystick (normalized -1 to 1)
            _moveInput = movementJoystick.Direction;

            // Get look input dari joystick dan apply sensitivity
            Vector2 rawLookInput = lookJoystick.Direction;
            _lookInput = rawLookInput * lookSensitivityMultiplier;

            // Get button inputs
            _jumpPressed = jumpButton != null && jumpButton.IsPressed();
            _sprintPressed = sprintButton != null && sprintButton.IsPressed();
            _crouchPressed = crouchButton != null && crouchButton.IsPressed();
        }

        /// <summary>
        /// Cek apakah platform adalah mobile (Android atau iOS)
        /// </summary>
        private bool IsMobilePlatform()
        {
            return Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }

        /// <summary>
        /// Validasi bahwa semua references telah diset dengan benar
        /// </summary>
        private void ValidateReferences()
        {
            if (movementJoystick == null)
                Debug.LogError("MobileInputManager: Movement Joystick reference tidak diset!");

            if (lookJoystick == null)
                Debug.LogError("MobileInputManager: Look Joystick reference tidak diset!");

            if (jumpButton == null)
                Debug.LogWarning("MobileInputManager: Jump Button reference tidak diset!");

            if (sprintButton == null)
                Debug.LogWarning("MobileInputManager: Sprint Button reference tidak diset!");

            if (crouchButton == null)
                Debug.LogWarning("MobileInputManager: Crouch Button reference tidak diset!");
        }

        private bool HasMissingReferences()
        {
            return movementJoystick == null
                || lookJoystick == null
                || jumpButton == null
                || sprintButton == null
                || crouchButton == null;
        }

        private void TryResolveSceneReferences()
        {
            if (!autoFindSceneControls)
                return;

            ResolveJoysticks();
            ResolveButtons();
        }

        private void ResolveJoysticks()
        {
            Joystick[] joysticks = FindObjectsOfType<Joystick>(true);
            if (joysticks == null || joysticks.Length == 0)
                return;

            if (movementJoystick == null)
                movementJoystick = FindJoystickByName(joysticks, movementJoystickObjectName);

            if (lookJoystick == null)
                lookJoystick = FindJoystickByName(joysticks, lookJoystickObjectName);

            if (movementJoystick == null || lookJoystick == null)
            {
                Joystick leftJoystick = null;
                Joystick rightJoystick = null;

                for (int i = 0; i < joysticks.Length; i++)
                {
                    Joystick candidate = joysticks[i];
                    if (candidate == null)
                        continue;

                    RectTransform rect = candidate.GetComponent<RectTransform>();
                    float x = rect != null ? rect.position.x : candidate.transform.position.x;

                    if (leftJoystick == null || x < GetJoystickPositionX(leftJoystick))
                        leftJoystick = candidate;

                    if (rightJoystick == null || x > GetJoystickPositionX(rightJoystick))
                        rightJoystick = candidate;
                }

                if (movementJoystick == null)
                    movementJoystick = leftJoystick;

                if (lookJoystick == null)
                    lookJoystick = rightJoystick != movementJoystick ? rightJoystick : null;
            }
        }

        private void ResolveButtons()
        {
            MobileButton[] buttonsInScene = FindObjectsOfType<MobileButton>(true);
            if (buttonsInScene == null || buttonsInScene.Length == 0)
                return;

            if (jumpButton == null)
                jumpButton = FindButtonByName(buttonsInScene, jumpButtonObjectName);

            if (sprintButton == null)
                sprintButton = FindButtonByName(buttonsInScene, sprintButtonObjectName);

            if (crouchButton == null)
                crouchButton = FindButtonByName(buttonsInScene, crouchButtonObjectName);
        }

        private Joystick FindJoystickByName(Joystick[] joysticks, string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return null;

            for (int i = 0; i < joysticks.Length; i++)
            {
                Joystick joystick = joysticks[i];
                if (joystick == null)
                    continue;

                if (string.Equals(joystick.gameObject.name, objectName, StringComparison.Ordinal))
                    return joystick;
            }

            return null;
        }

        private MobileButton FindButtonByName(MobileButton[] buttonsInScene, string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return null;

            for (int i = 0; i < buttonsInScene.Length; i++)
            {
                MobileButton button = buttonsInScene[i];
                if (button == null)
                    continue;

                if (string.Equals(button.gameObject.name, objectName, StringComparison.Ordinal))
                    return button;
            }

            return null;
        }

        private float GetJoystickPositionX(Joystick joystick)
        {
            RectTransform rect = joystick != null ? joystick.GetComponent<RectTransform>() : null;
            return rect != null ? rect.position.x : 0f;
        }

        /// <summary>
        /// Reset semua input (berguna saat app pause atau focus loss)
        /// </summary>
        public void ResetInput()
        {
            _moveInput = Vector2.zero;
            _lookInput = Vector2.zero;
            _jumpPressed = false;
            _sprintPressed = false;
            _crouchPressed = false;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                // Reset input saat app kehilangan focus
                ResetInput();
            }
        }
    }
}
