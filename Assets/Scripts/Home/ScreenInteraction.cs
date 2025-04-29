using System;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using Match3;
using Match3.Enums;
using Match3.Shares;

public class ScreenInteraction : MonoBehaviour
{
    public static ScreenInteraction Instance { get; private set; }
    [SerializeField] private BoxCollider2D camLimits;
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer map;
    [SerializeField] private LayerMask characterLayerMask; // Add this in the Inspector
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera virtualCamera;

    public float CameraSpeed = 0.1f;
    private bool IsDragging;
    private bool FirstMouseClick;
    private Vector2 PreviouseMousePos;


    private bool InteractAble = false;

    public event Action OnInteractAbleTriggered;

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

    private void Start()
    {
        StartCoroutine(InitializeCameraPos());
    }


    public event Action<CharacterID> OnCharacterInteracted;

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (!InteractAble)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUI())
        {
            OnMouseDown();
        }

        if (Input.GetMouseButton(0) && FirstMouseClick)
        {
            OnMouseDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
    }

    private void OnMouseDown()
    {
        FirstMouseClick = true;
        PreviouseMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero, 1000, characterLayerMask);
        // VfxGameObject a = VfxPool.Instance.GetVfxByName("Ripple");
        // a.gameObject.transform.position = PreviouseMousePos; 
        if (hit.collider is not null)
        {
            if (hit.collider.TryGetComponent(out CharacterBubble character))
            {
                if (TimeLineManager.Instance.IsCreatingNewActivity) return;
                AudioManager.Instance.PlayButtonSfx();
                OnCharacterInteracted?.Invoke(character.characterID);
            }
            else if (hit.collider.TryGetComponent(out CharacterDirectionArrow arrow))
            {
                AudioManager.Instance.PlayButtonSfx();
                StartCoroutine(MoveCameraToCharacter(arrow));
            }
            else if (hit.collider.TryGetComponent(out LightStreet lightStreet))
            {
                lightStreet.Toggle();
            }
        }
    }

    private IEnumerator MoveCameraToCharacter(CharacterDirectionArrow character)
    {
        InteractAble = false;
        float transitionTime = 0.5f;
        float elapsedTime = 0f;
        Vector2 startPosition = target.position;
        Vector2 endPosition = character.Pos;

        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionTime);
            Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, t);
            target.position = new Vector3(newPosition.x, newPosition.y, target.position.z);
            yield return null;
        }

        OnCharacterInteracted?.Invoke(character.id);
        InteractAble = true;
    }

    private void OnMouseDrag()
    {
        if (Vector2.Distance(PreviouseMousePos, mainCamera.ScreenToWorldPoint(Input.mousePosition)) > 0.1f)
        {
            IsDragging = true;
        }

        if (IsDragging)
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = PreviouseMousePos - mousePosition;
            MoveTargetWithinBounds(direction);
            PreviouseMousePos = mousePosition;
        }
    }

    private void OnMouseUp()
    {
        IsDragging = false;
        FirstMouseClick = false;
    }

    private void MoveTargetWithinBounds(Vector2 dir)
    {
        Vector3 newPosition = target.position + new Vector3(dir.x, dir.y, 0) * CameraSpeed;

        if (camLimits.bounds.Contains(newPosition) && target.position != newPosition)
        {
            target.position = newPosition;
        }
    }



    private IEnumerator InitializeCameraPos()
    {
        float duration = 1.5f;
        float startOrthoSize = 20;
        float endOrthoSize = 8;
        float elapsedTime = 0f;
    
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startOrthoSize, endOrthoSize, t);
            yield return null;
        }
    
    
        mainCamera.orthographicSize = endOrthoSize;
        InteractAble = true;
        OnInteractAbleTriggered?.Invoke();
    }
    
 


#if UNITY_EDITOR
    // private void OnDrawGizmos()
    // {
    //     if (map != null)
    //     {
    //         Vector2 pixelPos = Utilities.WorldPositionToImagePixel(map, PreviouseMousePos);
    //         Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    //
    //         GUIStyle style = new GUIStyle();
    //         style.fontSize = 16; // Set the font size
    //         style.normal.textColor = Color.black; // Set the text color to black
    //
    //         UnityEditor.Handles.Label(mousePos, $"({pixelPos.x}, {pixelPos.y})", style);
    //     }
    // }

#endif
}