// using System;
// using System.Collections.Generic;
// using Match3;
// using UnityEngine;
// # if UNITY_EDITOR
// using UnityEditor;
// #endif
// using Match3.Enums;
// using Match3.Shares;
// using UnityEngine.Serialization;
//
//
// public class TimeLineManager : MonoBehaviour
// {
//     public static TimeLineManager Instance { get; private set; }
//
//     [SerializeField] public DayInWeek currentDay;
//     [SerializeField] private int currentHour = 25;
//     [SerializeField] private CharacterBubble bubblePrefab;
//     [SerializeField] private SpriteRenderer map;
//     [SerializeField] private CharacterDirectionArrow directionArrowPrefab;
//     [SerializeField] private BoxCollider2D BoundCollider;
//     [SerializeField] private float padding = 1f; //for bounds check
//     [SerializeField] private GameObject _lightingManager2D;
//     [SerializeField] private GameObject _nightGameobjects;
//     private List<CharacterActivitySO> activeInDay = new();
//     private List<CharacterID> homeIds = new();
//     private List<CharacterID> activeIds = new();
//     private int lastCheckedHour = -1;
//
//
//     private Dictionary<CharacterID, IconWithPosition> pairDict = new();
//
//     private Bounds paddedBounds;
//
//
//     [Header("Create New Activity Info")] public bool IsCreatingNewActivity;
//     public CharacterID EditorCharacterID;
//     public CharacterBubble simulatedBubble;
//     public int StartTime;
//     public int EndTime;
//     public Vector2Int AppearPosition;
//     [SerializeField] private Sprite characterSprite;
//     [SerializeField] private Vector2Int homePos;
//
//
//     public Sprite CharacterSprite
//     {
//         get => characterSprite;
//         set => characterSprite = value;
//     }
//
//     public Vector2Int HomePos
//     {
//         get => homePos;
//         set => homePos = value;
//     }
//
//     private string spritePath = "Sprites/CharactersAvatar/";
//
//     private string
//         saveActivityPath = "Assets/Resources/DataSO/CharacterActivities/"; //saveas $CharacterActivities/{charId}.asset}
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
//
// # if UNITY_EDITOR
//     private void OnValidate()
//     {
//         //if id is changed
//         UpdateTimeChange();
//         if (EditorCharacterID != simulatedBubble?.characterID && simulatedBubble != null)
//         {
//             Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(
//                 $"Assets/Sprites/CharactersAvatar/{(int)EditorCharacterID}.png");
//             simulatedBubble.Initialize(EditorCharacterID, s, simulatedBubble.transform.position);
//             CharacterSprite = s;
//         }
//     }
// #endif
//
//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//
//         Instance = this;
//         currentDay = TimeManager.Instance.GetCurrentDayInWeek();
//         GetCurrentHour();
//         CheckDayAndNight();
//     }
//
//     private void Start()
//     {
//         GetCharacterActiveToday();
//         AdjustColliderBounds();
//         ScreenInteraction.Instance.OnInteractAbleTriggered += OnInteractAbleTriggered;
//     }
//
//     private void OnDestroy()
//     {
//         ScreenInteraction.Instance.OnInteractAbleTriggered -= OnInteractAbleTriggered;
//     }
//
//     private void OnInteractAbleTriggered()
//     {
//         GetCharactersInTime(activeInDay);
//     }
//
//     private void GetCharacterActiveToday()
//     {
//         activeInDay = GameDataManager.Instance.GetCharacterActive(currentDay);
//     }
//
//     public void PlusCurrentHour()
//     {
//         currentHour++;
//         UpdateTimeChange();
//         GetCharactersInTime(activeInDay);
//     }
//
//     public void MinusCurrentHour()
//     {
//         currentHour--;
//         UpdateTimeChange();
//         GetCharactersInTime(activeInDay);
//     }
//
//     public void ChangeTimeOfDay(TimeOfDay t)
//     {
//         if (t == TimeOfDay.Morning || t == TimeOfDay.Midday || t == TimeOfDay.Afternoon)
//         {
//             _lightingManager2D.SetActive(false);
//             _nightGameobjects.SetActive(false);
//         }
//         else
//         {
//             _lightingManager2D.SetActive(true);
//             _nightGameobjects.SetActive(true);
//         }
//     }
//
//     private void UpdateTimeChange()
//     {
//         bool isNewDay = false;
//
//         if (currentHour >= 24)
//         {
//             currentHour = 0;
//             currentDay = currentDay == DayInWeek.Sunday ? DayInWeek.Monday : currentDay + 1;
//             isNewDay = true;
//         }
//         else if (currentHour < 0)
//         {
//             currentHour = 23;
//             currentDay = currentDay == DayInWeek.Monday ? DayInWeek.Sunday : currentDay - 1;
//             isNewDay = true;
//         }
//
//         if (isNewDay) CheckNewDay();
//
//         CheckDayAndNight();
//         lastCheckedHour = currentHour;
//     }
//
//
//     private void CheckDayAndNight()
//     {
//         if (IsNight())
//         {
//             _lightingManager2D.SetActive(true);
//             _nightGameobjects.SetActive(true);
//         }
//         else
//         {
//             _lightingManager2D.SetActive(false);
//             _nightGameobjects.SetActive(false);
//         }
//     }
//
//     private bool IsNight()
//     {
//         return currentHour >= 12 || currentHour < 6;
//     }
//
//
//     private bool IsAtHomeNow(CharacterActivitySO data)
//     {
//         foreach (var activity in data.activityInfos)
//         {
//             if (activity.dayOfWeek == currentDay && activity.startTime <= currentHour && activity.endTime > currentHour)
//             {
//                 return false;
//             }
//         }
//
//         return true;
//     }
//
//     private void CheckNewDay()
//     {
//         GetCharacterActiveToday();
//     }
//
//
//     private void GetCharactersInTime(List<CharacterActivitySO> characters)
//     {
//         UnregisterAll();
//         foreach (var character in characters)
//         {
//             if (IsAtHomeNow(character))
//             {
//                 InitializeCharacterAtHome(character);
//             }
//             else
//             {
//                 InitializeCharacterToWorld(character);
//             }
//         }
//     }
//
//     private void UnregisterAll()
//     {
//         homeIds.ForEach(UnregisterCharacterIcon);
//         activeIds.ForEach(UnregisterCharacterIcon);
//         homeIds.Clear();
//         activeIds.Clear();
//     }
//
//     private void InitializeCharacterAtHome(CharacterActivitySO data)
//     {
//         Vector2 mapPos = map.ImagePixelToWorld(new Vector2(
//             data.homePosition.x,
//             data.homePosition.y));
//         CharacterBubble bubble = Instantiate(bubblePrefab, mapPos, Quaternion.identity);
//         bubble.transform.SetParent(this.transform);
//         bubble.Initialize(data.id, data.sprite, mapPos);
//         RegisterCharacterIcon(data.id, bubble);
//         homeIds.Add(data.id);
//     }
//
//     private void InitializeCharacterToWorld(CharacterActivitySO data)
//     {
//         foreach (var activity in data.activityInfos)
//         {
//             if (activity.dayOfWeek == currentDay && activity.startTime <= currentHour && activity.endTime > currentHour)
//             {
//                 Vector2 mapPos = map.ImagePixelToWorld(activity.appearPosition);
//                 CharacterBubble bubble = Instantiate(bubblePrefab, mapPos, Quaternion.identity);
//                 bubble.Initialize(data.id, data.sprite, mapPos);
//                 RegisterCharacterIcon(data.id, bubble);
//                 activeIds.Add(data.id);
//                 return;
//             }
//         }
//     }
//
//
//     private void GetCurrentHour()
//     {
//         currentHour = DateTime.Now.Hour;
//     }
//
//
//     public void RegisterCharacterIcon(CharacterID id, CharacterBubble characterBubble)
//     {
//         if (!pairDict.ContainsKey(id))
//         {
//             pairDict[id] = new IconWithPosition
//             {
//                 bubble = characterBubble,
//                 originPosition = characterBubble.transform.position
//             };
//         }
//     }
//
//     private void UnregisterCharacterIcon(CharacterID id)
//     {
//         if (pairDict.ContainsKey(id))
//         {
//             if (pairDict[id].directionArrow != null)
//             {
//                 Destroy(pairDict[id].directionArrow.gameObject);
//             }
//
//             Destroy(pairDict[id].bubble.gameObject);
//             pairDict.Remove(id);
//         }
//     }
//
//     private void FixedUpdate()
//     {
//         CheckCharacterOutOfBound();
//         UpdateAllDirectionArrows();
//     }
//
//
//     private void CheckCharacterOutOfBound()
//     {
//         paddedBounds = BoundCollider.bounds;
//         paddedBounds.Expand(padding * 3);
//
//         foreach (var entry in pairDict)
//         {
//             IconWithPosition iconWithPosition = entry.Value;
//             bool isOut = !paddedBounds.Contains(iconWithPosition.bubble.transform.position);
//
//             if (isOut != iconWithPosition.isOut)
//             {
//                 iconWithPosition.isOut = isOut;
//
//                 if (isOut)
//                 {
//                     if (iconWithPosition.directionArrow == null)
//                     {
//                         InstantiateAndPositionIcon(iconWithPosition, entry.Value.originPosition);
//                     }
//
//                     iconWithPosition.directionArrow.gameObject.SetActive(true);
//                 }
//                 else
//                 {
//                     iconWithPosition.directionArrow?.gameObject.SetActive(false);
//                 }
//             }
//         }
//     }
//
//
//     private void UpdateAllDirectionArrows()
//     {
//         foreach (var entry in pairDict)
//         {
//             IconWithPosition iconWithPosition = entry.Value;
//             if (iconWithPosition.isOut && iconWithPosition.directionArrow != null)
//             {
//                 UpdateDirectionArrow(iconWithPosition);
//             }
//         }
//     }
//
//
//     private void UpdateDirectionArrow(IconWithPosition iconWithPosition)
//     {
//         iconWithPosition.directionArrow.transform.position =
//             GetClosestPointOnCameraBounds(iconWithPosition.bubble.transform.position);
//     }
//
//     private Vector2 GetClosestPointOnCameraBounds(Vector2 position)
//     {
//         return BoundCollider.bounds.ClosestPoint(position);
//     }
//
//     private void InstantiateAndPositionIcon(IconWithPosition iconWithPosition, Vector2 originPos)
//     {
//         if (iconWithPosition.directionArrow == null)
//         {
//             CharacterDirectionArrow arrow = Instantiate(
//                 directionArrowPrefab,
//                 iconWithPosition.bubble.transform.position,
//                 Quaternion.identity
//             );
//             arrow.transform.SetParent(this.transform);
//             iconWithPosition.directionArrow = arrow;
//             arrow.Initialize(iconWithPosition.bubble.sprite, originPos, iconWithPosition.bubble.characterID);
//         }
//     }
//
//     private void AdjustColliderBounds()
//     {
//         float screenRatio = (float)Screen.width / (float)Screen.height;
//         float newWidth = BoundCollider.size.x - (padding * screenRatio);
//         float newHeight = BoundCollider.size.y - (padding * screenRatio);
//         BoundCollider.size = new Vector2(newWidth, newHeight);
//     }
// }
//
// [Serializable]
// public class IconWithPosition
// {
//     public CharacterBubble bubble;
//     public Vector2 originPosition;
//     public bool isOut;
//     public CharacterDirectionArrow directionArrow;
// }

using System;
using System.Collections.Generic;
using Match3;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
#endif
using Match3.Enums;
using Match3.Shares;
using UnityEngine.Serialization;


public class TimeLineManager : MonoBehaviour
{
    public static TimeLineManager Instance { get; private set; }

    [SerializeField] public DayInWeek currentDay;
    [SerializeField] private int currentHour = 25;
    [SerializeField] private CharacterBubble bubblePrefab;
    [SerializeField] private SpriteRenderer map;
    [SerializeField] private CharacterDirectionArrow directionArrowPrefab;
    [SerializeField] private float padding = 10f; // Padding from screen edges in pixels
    [SerializeField] private GameObject _lightingManager2D;
    [SerializeField] private GameObject _nightGameobjects;
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
        CheckDayAndNight();
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

    public void ChangeTimeOfDay(TimeOfDay t)
    {
        if (t == TimeOfDay.Morning || t == TimeOfDay.Midday || t == TimeOfDay.Afternoon)
        {
            _lightingManager2D.SetActive(false);
            _nightGameobjects.SetActive(false);
        }
        else
        {
            _lightingManager2D.SetActive(true);
            _nightGameobjects.SetActive(true);
        }
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

        CheckDayAndNight();
        lastCheckedHour = currentHour;
    }


    private void CheckDayAndNight()
    {
        if (IsNight())
        {
            _lightingManager2D.SetActive(true);
            _nightGameobjects.SetActive(true);
        }
        else
        {
            _lightingManager2D.SetActive(false);
            _nightGameobjects.SetActive(false);
        }
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

    private void FixedUpdate()
    {
        CheckCharacterOutOfBound();
        UpdateAllDirectionArrows();
    }

    private bool IsPositionVisible(Vector3 worldPosition)
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
        float buffer = padding / 100f; // Convert padding to normalized viewport space

        // Check if the point is within the visible area with padding
        return viewportPosition.x >= (0 + buffer) &&
               viewportPosition.x <= (1 - buffer) &&
               viewportPosition.y >= (0 + buffer) &&
               viewportPosition.y <= (1 - buffer) &&
               viewportPosition.z > 0; // In front of camera
    }

    private void CheckCharacterOutOfBound()
    {
        foreach (var entry in pairDict)
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

        // Get normalized direction from center of screen to bubble
        Vector2 direction = new Vector2(viewportPosition.x - 0.5f, viewportPosition.y - 0.5f).normalized;

        // Calculate intersection with screen edge
        float bufferSpace = padding / 100f; // Convert padding to normalized viewport space
        float maxX = 1f - bufferSpace;
        float maxY = 1f - bufferSpace;
        float minX = bufferSpace;
        float minY = bufferSpace;

        // Determine which screen edge to place the arrow on
        float m = direction.y / direction.x; // Slope

        // Intersection with right or left edge
        float x, y;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Intersects with left or right edge
            x = direction.x > 0 ? maxX : minX;
            y = 0.5f + m * (x - 0.5f);

            // Clamp y value to be within screen bounds
            y = Mathf.Clamp(y, minY, maxY);
        }
        else
        {
            // Intersects with top or bottom edge
            y = direction.y > 0 ? maxY : minY;
            x = 0.5f + (y - 0.5f) / m;

            // Handle division by zero (vertical direction)
            if (float.IsNaN(x) || float.IsInfinity(x))
            {
                x = 0.5f;
            }

            // Clamp x value to be within screen bounds
            x = Mathf.Clamp(x, minX, maxX);
        }

        // Convert viewport position back to world position
        arrowPosition = mainCamera.ViewportToWorldPoint(new Vector3(x, y, bubblePosition.z));
        arrowPosition.z = iconWithPosition.directionArrow.transform.position.z; // Maintain original z position

        // Update arrow position
        iconWithPosition.directionArrow.transform.position = arrowPosition;

        // // Update arrow rotation to point toward character
        // Vector2 pointDirection = (Vector2)bubblePosition - (Vector2)arrowPosition;
        // float angle = Mathf.Atan2(pointDirection.y, pointDirection.x) * Mathf.Rad2Deg;
        // iconWithPosition.directionArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
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