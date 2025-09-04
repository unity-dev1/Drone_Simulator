using UnityEngine;
using UnityEngine.EventSystems;

public class MidPointHandler : MonoBehaviour, IPointerClickHandler
{
    private PerimeterLine perimeterLine;
    private int index; // Index in the midPoints list

    public void Initialize(PerimeterLine perimeter, int pointIndex)
    {
        perimeterLine = perimeter;
        index = pointIndex;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EnvironmentOptionSO selectedEnvironment = SelectionManager.Instance?.selectedEnvironment;
        if (selectedEnvironment.index == 3 || selectedEnvironment.index == 6) return;
        if (perimeterLine != null)
        {
            perimeterLine.AddNodeAtMidPoint(index);
        }
    }
}
