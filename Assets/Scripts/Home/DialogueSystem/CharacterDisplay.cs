using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDisplay : MonoBehaviour
{
    public static CharacterDisplay Instance { get; private set; }

    public event Action OnCloseDialogue;
    public event  Action OnCharacterInteracted;
    public bool IsActiveCharacter = false;

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

    private const string mockupDialogue = "Hi! How are you today? I have a small problem, can you help me?";
    private const string rejectDialogue = "This quest is too hard for you, I will ask someone else.";
    public Sympathy sympathy;

    public Dictionary<int, string> dialogueOfLevel = new Dictionary<int, string>()
    {
        { 0, "I need some <sprite index=2>, could you help me to collect them?" },
        { 1, "I need some <sprite index=4> and <sprite index=7> to make a cake." },
        { 2, "Can you get me some butter and vanilla extract?" },
        { 3, "I also need some baking powder and milk." }
    };

    public event Action<Sympathy, CharacterState> OnCharacterStateChanged;

    private ICharacterState currentState;
    private CharacterState state;
    private readonly IdleState idleState = new IdleState();
    private readonly TalkingState talkingState = new TalkingState();
    private readonly GreetingState emotionState = new GreetingState();


    public float Timer = 0;
    public float TimeToChangeState = 5; //random change to emotion state


    private void Start()
    {
    }

    public string GetDialogue() => mockupDialogue;

    public string GetDialogue(int index) => dialogueOfLevel.ContainsKey(index) ? dialogueOfLevel[index] : string.Empty;

    public string GetRejectDialogue() => rejectDialogue;


    public void TransitionToState(CharacterState newState)
    {
        currentState?.Exit(this);
        state = newState;
        OnCharacterStateChanged?.Invoke(sympathy, state);
    }

    public void CloseDialogue()
    {
        OnCloseDialogue?.Invoke();
    }
    
    public void OpenDialogue()
    {
        OnCharacterInteracted?.Invoke();
    }
    
    


    private void Update()
    {
        if (!IsActiveCharacter) return;
        Timer += Time.deltaTime;
        if (Timer >= TimeToChangeState)
        {
            Debug.Log("Random Emotion");
            TransitionToState(CharacterState.Talking);
            Timer = 0;
        }
    }
}


public interface ICharacterState
{
    void Enter(CharacterDisplay characterDisplay);
    void Exit(CharacterDisplay characterDisplay);
}

public enum CharacterState
{
    Idle,
    Talking,
    Greeting,
    AfterGreeting,
}


public class IdleState : ICharacterState
{
    public void Enter(CharacterDisplay characterDisplay)
    {
    }


    public void Exit(CharacterDisplay characterDisplay)
    {
    }
}

public class TalkingState : ICharacterState
{
    public void Enter(CharacterDisplay characterDisplay)
    {
    }


    public void Exit(CharacterDisplay characterDisplay)
    {
    }
}

public class GreetingState : ICharacterState
{
    public void Enter(CharacterDisplay characterDisplay)
    {
    }


    public void Exit(CharacterDisplay characterDisplay)
    {
    }
}


public enum Sympathy
{
    Low,
    Medium,
    High
}