namespace EasyPeasyFirstPersonController
{
    using UnityEngine;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

    public class InputManagerOld : MonoBehaviour, IInputManager
    {
        public Vector2 moveInput => GetMoveInput();
        public Vector2 lookInput => GetLookInput();
        public bool jump => GetJump();
        public bool sprint => GetSprint();
        public bool crouch => GetCrouch();
        public bool slide => GetCrouch();

        private Vector2 GetMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 move = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    move.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    move.x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    move.y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    move.y += 1f;
            }

            if (Gamepad.current != null)
                move += Gamepad.current.leftStick.ReadValue();

            return Vector2.ClampMagnitude(move, 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#else
            return Vector2.zero;
#endif
        }

        private Vector2 GetLookInput()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 look = Vector2.zero;

            if (Mouse.current != null)
                look += Mouse.current.delta.ReadValue();

            if (Gamepad.current != null)
                look += Gamepad.current.rightStick.ReadValue();

            return look;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
            return Vector2.zero;
#endif
        }

        private bool GetJump()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
                   || (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.Space);
#else
            return false;
#endif
        }

        private bool GetSprint()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null
                    && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed))
                   || (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftShift);
#else
            return false;
#endif
        }

        private bool GetCrouch()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null
                    && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed))
                   || (Gamepad.current != null && Gamepad.current.rightStickButton.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftControl);
#else
            return false;
#endif
        }
    }
}