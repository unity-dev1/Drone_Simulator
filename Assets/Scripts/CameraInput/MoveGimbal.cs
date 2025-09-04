using UnityEngine;
using UnityEngine.EventSystems;

public class MoveGimbal : MonoBehaviour, IGimbalMovement, IPointerDownHandler, IPointerUpHandler
{
    bool isGimbalPressed = false;

    public void OnInputPressed(Vector2 movementVector)
    {
        if (isGimbalPressed) return;
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = movementVector * 100;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isGimbalPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isGimbalPressed = false;
    }
}
