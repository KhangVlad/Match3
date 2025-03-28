using System;
using UnityEngine;
using Match3;
using UnityEngine.SceneManagement;


public class LoadingAnimationController : MonoBehaviour
{
    public static LoadingAnimationController Instance { get; private set; }
    [SerializeField] private Animator anim;
    [SerializeField] private Canvas canvas;

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


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAnimation();
        }
    }

    private void TestAnimation()
    {
        SetActive(true);
        Utilities.WaitAfter(1f, () => SetActive(false));
    }

    private void SetActive(bool active)
    {
        if (active)
        {
            canvas.enabled = active;
        }
        else
        {
            Utilities.WaitAfter(0.5f, () => canvas.enabled = active);
        }

        anim.SetBool("IsLoad", active);

        if (!active)
        {
            anim.SetTrigger("UnLoad");
        }
    }
}