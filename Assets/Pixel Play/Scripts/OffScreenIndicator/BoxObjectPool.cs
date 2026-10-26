using System.Collections.Generic;
using UnityEngine;

public class BoxObjectPool : MonoBehaviour
{
    public static BoxObjectPool current;

    [Tooltip("Assign the box prefab.")]
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
            Debug.LogError("[BoxObjectPool] Box indicator prefab is missing.", this);
            return;
        }

        for (int i = 0; i < pooledAmount; i++)
        {
            OffScreenBaseIndicator box = Instantiate(pooledObject);
            box.transform.SetParent(transform, false);
            box.Activate(false);
            pooledObjects.Add(box);
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
            OffScreenBaseIndicator box = Instantiate(pooledObject);
            box.transform.SetParent(transform, false);
            box.Activate(false);
            pooledObjects.Add(box);
            return box;
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

        foreach (OffScreenBaseIndicator box in pooledObjects)
        {
            box.Activate(false);
        }
    }

    void OnDestroy()
    {
        if (current == this)
            current = null;
    }
}
