using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ThermalEffectController : MonoBehaviour
{
    [SerializeField] private GameObject thermalEffect;
    [SerializeField] private GameObject cameraJoystick;
    [SerializeField] private GameObject captureButton;

    [SerializeField] private GameObject fpvCamera;
    [SerializeField] private Button thermaleffectToggler;
    bool isthermalEffect;

    private void Start()
    {
        thermaleffectToggler.onClick.AddListener(() =>
        {
            isthermalEffect = !isthermalEffect;
            fpvCamera.SetActive(!isthermalEffect);
            thermalEffect.SetActive(isthermalEffect);
        });
    }

    private void Update()
    {
        if (thermaleffectToggler.gameObject.activeInHierarchy &&
            Input.GetKeyDown(KeyCode.T)) 
        {
            thermaleffectToggler.onClick.Invoke();
        }
    }


    public void ToggleThermalEffect(bool isEnable) 
    {
        thermaleffectToggler.gameObject.SetActive(isEnable);
        cameraJoystick.SetActive(isEnable);
        if (SelectionManager.Instance?.selectedScenario.index == 5) 
        {
            captureButton.SetActive(isEnable);
        }
        

        if (!isEnable) 
        {
            fpvCamera.SetActive(true);
            thermalEffect.SetActive(false);
            thermaleffectToggler.gameObject.SetActive(false);
            if (captureButton != null)
            {
                captureButton?.gameObject.SetActive(false);
            }
            isthermalEffect = false;
        }
    }
}
