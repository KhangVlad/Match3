using UnityEngine;
using Match3.Enums;

public class CharacterDirectionArrow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Vector2 Pos;
    public CharacterID id;

    public void Initialize(Sprite sprite, Vector2 position, CharacterID chardId)
    {
        spriteRenderer.sprite = sprite;
        Pos = position;
        id = chardId;
    }
}   