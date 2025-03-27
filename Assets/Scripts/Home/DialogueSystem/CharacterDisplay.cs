using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDisplay : MonoBehaviour
{
    public static CharacterDisplay Instance { get; private set; }
    public event Action OnCloseDialogue;
    public bool IsActiveCharacter = false;
    public CharacterDialogueSO characterDialogueSO;

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


    public event Action<CharacterState> OnCharacterStateChanged;
    public CharacterState state;
    public float Timer = 0;
    public float TimeToChangeState = 5; //random change to emotion state

    public string GetDialogue() => mockupDialogue;

    public string GetDialogue(int index)
    {
        return characterDialogueSO.levelDialogues[index];
    }

    public string GetRejectDialogue() => rejectDialogue;


    public void TransitionToState(CharacterState newState)
    {
        state = newState;
        if (state == CharacterState.Entry)
        {
            IsActiveCharacter = true;
        }

        if (state == CharacterState.Exit)
        {
            IsActiveCharacter = false;
        }

        OnCharacterStateChanged?.Invoke(state);
    }

    public void CloseDialogue()
    {
        OnCloseDialogue?.Invoke();
    }



    private void Update()
    {
        if (!IsActiveCharacter || state == CharacterState.Entry) return;
        Timer += Time.deltaTime;
        if (Timer >= TimeToChangeState)
        {
            Debug.Log("new state Change");
            TransitionToState(CharacterState.Idle);
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
    Entry,
    Exit
}