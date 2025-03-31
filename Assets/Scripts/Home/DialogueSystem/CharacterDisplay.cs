// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class CharacterDisplay : MonoBehaviour
// {
//     public static CharacterDisplay Instance { get; private set; }
//     public CharacterDialogueSO characterDialogueSO;
//     public bool IsActiveCharacter = false;
//     public event Action<CharacterState, AngryState> OnCharacterStateChanged;
//     public CharacterState state;
//     public float Timer = 0;
//     public float TimeToChangeState = 5; //random change to emotion state
//     public float AngryPoint = 0; // on touch +5, on time -2 per second
//     public bool IsAngry = false;
//     private int[] AngryThreshold = { 1, 20, 40 };
//     public AngryState angryState; //if >1 then angry 
//     //if >1 then angry
//
//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
//
//     private void Start()
//     {
//         ScreenInteraction.Instance.OnCharacterInteracted += LoadCharacterDialogue;
//     }
//
//     private void OnDestroy()
//     {
//         ScreenInteraction.Instance.OnCharacterInteracted -= LoadCharacterDialogue;
//     }
//
//     private void LoadCharacterDialogue(CharacterID id)
//     {
//         characterDialogueSO = CharactersDataManager.Instance.GetCharacterDialogue(id);
//     }
//
//     private const string mockupDialogue = "Hi! How are you today? I have a small problem, can you help me?";
//     private const string rejectDialogue = "This quest is too hard for you, I will ask someone else.";
//
//     public string GetDialogue() => mockupDialogue;
//
//     public string GetDialogue(int index)
//     {
//         return characterDialogueSO.levelDialogues[index];
//     }
//
//     public string GetRejectDialogue() => rejectDialogue;
//
//     public void TransitionToState(CharacterState newState, AngryState ang = AngryState.None)
//     {
//         Debug.Log("Transition to state: " + newState);
//         if (state != newState || angryState != ang)
//         {
//             state = newState;
//             angryState = ang;
//
//             if (state == CharacterState.Entry)
//             {
//                 IsActiveCharacter = true;
//             }
//
//             if (state == CharacterState.Exit)
//             {
//                 IsActiveCharacter = false;
//             }
//
//             OnCharacterStateChanged?.Invoke(state, ang);
//         }
//     }
//
//     private void Update()
//     {
//         if (!IsActiveCharacter || (state == CharacterState.Entry)) return;
//         HandleAngryState();
//     }
//
//     private void HandleAngryState()
//     {
//         float previousAngryPoint = AngryPoint;
//         AngryState previousAngryState = angryState;
//
//         if (Input.GetMouseButtonDown(0))
//         {
//             AngryPoint += 5;
//             IsAngry = true;
//         }
//
//         if (IsAngry)
//         {
//             AngryPoint -= 2 * Time.deltaTime;
//             AngryPoint = Mathf.Max(0, AngryPoint); 
//         }
//
//         AngryState newAngryState = AngryState.None;
//         if (AngryPoint >= AngryThreshold[2]) newAngryState = AngryState.High;
//         else if (AngryPoint >= AngryThreshold[1]) newAngryState = AngryState.Medium;
//         else if (AngryPoint >= AngryThreshold[0]) newAngryState = AngryState.Low;
//
//         if (AngryPoint > previousAngryPoint && newAngryState != previousAngryState)
//         {
//             TransitionToState(CharacterState.Angry, newAngryState);
//         }
//
//         if (IsAngry && AngryPoint <= 0)
//         {
//             IsAngry = false;
//             TransitionToState(CharacterState.Idle, AngryState.None);
//         }
//
//         Timer += Time.deltaTime;
//         if (Timer >= TimeToChangeState)
//         {
//             Debug.Log("new state Change");
//             TransitionToState(CharacterState.Idle, AngryState.None);
//             Timer = 0;
//         }
//     }
// }
//
// public enum CharacterState
// {
//     Idle,
//     Talking,
//     Greeting,
//     Entry,
//     Exit,
//     Angry,
//     AngryIdle,
//     EngAngry,
// }
//
// public enum AngryState
// {
//     None,
//     Low,
//     Medium,
//     High,
// }

using System;
using UnityEngine;

public class CharacterDisplay : MonoBehaviour
{
    public static CharacterDisplay Instance { get; private set; }
    public CharacterDialogueSO characterDialogueSO;
    public bool IsActiveCharacter = false;
    public event Action<CharacterState, AngryState> OnCharacterStateChanged;
    public event Action<AngryState, CharacterState> OnAngryLevelChanged;

    public CharacterState state;
    public AngryState angryState;

    public float Timer = 0;
    public float TimeToChangeState = 5;
    public float AngryPoint = 0;
    public bool IsAngry = false;

    private readonly int[] AngryThreshold = { 1, 20, 40 };
    private const float AngryDecayRate = 2f;


    private const string mockupDialogue = "Hi! How are you today? I have a small problem, can you help me?";
    private const string rejectDialogue = "This quest is too hard for you, I will ask someone else.";
    public bool WaitingVideoEnd = false;
    public string GetDialogue() => mockupDialogue;

    public string GetDialogue(int index)
    {
        return characterDialogueSO.levelDialogues[index];
    }

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
        if (state == newState) return;
        state = newState;
        IsActiveCharacter = (state != CharacterState.Exit);
        OnCharacterStateChanged?.Invoke(state, angryState);
    }

    private void Update()
    {
        if (!IsActiveCharacter || state == CharacterState.Entry) return;
        HandleAngryState();
    }

    // private void HandleAngryState()
    // {
    //     float previousAngryPoint = AngryPoint;
    //     AngryState previousAngryState = angryState;
    //
    //     if (Input.GetMouseButtonDown(0)) IncreaseAnger(5);
    //     if (IsAngry) DecreaseAnger(Time.deltaTime * AngryDecayRate);
    //
    //     AngryState newAngryState = GetAngryStateFromPoint(AngryPoint);
    //
    //     if (AngryPoint > previousAngryPoint && newAngryState != previousAngryState) // increasing anger
    //     {
    //         TransitionToState(CharacterState.Angry);
    //     }
    //     //transition to end angry state if angry point is decreasing to threshold 
    //
    //     if (AngryPoint < previousAngryPoint && newAngryState != previousAngryState)
    //     {
    //         Debug.Log("AAAAAA");
    //         TransitionToState(CharacterState.EngAngry);
    //     }
    //
    //     else if (IsAngry && AngryPoint <= 0)
    //     {
    //         IsAngry = false;
    //         TransitionToState(CharacterState.Idle);
    //     }
    // }


    private void HandleAngryState()
    {
        float previousAngryPoint = AngryPoint;
        AngryState previousAngryState = angryState;

        if (Input.GetMouseButtonDown(0)) IncreaseAnger(5);
        if (IsAngry) DecreaseAnger(Time.deltaTime * AngryDecayRate);

        AngryState newAngryState = GetAngryStateFromPoint(AngryPoint);

        // If anger is increasing and state is changing
        if (AngryPoint > previousAngryPoint && newAngryState != previousAngryState)
        {
            TransitionToState(CharacterState.Angry);
        }
        // Transition to EngAngry only if anger is decreasing from Angry state
        else if (state == CharacterState.Angry && AngryPoint < previousAngryPoint &&
                 newAngryState != previousAngryState)
        {
            Debug.Log("Transitioning to EngAngry...");
            TransitionToState(CharacterState.EngAngry);
        }
        // If anger completely decays, move to Idle
        else if (IsAngry && AngryPoint <= 0)
        {
            IsAngry = false;
            TransitionToState(CharacterState.Idle);
        }
    }

    private void HandleAngryStateChange(AngryState newAngryState)
    {
        if (newAngryState != angryState)
        {
            Debug.Log("Angry state changed to: " + newAngryState + " from " + state);
            angryState = newAngryState;
            OnAngryLevelChanged?.Invoke(angryState, state);
            WaitingVideoEnd = false;
        }
    }

    private void IncreaseAnger(float amount)
    {
        AngryPoint += amount;
        IsAngry = true;
        // Clamp the value to the maximum threshold
    }

    private void DecreaseAnger(float amount)
    {
        AngryPoint = Mathf.Max(0, AngryPoint - amount);
        HandleAngryStateChange(GetAngryStateFromPoint(AngryPoint));
    }


    public void CheckStateAfterVideoPlay()
    {
        if (!WaitingVideoEnd)
        {
            //check angry state

            AngryState s = GetAngryStateFromPoint(AngryPoint);
            if (s != AngryState.None)
            {
                TransitionToState(CharacterState.Angry);
            }
            else
            {
                TransitionToState(CharacterState.Idle);
            }

            WaitingVideoEnd = true;
        }
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
    None,
    Low,
    Medium,
    High,
}