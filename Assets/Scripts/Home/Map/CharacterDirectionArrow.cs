using UnityEngine;

public class CharacterDirectionArrow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Vector2 Pos;

    public void Initialize(Sprite sprite, Vector2 position)
    {
        spriteRenderer.sprite = sprite;
        Pos = position;
    }
}