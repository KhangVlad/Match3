using UnityEngine;
using UnityEngine.UI;
using Match3.Enums;

public class CharacterBubble : MonoBehaviour
{
    public CharacterID characterID;
    public Sprite sprite;
    public SpriteRenderer image;


    public void Initialize(CharacterID id, Sprite s , Vector2 position)
    {
        characterID = id;
        sprite = s;
        transform.position = position;
        image.sprite = sprite;
    }
}