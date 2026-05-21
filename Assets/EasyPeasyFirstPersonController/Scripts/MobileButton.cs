using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyPeasyFirstPersonController
{

/// <summary>
/// Base class untuk mobile button input
/// Gunakan kelas ini sebagai reference di MobileInputManager
/// </summary>
public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    protected bool isPressed = false;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public bool IsPressed()
    {
        return isPressed;
    }
}
}
