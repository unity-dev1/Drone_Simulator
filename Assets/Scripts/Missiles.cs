using UnityEngine;

public class Missiles : MonoBehaviour
{
    Rigidbody missibleRb;
    [SerializeField] private float speed = 10f;
    MissileSpawner missileSpawner;

    const string layerName = "drone";


    private void Start()
    {
        missibleRb = GetComponent<Rigidbody>();
        missileSpawner = FindFirstObjectByType<MissileSpawner>();
    }


    private void FixedUpdate()
    {
        missibleRb.linearVelocity = Vector3.down * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer != LayerMask.NameToLayer(layerName)) 
        {
            gameObject.SetActive(false);
            missileSpawner.ReturnToPool(gameObject);
        }
    }
   
}
