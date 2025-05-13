using System;
using UnityEngine;
using Match3;
using Match3.Enums;
using UnityEngine.SceneManagement;
using Match3.Shares;

public class LoadingAnimationController : MonoBehaviour
{
    public static LoadingAnimationController Instance { get; private set; }
    [SerializeField] private Animator anim;
    [SerializeField] private Canvas canvas;
    public bool IsActive = false;

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


    public void SceneSwitch(Loader.Scene targetScene)
    {
        SetActive(true);
        StartCoroutine(Loader.LoadSceneAsyncCoroutine(targetScene, LoadSceneMode.Single, 0.5f, () => SetActive(false)));
    }


  

    public void SetActive(bool active, Action ondoneAnination = null)
    {
        IsActive = active;
        if (active)
        {
            canvas.enabled = active;
        }
        // else
        // {
        //     Utilities.WaitAfter(0.5f, () => canvas.enabled = active);
        // }

        anim.SetBool("IsLoad", active);

        if (!active)
        {
            anim.SetTrigger("UnLoad");
        }

        Utilities.WaitAfter(1f, () => ondoneAnination?.Invoke());
    }
}