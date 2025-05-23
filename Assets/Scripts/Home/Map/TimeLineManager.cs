#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using Match3;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
#endif
using Match3.Enums;
using Match3.Shares;


public class TimeLineManager : MonoBehaviour
{
    public static TimeLineManager Instance { get; private set; }

    [SerializeField] public DayInWeek currentDay;
    [SerializeField] private int currentHour = 25;
    [SerializeField] private CharacterBubble bubblePrefab;
    [SerializeField] private SpriteRenderer map;
    [SerializeField] private CharacterDirectionArrow directionArrowPrefab;
    [SerializeField] private float padding = 10f; // Padding from screen edges in pixels
   
 
    private List<CharacterActivitySO> activeInDay = new();
    private List<CharacterID> homeIds = new();
    private List<CharacterID> activeIds = new();
    private int lastCheckedHour = -1;

    private Dictionary<CharacterID, IconWithPosition> pairDict = new();
    private Camera mainCamera;

    [Header("Create New Activity Info")] public bool IsCreatingNewActivity;
    public CharacterID EditorCharacterID;
    public CharacterBubble simulatedBubble;
    public int StartTime;
    public int EndTime;
    public Vector2Int AppearPosition;
    [SerializeField] private Sprite characterSprite;
    [SerializeField] private Vector2Int homePos;


    public Sprite CharacterSprite
    {
        get => characterSprite;
        set => characterSprite = value;
    }

    public Vector2Int HomePos
    {
        get => homePos;
        set => homePos = value;
    }

    private string spritePath = "Sprites/CharactersAvatar/";

    private string
        saveActivityPath = "Assets/Resources/DataSO/CharacterActivities/"; //saveas $CharacterActivities/{charId}.asset}

//
// #if UNITY_EDITOR
//     private void Update()
//     {
//         if (!IsCreatingNewActivity) return;
//         if (Input.GetMouseButtonDown(0))
//         {
//             AppearPosition = map.WorldPositionToImagePixel(Camera.main.ScreenToWorldPoint(Input.mousePosition));
//             Vector3 worldPosition = (Vector2)map.ImagePixelToWorld(AppearPosition); // Convert Vector2Int to Vector3
//
//             if (simulatedBubble == null)
//             {
//                 CharacterBubble bubble = Instantiate(bubblePrefab, worldPosition, Quaternion.identity);
//                 Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(
//                     $"Assets/Sprites/CharactersAvatar/{(int)EditorCharacterID}.png");
//                 CharacterSprite = s;
//                 bubble.Initialize(EditorCharacterID, s, worldPosition);
//                 simulatedBubble = bubble;
//             }
//             else
//             {
//                 simulatedBubble.transform.position = worldPosition;
//             }
//         }
//     }
// #endif
    private void Update()
    {
        CheckCharacterOutOfBound();
        UpdateAllDirectionArrows();
    }

# if UNITY_EDITOR
    private void OnValidate()
    {
        //if id is changed
        UpdateTimeChange();
        if (EditorCharacterID != simulatedBubble?.characterID && simulatedBubble != null)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/Sprites/CharactersAvatar/{(int)EditorCharacterID}.png");
            simulatedBubble.Initialize(EditorCharacterID, s, simulatedBubble.transform.position);
            CharacterSprite = s;
        }
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentDay = TimeManager.Instance.GetCurrentDayInWeek();
        GetCurrentHour();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        GetCharacterActiveToday();
        ScreenInteraction.Instance.OnInteractAbleTriggered += OnInteractAbleTriggered;
    }

    private void OnDestroy()
    {
        ScreenInteraction.Instance.OnInteractAbleTriggered -= OnInteractAbleTriggered;
    }

    private void OnInteractAbleTriggered()
    {
        GetCharactersInTime(activeInDay);
    }

    private void GetCharacterActiveToday()
    {
        activeInDay = GameDataManager.Instance.GetCharacterActive(currentDay);
    }

    public void PlusCurrentHour()
    {
        currentHour++;
        UpdateTimeChange();
        GetCharactersInTime(activeInDay);
    }

    public void MinusCurrentHour()
    {
        currentHour--;
        UpdateTimeChange();
        GetCharactersInTime(activeInDay);
    }
    private void UpdateTimeChange()
    {
        bool isNewDay = false;

        if (currentHour >= 24)
        {
            currentHour = 0;
            currentDay = currentDay == DayInWeek.Sunday ? DayInWeek.Monday : currentDay + 1;
            isNewDay = true;
        }
        else if (currentHour < 0)
        {
            currentHour = 23;
            currentDay = currentDay == DayInWeek.Monday ? DayInWeek.Sunday : currentDay - 1;
            isNewDay = true;
        }

        if (isNewDay) CheckNewDay();

        lastCheckedHour = currentHour;
    }



    private bool IsNight()
    {
        return currentHour >= 12 || currentHour < 6;
    }


    private bool IsAtHomeNow(CharacterActivitySO data)
    {
        foreach (var activity in data.activityInfos)
        {
            if (activity.dayOfWeek == currentDay && activity.startTime <= currentHour && activity.endTime > currentHour)
            {
                return false;
            }
        }

        return true;
    }

    private void CheckNewDay()
    {
        GetCharacterActiveToday();
    }


    private void GetCharactersInTime(List<CharacterActivitySO> characters)
    {
        UnregisterAll();
        foreach (var character in characters)
        {
            if (IsAtHomeNow(character))
            {
                InitializeCharacterAtHome(character);
            }
            else
            {
                InitializeCharacterToWorld(character);
            }
        }
    }

    private void UnregisterAll()
    {
        homeIds.ForEach(UnregisterCharacterIcon);
        activeIds.ForEach(UnregisterCharacterIcon);
        homeIds.Clear();
        activeIds.Clear();
    }

    private void InitializeCharacterAtHome(CharacterActivitySO data)
    {
        Vector2 mapPos = map.ImagePixelToWorld(new Vector2(
            data.homePosition.x,
            data.homePosition.y));
        CharacterBubble bubble = Instantiate(bubblePrefab, mapPos, Quaternion.identity);
        bubble.transform.SetParent(this.transform);
        bubble.Initialize(data.id, data.sprite, mapPos);
        RegisterCharacterIcon(data.id, bubble);
        homeIds.Add(data.id);
    }

    private void InitializeCharacterToWorld(CharacterActivitySO data)
    {
        foreach (var activity in data.activityInfos)
        {
            if (activity.dayOfWeek == currentDay && activity.startTime <= currentHour && activity.endTime > currentHour)
            {
                Vector2 mapPos = map.ImagePixelToWorld(activity.appearPosition);
                CharacterBubble bubble = Instantiate(bubblePrefab, mapPos, Quaternion.identity);
                bubble.Initialize(data.id, data.sprite, mapPos);
                RegisterCharacterIcon(data.id, bubble);
                activeIds.Add(data.id);
                return;
            }
        }
    }


    private void GetCurrentHour()
    {
        currentHour = DateTime.Now.Hour;
    }


    public void RegisterCharacterIcon(CharacterID id, CharacterBubble characterBubble)
    {
        if (!pairDict.ContainsKey(id))
        {
            pairDict[id] = new IconWithPosition
            {
                bubble = characterBubble,
                originPosition = characterBubble.transform.position
            };
        }   
    }

    private void UnregisterCharacterIcon(CharacterID id)
    {
        if (pairDict.ContainsKey(id))
        {
            if (pairDict[id].directionArrow != null)
            {
                Destroy(pairDict[id].directionArrow.gameObject);
            }

            Destroy(pairDict[id].bubble.gameObject);
            pairDict.Remove(id);
        }
    }


    private bool IsPositionVisible(Vector2 worldPosition)
    {
        Vector2 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
        float buffer = padding / 100f;

        float extraMarginTop = 0.2f;
        float extraMarginLeft = 0.2f;
        float extraMarginRight = 0.2f;
    
        return viewportPosition.x >= (0 + buffer - extraMarginLeft) &&
               viewportPosition.x <= (1 - buffer + extraMarginRight) &&
               viewportPosition.y >= (0 + buffer - 0) &&
               viewportPosition.y <= (1 - buffer + extraMarginTop);
    }

    private void CheckCharacterOutOfBound()
    {  foreach (var entry in pairDict)
        {
            IconWithPosition iconWithPosition = entry.Value;
            bool isOut = !IsPositionVisible(iconWithPosition.bubble.transform.position);
            if (isOut != iconWithPosition.isOut)
            {
                iconWithPosition.isOut = isOut;
                if (isOut)
                {
                    if (iconWithPosition.directionArrow == null)
                    {
                        InstantiateAndPositionIcon(iconWithPosition, entry.Value.originPosition);
                    }

                    iconWithPosition.directionArrow.gameObject.SetActive(true);
                }
                else
                {
                    iconWithPosition.directionArrow?.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateAllDirectionArrows()
    {
        foreach (var entry in pairDict)
        {
            IconWithPosition iconWithPosition = entry.Value;
            if (iconWithPosition.isOut && iconWithPosition.directionArrow != null)
            {
                UpdateDirectionArrow(iconWithPosition);
            }
        }
    }

    private void UpdateDirectionArrow(IconWithPosition iconWithPosition)
    {
        Vector3 bubblePosition = iconWithPosition.bubble.transform.position;
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(bubblePosition);
        Vector3 arrowPosition;
        Vector2 direction = new Vector2(viewportPosition.x - 0.5f, viewportPosition.y - 0.5f).normalized;
        float bufferSpace = padding / 100f;
        float maxX = 1f - bufferSpace;
        float maxY = 1f - bufferSpace;
        float minX = bufferSpace;
        float minY = bufferSpace;
        float m = direction.y / direction.x; // Slope
        float x, y;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            x = direction.x > 0 ? maxX : minX;
            y = 0.5f + m * (x - 0.5f);

            y = Mathf.Clamp(y, minY, maxY);
        }
        else
        {
            y = direction.y > 0 ? maxY : minY;
            x = 0.5f + (y - 0.5f) / m;
            if (float.IsNaN(x) || float.IsInfinity(x))
            {
                x = 0.5f;
            }

            x = Mathf.Clamp(x, minX, maxX);
        }

        arrowPosition = mainCamera.ViewportToWorldPoint(new Vector3(x, y, bubblePosition.z));
        arrowPosition.z = iconWithPosition.directionArrow.transform.position.z; // Maintain original z position
        iconWithPosition.directionArrow.transform.position = arrowPosition;
    }

    private void InstantiateAndPositionIcon(IconWithPosition iconWithPosition, Vector2 originPos)
    {
        if (iconWithPosition.directionArrow == null)
        {
            CharacterDirectionArrow arrow = Instantiate(
                directionArrowPrefab,
                iconWithPosition.bubble.transform.position,
                Quaternion.identity
            );
            arrow.transform.SetParent(this.transform);
            iconWithPosition.directionArrow = arrow;
            arrow.Initialize(iconWithPosition.bubble.sprite, originPos, iconWithPosition.bubble.characterID);
        }
    }
}

[Serializable]
public class IconWithPosition
{
    public CharacterBubble bubble;
    public Vector2 originPosition;
    public bool isOut;
    public CharacterDirectionArrow directionArrow;
}
#endif