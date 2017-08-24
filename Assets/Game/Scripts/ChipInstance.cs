using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChipInstance : MonoBehaviour
{
    public ChipValue Value => value;

    [SerializeField]
    private ChipValue value;

    private bool isDragging;
    private GameObject activeChipGameObject;

    private void OnMouseDown()
    {
        if (GameController.Instance.IsPaused || GameController.Instance.IsBetFinalized) return;

        isDragging = true;
        activeChipGameObject = Create(value, transform.position, true);  
        SpriteRenderer spriteRenderer = activeChipGameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Bet Table Chip";
        spriteRenderer.sortingOrder = GameController.Instance.BetChipsCount;
    }

    private void OnMouseUp()
    {
        if (GameController.Instance.IsPaused || GameController.Instance.IsBetFinalized) return;

        isDragging = false;
        RaycastHit2D ray = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (ray.collider == null || ray.collider.gameObject.name != "BetArea")
        {
            Destroy(activeChipGameObject);
            return;
        }

        activeChipGameObject.transform.position = ray.collider.bounds.center + new Vector3(Random.Range(-1f, 1f), Random.Range(-0.1f, 0.1f));
        GameController.Instance.Bet(value);
    }

    private void Update()
    {
        if (isDragging)
        {
            activeChipGameObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
        }
    }

    public static GameObject Create(ChipValue value, Vector3 position, bool getValuePrefab = false)
    {
        string name = Enum.GetName(typeof(ChipValue), value);
        if (getValuePrefab)
        {
            name += "_Value";
        }

        GameObject chipGameObject = Instantiate(Resources.Load<GameObject>($"Prefabs/Chip_{name}"), position, Quaternion.identity);
        chipGameObject.transform.SetParent(ChipContainer.ChipParent);
        return chipGameObject;
    }
}
