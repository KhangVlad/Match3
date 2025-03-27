using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Serialization;

public class VfxPool : MonoBehaviour
{
    public static VfxPool Instance { get; private set; }
    private Dictionary<string, Queue<VfxGameObject>> vfxPool = new();


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

    public VfxGameObject GetVfxByName(string name)
    {
        if (vfxPool.ContainsKey(name))
        {
            if (vfxPool[name].Count > 0)
            {
                VfxGameObject vfx = vfxPool[name].Dequeue();
                vfx.gameObject.SetActive(true);
                StartCoroutine(ReturnAfterPlayTime(vfx));
                return vfx;
            }
        }
        else
        {
            vfxPool.Add(name, new Queue<VfxGameObject>());
        }

        GameObject newVfx = Instantiate(Resources.Load<GameObject>("Vfx/" + name));
        newVfx.name = name; // Ensure the name is set correctly
        VfxGameObject vfxGameObject = new VfxGameObject()
        {
            gameObject = newVfx,
            playTime = newVfx.GetComponent<ParticleSystem>().main.duration
        };
        StartCoroutine(ReturnAfterPlayTime(vfxGameObject));
        return vfxGameObject;
    }

    private IEnumerator ReturnAfterPlayTime(VfxGameObject vfx)
    {
        yield return new WaitForSeconds(vfx.playTime);
        ReturnVfx(vfx);
    }

    public void ReturnVfx(VfxGameObject vfx)
    {
        vfx.gameObject.SetActive(false);
        vfx.gameObject.transform.SetParent(transform);
        if (!vfxPool.ContainsKey(vfx.gameObject.name))
        {
            vfxPool.Add(vfx.gameObject.name, new Queue<VfxGameObject>());
        }

        vfxPool[vfx.gameObject.name].Enqueue(vfx);
    }
}


[Serializable]
public class VfxGameObject
{
    public GameObject gameObject;
    public float playTime;
}