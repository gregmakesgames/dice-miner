using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed class FieldEntityPool : IDisposable
{
    private readonly FieldEntity prefab;
    private readonly Transform parent;
    private readonly ObjectPool<FieldEntity> pool;
    private readonly List<FieldEntity> activeEntities = new();

    public FieldEntityPool(FieldEntity prefab, Transform parent, int defaultCapacity = 64, int maxSize = 1024)
    {
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab));
        }

        this.prefab = prefab;
        this.parent = parent;

        pool = new ObjectPool<FieldEntity>(
            createFunc: CreateEntity,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyEntity,
            collectionCheck: true,
            defaultCapacity: Mathf.Max(0, defaultCapacity),
            maxSize: Mathf.Max(1, maxSize));
    }

    public int ActiveCount => activeEntities.Count;
    public int InactiveCount => pool.CountInactive;

    public FieldEntity Get()
    {
        FieldEntity entity = pool.Get();
        activeEntities.Add(entity);
        return entity;
    }

    public void Release(FieldEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        if (!activeEntities.Remove(entity))
        {
            return;
        }

        pool.Release(entity);
    }

    public void ReleaseAll()
    {
        for (int i = activeEntities.Count - 1; i >= 0; i--)
        {
            FieldEntity entity = activeEntities[i];
            if (entity != null)
            {
                pool.Release(entity);
            }
        }

        activeEntities.Clear();
    }

    public void Dispose()
    {
        activeEntities.Clear();
        pool.Dispose();
    }

    private FieldEntity CreateEntity()
    {
        return UnityEngine.Object.Instantiate(prefab, parent);
    }

    private static void OnGet(FieldEntity entity)
    {
        entity.gameObject.SetActive(true);
    }

    private void OnRelease(FieldEntity entity)
    {
        entity.Clear();
        entity.gameObject.SetActive(false);

        if (parent != null && entity.transform.parent != parent)
        {
            entity.transform.SetParent(parent, false);
        }
    }

    private static void OnDestroyEntity(FieldEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(entity.gameObject);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(entity.gameObject);
        }
    }
}
