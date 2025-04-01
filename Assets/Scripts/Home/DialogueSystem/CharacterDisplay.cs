using System;
using UnityEngine;
using System.Linq;

public class CharacterDisplay : MonoBehaviour
{
    public static CharacterDisplay Instance { get; private set; }
    public CharacterDialogueSO characterDialogueSO;
    public bool IsActiveCharacter = false;
    public event Action<CharacterState, AngryState> OnCharacterStateChanged;
    public event Action<AngryState, bool> OnAngryLevelChanged; //bool is state change by increase by time or on touch

    public CharacterState state;
    public AngryState angryState;
    public float AngryPoint = 0;
    public bool IsAngry => AngryPoint > 0;
    private readonly int[] AngryThreshold = { 1, 20, 40 };
    private const float AngryDecayRate = 2f;
    private const string mockupDialogue = "Hi! How are you today? I have a small problem, can you help me?";
    private const string rejectDialogue = "This quest is too hard for you, I will ask someone else.";

    private const string notEnoughSympathyDialogue =
        "I don't think you are the right person to help me now, but i will ask you later.";

    public float TimeToDecreaseAngryPoint = 10; //after 10s not touch, decrease angry point
    private float lastInteractionTime; // Track last interaction time

    public bool IsRecovering;
    public string GetDialogue() => mockupDialogue;

    public string GetDialogue(int level, int subLevel)
    {
        return characterDialogueSO.data[level].dialog[subLevel];
    }

    public string GetNotEnoughSympathyDialogue() => notEnoughSympathyDialogue;

    public string GetRejectDialogue() => rejectDialogue;

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
        lastInteractionTime = Time.time;
        ScreenInteraction.Instance.OnCharacterInteracted += LoadCharacterDialogue;
    }

    private void OnDestroy()
    {
        ScreenInteraction.Instance.OnCharacterInteracted -= LoadCharacterDialogue;
    }

    private void LoadCharacterDialogue(CharacterID id)
    {
        characterDialogueSO = CharactersDataManager.Instance.GetCharacterDialogue(id);
    }

    public void TransitionToState(CharacterState newState)
    {
        state = newState;
        IsActiveCharacter = (state != CharacterState.Exit);
        OnCharacterStateChanged?.Invoke(state, angryState);
    }

    private void Update()
    {
        // if (!IsActiveCharacter || state == CharacterState.Entry) return;
        // HandleAngryState();

        if (!IsActiveCharacter || state == CharacterState.Entry) return;
        HandleAngryState();

        // Check if it's time to start decreasing anger
        if (Time.time - lastInteractionTime > TimeToDecreaseAngryPoint)
        {
            IsRecovering = true;
            DecreaseAnger(Time.deltaTime * AngryDecayRate);
        }
    }


    private void HandleAngryState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IncreaseAnger(5);
            IsRecovering = false;
            lastInteractionTime = Time.time; // Reset interaction timer
        }

        if (IsAngry && IsRecovering)
        {
            AngryState previousState = angryState;
            if (previousState != GetAngryStateFromPoint(AngryPoint))
            {
                Debug.Log("Angry state changed" + GetAngryStateFromPoint(AngryPoint) + " " + state);
                angryState = GetAngryStateFromPoint(AngryPoint);
                state = CharacterState.EngAngry;
                TransitionToState(CharacterState.EngAngry);
            }
        }
    }

    public void PlayCurrentState()
    {
        if (IsAngry)
        {
            TransitionToState(CharacterState.Angry);
        }
        else
        {
            TransitionToState(CharacterState.Idle);
        }
    }


    private void IncreaseAnger(float amount)
    {
        AngryPoint += amount;
        AngryState previousState = angryState;

        if (previousState != GetAngryStateFromPoint(AngryPoint))
        {
            angryState = GetAngryStateFromPoint(AngryPoint);
            state = CharacterState.Angry;
            TransitionToState(CharacterState.Angry);
        }


        // Clamp the value to the maximum threshold
    }

    private void DecreaseAnger(float amount)
    {
        AngryPoint = Mathf.Max(0, AngryPoint - amount);
    }


    private AngryState GetAngryStateFromPoint(float point)
    {
        if (point >= AngryThreshold[2]) return AngryState.High;
        if (point >= AngryThreshold[1]) return AngryState.Medium;
        if (point >= AngryThreshold[0]) return AngryState.Low;
        return AngryState.None;
    }
}

public enum CharacterState
{
    Idle,
    Talking,
    Greeting,
    Entry,
    Exit,
    Angry,
    EngAngry,
}

public enum AngryState
{
    None = 100,
    Low = 0,
    Medium = 1,
    High = 2,
}