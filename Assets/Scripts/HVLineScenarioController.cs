using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HVLineScenarioController : MonoBehaviour
{
    [SerializeField] private GameObject otherSceneariosSetting;
    [SerializeField] private GameObject hvLineScenarioSetting;

    [SerializeField] private Transform point1;
    [SerializeField] private Transform point2;
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private Button firstPointButton;
    [SerializeField] private Button secondPointButton;

    [SerializeField] private TextMeshProUGUI firstMessage;
    [SerializeField] private TextMeshProUGUI secondMessage;


    private void OnEnable()
    {
        hvLineScenarioSetting.SetActive(false);
        otherSceneariosSetting.SetActive(false);

        if (SelectionManager.Instance?.selectedScenario.index == 4 || SelectionManager.Instance?.selectedEnvironment.index == 5)
        {
            hvLineScenarioSetting.SetActive(true);
        }
        else
        {
            otherSceneariosSetting.SetActive(true);
        }
    }

    private void Start()
    {
        lineRenderer.positionCount = 2;

        firstPointButton.onClick.AddListener(() =>
        {
            lineRenderer.SetPosition(0, point1.position);
            lineRenderer.SetPosition(1, point1.position);
            firstPointButton.interactable = false;
            secondPointButton.gameObject.SetActive(true);
            firstMessage.gameObject.SetActive(false);
        });

        secondPointButton.onClick.AddListener(() =>
        {
            lineRenderer.SetPosition(1, point2.position);
            secondPointButton.interactable = false;
            secondMessage.gameObject.SetActive(false);
        });
    }



   
}
