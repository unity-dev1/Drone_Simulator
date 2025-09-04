using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HVLinesAutomationSettings : MonoBehaviour
{
    [SerializeField] private List<NodeDragHandler> moveNodes;

    private void OnEnable()
    {
        DisableNodes();
    }

    private void DisableNodes() 
    {
        EnvironmentOptionSO selectedEnvironment = SelectionManager.Instance?.selectedEnvironment;
        if (selectedEnvironment.index == 3 || selectedEnvironment.index == 6) 
        {
            foreach (NodeDragHandler node in moveNodes)
            {
                node.enabled = false;
            }
        }
    }
}
