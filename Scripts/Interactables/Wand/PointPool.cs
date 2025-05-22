using System.Collections.Generic;
using UnityEngine;

/*
 * Code by Gabriel Porlier
 * 
 * ***UNUSED***
 *
 * Replaced by Gandalf system
 *
 * */
public class PointPool : MonoBehaviour
{ 

    [SerializeField] private GameObject _pointPrefab;    
    [SerializeField] public int _poolSize = 100;
    [SerializeField] private Transform _poolContainer = default;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {        
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject point = Instantiate(_pointPrefab);
            point.SetActive(false);
            point.transform.parent = _poolContainer;
            pool.Enqueue(point);
        }
    }

    public GameObject GetPoint()
    {
        if (pool.Count > 0)
        {
            GameObject point = pool.Dequeue();
            point.SetActive(true);
            return point;
        }
        else
        {
            Debug.LogWarning("Pool épuisée ! Augmentez la taille.");
            return null;
        }
    }

    public void ReturnPoint(GameObject point)
    {
        point.SetActive(false);
        pool.Enqueue(point);
    }


}
