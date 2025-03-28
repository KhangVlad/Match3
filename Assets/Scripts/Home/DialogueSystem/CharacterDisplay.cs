using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDisplay : MonoBehaviour
{
    public static CharacterDisplay Instance { get; private set; }
    public CharacterDialogueSO characterDialogueSO;
    public bool IsActiveCharacter = false;
    public event Action<CharacterState> OnCharacterStateChanged;
    public CharacterState state;
    public float Timer = 0;
    public float TimeToChangeState = 5; //random change to emotion state

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


    private const string mockupDialogue = "Hi! How are you today? I have a small problem, can you help me?";
    private const string rejectDialogue = "This quest is too hard for you, I will ask someone else.";


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

public enum CharacterState
{
    Idle,
    Talking,
    Greeting,
    Entry,
    Exit
}