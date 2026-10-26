using System.Collections.Generic;
using UnityEngine;

class ArrowObjectPool : MonoBehaviour
{
    public static ArrowObjectPool current;

    [Tooltip("Assign the arrow prefab.")]
    public OffScreenBaseIndicator pooledObject;
    [Tooltip("Initial pooled amount.")]
    public int pooledAmount = 1;
    [Tooltip("Should the pooled amount increase.")]
    public bool willGrow = true;

    List<OffScreenBaseIndicator> pooledObjects;

    void Awake()
    {
        current = this;
    }

    void Start()
    {
        pooledObjects = new List<OffScreenBaseIndicator>();

        if (pooledObject == null)
        {
            Debug.LogError("[ArrowObjectPool] Arrow indicator prefab is missing.", this);
            return;
        }

        for (int i = 0; i < pooledAmount; i++)
        {
            OffScreenBaseIndicator arrow = Instantiate(pooledObject);
            arrow.transform.SetParent(transform, false);
            arrow.Activate(false);
            pooledObjects.Add(arrow);
        }
    }

    /// <summary>
    /// Gets pooled objects from the pool.
    /// </summary>
    /// <returns></returns>
    public OffScreenBaseIndicator GetPooledObject()
    {
        if (pooledObjects == null || pooledObject == null)
            return null;

        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].Active)
            {
                return pooledObjects[i];
            }
        }
        if (willGrow)
        {
            OffScreenBaseIndicator arrow = Instantiate(pooledObject);
            arrow.transform.SetParent(transform, false);
            arrow.Activate(false);
            pooledObjects.Add(arrow);
            return arrow;
        }
        return null;
    }

    /// <summary>
    /// Deactive all the objects in the pool.
    /// </summary>
    public void DeactivateAllPooledObjects()
    {
        if (pooledObjects == null)
            return;

        foreach (OffScreenBaseIndicator arrow in pooledObjects)
        {
            arrow.Activate(false);
        }
    }

    void OnDestroy()
    {
        if (current == this)
            current = null;
    }
}
