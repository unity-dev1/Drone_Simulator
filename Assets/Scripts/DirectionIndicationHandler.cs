using UnityEngine;
using UnityEngine.UI;

public class DirectionIndicationHandler : MonoBehaviour
{
    public Button indicationButton;
    public GameObject indicator;

    void Start()
    {
        indicationButton.onClick.AddListener(() =>
        {
            ToggleIndicator();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) 
        {
            ToggleIndicator();
        }
    }


    private void ToggleIndicator() 
    {
        if (indicator.activeInHierarchy)
        {
            indicator.SetActive(false);
        }
        else
        {
            indicator.SetActive(true);
        }
    }
}
