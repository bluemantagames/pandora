using UnityEngine;
using System.Collections;

public class MapListener : MonoBehaviour
{
    Vector2 mapSize = new Vector2(16, 13);
    Sprite sprite;
    GameObject lastPuppet;

    public void Awake()
    {
        sprite = GetComponent<SpriteRenderer>().sprite;
    }

    private Vector2 GetPointedCell()
    {
        Vector2 position = Camera.main.WorldToScreenPoint(transform.position);

        Vector2 mousePosition =
            new Vector2(
                Input.mousePosition.x - position.x,
                Input.mousePosition.y - position.y
            );

        Debug.Log(
            "Rect" + position
        );


        Debug.Log(
            "Mouse " + Input.mousePosition
        );


        float cellHeight = sprite.rect.height / mapSize.y;
        float cellWidth = sprite.rect.width / mapSize.x;

        Vector2 cellPosition = new Vector2(
            Mathf.Floor(mousePosition.x / cellWidth),
            Mathf.Floor(mousePosition.y / cellHeight)
        );

        return cellPosition;
    }

    private Vector2 GetWorldPointedCell()
    {

        var cell = GetPointedCell();

        float cellHeight = sprite.rect.height / mapSize.y;
        float cellWidth = sprite.rect.width / mapSize.x;

        var worldCellPoint = Camera.main.WorldToScreenPoint(transform.position);

        worldCellPoint.x += cellWidth * cell.x + (cellWidth / 2);
        worldCellPoint.y += cellHeight * cell.y + (cellHeight / 2);
        worldCellPoint.z = 1;

        return Camera.main.ScreenToWorldPoint(worldCellPoint);
    }

    public void Update()
    {
    }

    public void DestroyPuppet()
    {
        if (lastPuppet != null)
            Destroy(lastPuppet);
    }

    public void SpawnCard(GameObject card)
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 1;

        var cell = GetWorldPointedCell();

        Instantiate(card, cell, Quaternion.identity);
    }

    public void OnUICardCollision(GameObject puppet)
    {
        DestroyPuppet();

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 1;

        var cell = GetWorldPointedCell();

        lastPuppet = Instantiate(puppet, cell, Quaternion.identity);
    }
}
