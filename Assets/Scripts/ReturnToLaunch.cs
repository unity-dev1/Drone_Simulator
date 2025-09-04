using UnityEngine;

public class ReturnToLaunch : MonoBehaviour
{
    public float returnSpeed = 5f;
    private Vector3 launchPosition;
    private Rigidbody rb;
    public bool isReturning = false;
    public WaterSprayToggle wst;
    public float yOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        launchPosition = transform.position + new Vector3(0, yOffset, 0);
    }

    void FixedUpdate()
    {
        if (isReturning)
        {
            Vector3 direction = (launchPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, launchPosition);

            if (distance > 0.1f)
            {
                rb.linearVelocity = direction * returnSpeed;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
                isReturning = false; // stop once close enough
            }
        }
    }

    public void ReturnToLaunchPosition()
    {
        isReturning = true;
        if (wst.sprayEnabled)
        {
            wst.isSpraying = true;
            wst.ToggleSpray();
            wst.shouldSpray = false;
        }
    }
}
