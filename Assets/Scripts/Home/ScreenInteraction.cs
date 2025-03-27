using System;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ScreenInteraction : MonoBehaviour
{
    public static ScreenInteraction Instance { get; private set; }
    [SerializeField] private BoxCollider2D camLimits;
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer map;
    [SerializeField] private LayerMask characterLayerMask; // Add this in the Inspector
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineCamera virtualCamera;
    private bool IsDragging;
    private bool FirstMouseClick;
    private Vector2 PreviouseMousePos;


    private bool InteractAble = false;

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

        if (Input.GetMouseButtonDown(0))
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

        RaycastHit2D hit = Physics2D.Raycast(PreviouseMousePos, Vector2.zero, 1000, characterLayerMask);
        VfxGameObject a = VfxPool.Instance.GetVfxByName("Heart");
        a.gameObject.transform.position = PreviouseMousePos;

        if (hit.collider is not null)
        {
            if (hit.collider.TryGetComponent(out CharacterBubble character))
            {
                OnCharacterInteracted?.Invoke(character.characterID);
            }

            if (hit.collider.TryGetComponent(out CharacterDirectionArrow arrow))
            {
                StartCoroutine(MoveCameraToCharacter(arrow));
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

    private void MoveTargetWithinBounds(Vector2 position)
    {
        Vector2 newPosition = target.position + (Vector3)position;
        if (camLimits.bounds.Contains(newPosition))
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
    }
}