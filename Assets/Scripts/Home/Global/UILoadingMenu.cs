using UnityEngine;
using UnityEngine.UI;

public class UILoadingMenu : MonoBehaviour
{
    [SerializeField] private Button homeButton;

    private void Start()
    {
        homeButton.onClick.AddListener(OnHomeClick);
    }

    private void OnDestroy()
    {
        homeButton.onClick.RemoveListener(OnHomeClick);
    }

    private void OnHomeClick()
    {
        LoadingAnimationController.Instance.SceneSwitch();
    }
}