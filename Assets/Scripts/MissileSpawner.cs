using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MissileSpawner : MonoBehaviour
{
    [SerializeField] GameObject missiblePrefab;
    [SerializeField] int poolSize = 10;
    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    [SerializeField] Button missileLaunch;
    [SerializeField] Transform missileLaunchPosition;

    public Transform MissileLaunchPos 
    {
        private get { return missileLaunchPosition;}
        set { missileLaunchPosition = value; }
    }

    private void OnEnable()
    {
        missileLaunch.gameObject.SetActive(SelectionManager.Instance?.selectedEnvironment.index == 2);
    }

    void Start()
    {
        for (int i = 0; i < poolSize; i++) 
        {
            GameObject missile = Instantiate(missiblePrefab, transform);
            missile.SetActive(false); 
            poolQueue.Enqueue(missile);
        }

        missileLaunch.onClick.AddListener(() =>
        {
            if (poolQueue.Count <= 0) return;
            GetPooledObject(MissileLaunchPos.position, MissileLaunchPos.rotation);
        });
    }

    public GameObject GetPooledObject(Vector3 position, Quaternion rotation)
    {
        if (poolQueue.Count > 0)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        return null; 
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}
