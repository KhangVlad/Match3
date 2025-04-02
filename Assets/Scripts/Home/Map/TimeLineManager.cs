using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
# if UNITY_EDITOR
using UnityEditor;
#endif

public class TimeLineManager : MonoBehaviour
{
    public static TimeLineManager Instance { get; private set; }

    [SerializeField] public DayInWeek currentDay;
    [SerializeField] private int currentHour = 25;
    [HideInInspector] [SerializeField] private CharacterBubble bubblePrefab;
    [SerializeField] private SpriteRenderer map;
    [HideInInspector] [SerializeField] private CharacterDirectionArrow directionArrowPrefab;
    [HideInInspector] [SerializeField] private BoxCollider2D cameraCollider;
    [HideInInspector] [SerializeField] private float padding = 1f;
    public GameObject LightingManager2D;
    public GameObject Moon;
    private List<CharacterActivitySO> activeInDay = new();

    private List<CharacterID> homeIds = new();
    private List<CharacterID> activeIds = new();
    private int lastCheckedHour = -1;

    private static readonly Dictionary<DayOfWeek, DayInWeek> DayMapping = new()
    {
        { DayOfWeek.Monday, DayInWeek.Monday },
        { DayOfWeek.Tuesday, DayInWeek.Tuesday },
        { DayOfWeek.Wednesday, DayInWeek.Wednesday },
        { DayOfWeek.Thursday, DayInWeek.Thursday },
        { DayOfWeek.Friday, DayInWeek.Friday },
        { DayOfWeek.Saturday, DayInWeek.Saturday },
        { DayOfWeek.Sunday, DayInWeek.Sunday }
    };

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
        if (Input.GetMouseButtonDown(0) && IsCreatingNewActivity)
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
        if (Instance == null)
        {
            Instance = this;
            currentDay = GetCurrentDay();
            GetCurrentHour();
        }
        else
        {
            Destroy(gameObject);
        }
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
        activeInDay = CharactersDataManager.Instance.GetCharacterActive(currentDay);
    }

    // private void OnValidate()
    // {
    //     if (Application.isPlaying && currentHour != lastCheckedHour)
    //     {
    //         UpdateTimeChange();
    //         GetCharactersInTime(activeInDay);
    //     }
    // }

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
        if (currentHour >= 24)
        {
            currentHour = 0;
            currentDay = currentDay == DayInWeek.Sunday ? DayInWeek.Monday : currentDay + 1;
            CheckNewDay();
        }

        if (currentHour < 0)
        {
            currentHour = 23;
            currentDay = currentDay == DayInWeek.Monday ? DayInWeek.Sunday : currentDay - 1;
            CheckNewDay();
        }

        lastCheckedHour = currentHour;
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
        ActivityInfo activity = data.activityInfos.FirstOrDefault(a =>
            a.dayOfWeek == currentDay &&
            a.startTime <= currentHour &&
            a.endTime > currentHour);

        if (activity != null)
        {
            Vector2 mapPos = map.ImagePixelToWorld(new Vector2(activity.appearPosition.x, activity.appearPosition.y));
            CharacterBubble bubble = Instantiate(bubblePrefab, mapPos, Quaternion.identity);
            bubble.Initialize(data.id, data.sprite, mapPos);
            RegisterCharacterIcon(data.id, bubble);
            activeIds.Add(data.id);
        }
    }


    private void GetCurrentHour()
    {
        currentHour = DateTime.Now.Hour;
        //if > 18h LightingManager2D.SetActive(true);
        if (currentHour >= 12)
        {
            Moon.SetActive(true);
            LightingManager2D.SetActive(true);
        }else
        {
            Moon.SetActive(false);
            LightingManager2D.SetActive(false);
        }
    }


    private DayInWeek GetCurrentDay() => DayMapping[DateTime.Now.DayOfWeek];


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
        paddedBounds.Expand(padding * 2); // Expand bounds once per frame
        foreach (var entry in pairDict)
        {
            IconWithPosition iconWithPosition = entry.Value;
            bool isOut = !paddedBounds.Contains(iconWithPosition.bubble.transform.position);

            if (isOut != iconWithPosition.isOut)
            {
                iconWithPosition.isOut = isOut;
                if (isOut)
                {
                    if (iconWithPosition.directionArrow != null)
                    {
                        iconWithPosition.directionArrow.transform.DOScale(Vector3.one, 0.2f);
                    }
                    else
                    {
                        InstantiateAndPositionIcon(iconWithPosition, entry.Value.originPosition);
                    }
                }
                else
                {
                    if (iconWithPosition.directionArrow != null)
                    {
                        iconWithPosition.directionArrow.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => { });
                    }
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
}

[Serializable]
public class IconWithPosition
{
    public CharacterBubble bubble;
    public Vector2 originPosition;
    public bool isOut;
    public CharacterDirectionArrow directionArrow;
}