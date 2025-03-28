using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UnityEngine.Serialization;

public class TimeLineManager : MonoBehaviour
{
    public static TimeLineManager Instance { get; private set; }


    [SerializeField] private DayInWeek currentDay;
    [SerializeField] private int currentHour = 25;
    [SerializeField] private CharacterBubble bubblePrefab;
    [SerializeField] private SpriteRenderer map;
    [SerializeField] private CharacterDirectionArrow directionArrowPrefab;
    [SerializeField] private BoxCollider2D cameraCollider;
    [SerializeField] private float padding = 1f;

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
    }

    private void GetCharacterActiveToday()
    {
        activeInDay = CharactersDataManager.Instance.GetCharacterActive(currentDay);
        GetCharactersInTime(activeInDay);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && currentHour != lastCheckedHour)
        {
            UpdateTimeChange();
            GetCharactersInTime(activeInDay);
        }
    }

    private void UpdateTimeChange()
    {
        if (currentHour >= 24)
        {
            currentHour = 0;
            currentDay = currentDay == DayInWeek.Sunday ? DayInWeek.Monday : currentDay + 1;
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

    // private void InitializeCharacterToWorld(CharacterActivitySO data)
    // {
    //     for (int i = 0; i < data.activityInfos.Length; i++)
    //     {
    //         if (data.activityInfos[i].dayOfWeek == currentDay &&
    //             data.activityInfos[i].startTime <= currentHour &&
    //             data.activityInfos[i].endTime > currentHour)
    //         {
    //             Vector2 mapPos = map.ImagePixelToWorld(new Vector2(
    //                 data.activityInfos[i].appearPosition.x,
    //                 data.activityInfos[i].appearPosition.y));
    //             CharacterIcon icon = Instantiate(iconPrefab, mapPos, Quaternion.identity);
    //             icon.Initialize(data.id, data.sprite, mapPos);
    //             RegisterCharacterIcon(data.id, icon);
    //             activeIds.Add(data.id);
    //         }
    //     }
    // }
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