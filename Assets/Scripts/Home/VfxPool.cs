using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VfxPool : MonoBehaviour
{
    public static VfxPool Instance { get; private set; }
    private Dictionary<string, Queue<GameObject>> vfxPool = new();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public GameObject GetVfxByName(string name)
    {
        if (vfxPool.ContainsKey(name))
        {
            if (vfxPool[name].Count > 0)
            {
                GameObject vfx = vfxPool[name].Dequeue();
                vfx.SetActive(true);
                StartCoroutine(ReturnAfterPlayTime(vfx));
                return vfx;
            }
        }
        else
        {
            vfxPool.Add(name, new Queue<GameObject>());
        }

        GameObject newVfx = Instantiate(Resources.Load<GameObject>("Vfx/" + name));
        newVfx.name = name;
        newVfx.SetActive(true);
        StartCoroutine(ReturnAfterPlayTime(newVfx));
        return newVfx;
    }

    private IEnumerator ReturnAfterPlayTime(GameObject vfx)
    {
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            yield return new WaitForSeconds(ps.main.duration);
            ReturnVfx(vfx);
        }
    }

    public void ReturnVfx(GameObject vfx)
    {
        vfx.SetActive(false);
        vfx.transform.SetParent(transform);
        if (!vfxPool.ContainsKey(vfx.name))
        {
            vfxPool.Add(vfx.name, new Queue<GameObject>());
        }

        vfxPool[vfx.name].Enqueue(vfx);
    }
}