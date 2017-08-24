using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class ScaleToScreenSize : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite previousFrameSprite;

    private Vector2 screenSize;
    [SerializeField]
    private float multiplier = 1f;

    // Use this for initialization
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Scale();
    }

    // Update is called once per frame
    private void Update()
    {
        Vector2 currentSize = GetScreenSize();
        if ((previousFrameSprite != spriteRenderer.sprite || currentSize != screenSize) && spriteRenderer.sprite != null)
        {
            Scale();
        }

        previousFrameSprite = spriteRenderer.sprite;
    }

    private void Scale()
    {
        Vector2 currentSize = GetScreenSize();
        transform.localScale = Vector3.one;
        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;

        Vector3 localScale = transform.localScale;
        localScale.x = currentSize.x / width;
        localScale.y = currentSize.y / height;

        transform.localScale = localScale * multiplier;
        screenSize = currentSize;
    }

    private static Vector2 GetScreenSize()
    {
        float height = Camera.main.orthographicSize * 2f;
        float width = height / Screen.height * Screen.width;

        return new Vector2(width, height);
    }


}

