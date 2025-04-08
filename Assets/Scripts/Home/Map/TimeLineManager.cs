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
    [HideInInspector] [SerializeField] private CharacterBubble bubblePrefab;
    [SerializeField] private SpriteRenderer map;
    [HideInInspector] [SerializeField] private CharacterDirectionArrow directionArrowPrefab;
    [HideInInspector] [SerializeField] private BoxCollider2D cameraCollider;
    [SerializeField] private float padding = 1f; //for bounds check
    [SerializeField] private GameObject _lightingManager2D;
    [SerializeField] private GameObject _nightGameobjects;
    private List<CharacterActivitySO> activeInDay = new();
    private List<CharacterID> homeIds = new();
    private List<CharacterID> activeIds = new();
    private int lastCheckedHour = -1;


    private Dictionary<CharacterID, IconWithPosition> pairDict = new();

    private Bounds paddedBounds;


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

#if UNITY_EDITOR
    private void Update()
    {
        if (!IsCreatingNewActivity) return;
        if (Input.GetMouseButtonDown(0))
        {
            AppearPosition = map.WorldPositionToImagePixel(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector3 worldPosition = (Vector2)map.ImagePixelToWorld(AppearPosition); // Convert Vector2Int to Vector3

            if (simulatedBubble == null)
            {
                CharacterBubble bubble = Instantiate(bubblePrefab, worldPosition, Quaternion.identity);
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(
                    $"Assets/Sprites/CharactersAvatar/{(int)EditorCharacterID}.png");
                CharacterSprite = s;
                bubble.Initialize(EditorCharacterID, s, worldPosition);
                simulatedBubble = bubble;
            }
            else
            {
                simulatedBubble.transform.position = worldPosition;
            }
        }
    }
#endif

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
    }

    private void Start()
    {
        GetCharacterActiveToday();
        AdjustColliderBounds();
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
        Debug.Log("fix");
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


    private void CheckCharacterOutOfBound()
    {
        paddedBounds = cameraCollider.bounds;
        paddedBounds.Expand(padding * 3);

        foreach (var entry in pairDict)
        {
            IconWithPosition iconWithPosition = entry.Value;
            bool isOut = !paddedBounds.Contains(iconWithPosition.bubble.transform.position);

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
        iconWithPosition.directionArrow.transform.position =
            GetClosestPointOnCameraBounds(iconWithPosition.bubble.transform.position);
    }

    private Vector2 GetClosestPointOnCameraBounds(Vector2 position)
    {
        return cameraCollider.bounds.ClosestPoint(position);
    }

    private void InstantiateAndPositionIcon(IconWithPosition iconWithPosition, Vector2 originPos)
    {
        if (iconWithPosition.directionArrow == null)
        {
            CharacterDirectionArrow arrow = Instantiate(directionArrowPrefab,
                iconWithPosition.bubble.transform.position,
                Quaternion.identity);
            iconWithPosition.directionArrow = arrow;
            arrow.Initialize(iconWithPosition.bubble.sprite, originPos, iconWithPosition.bubble.characterID);
        }
    }

    private void AdjustColliderBounds()
    {
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float newWidth = cameraCollider.size.x - (padding * screenRatio);
        float newHeight = cameraCollider.size.y - (padding * screenRatio);
        cameraCollider.size = new Vector2(newWidth, newHeight);
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