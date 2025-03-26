using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingAnimationController : MonoBehaviour
{
    public static LoadingAnimationController Instance { get; private set; }
    [SerializeField] private Animator anim;
    [SerializeField] private Canvas canvas;
    public event Action OnLoadDone;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SceneSwitch()
    {
        SetActive(true);
        StartCoroutine(Loader.LoadSceneAsyncCoroutine(Loader.Scene.Town, 0.5f, () => SetActive(false)));
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
            OnLoadDone?.Invoke();
            Utilities.WaitAfter(0.5f, () => canvas.enabled = active);
        }

        anim.SetBool("IsLoad", active);

        if (!active)
        {
            anim.SetTrigger("UnLoad");
        }
    }
}