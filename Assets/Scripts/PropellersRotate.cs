using UnityEngine;
using System.Collections;

public class PropellersRotate : MonoBehaviour
{
    public GameObject[] propellers;
    public GameObject propellersZ;
    public float maxRotationSpeed = 100f;
    public float transitionTime = 2f;

    public float currentRotationSpeed = 0f;
    private Coroutine rotationCoroutine;

    void Update()
    {
        if (currentRotationSpeed > 0f)
        {
            foreach (GameObject propeller in propellers)
            {
                if (propeller != null)
                {
                    propeller.transform.Rotate(Vector3.up * currentRotationSpeed * Time.deltaTime);
                }
            }

            propellersZ.transform.Rotate(Vector3.forward * currentRotationSpeed * Time.deltaTime);
        }
        
    }

    public void StartRotation()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(RotateToSpeed(maxRotationSpeed));
    }

    public void StopRotation()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        rotationCoroutine = StartCoroutine(RotateToSpeed(0f));
    }

    private IEnumerator RotateToSpeed(float targetSpeed)
    {
        float initialSpeed = currentRotationSpeed;
        float timeElapsed = 0f;

        while (timeElapsed < transitionTime)
        {
            currentRotationSpeed = Mathf.Lerp(initialSpeed, targetSpeed, timeElapsed / transitionTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        currentRotationSpeed = targetSpeed;
    }
}
