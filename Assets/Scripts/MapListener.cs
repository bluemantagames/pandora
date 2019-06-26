using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapListener : MonoBehaviour
{
    Vector2 mapSize = new Vector2(16, 13);
    Sprite sprite;
    GameObject lastPuppet;
    HashSet<Vector2> obstaclePositions;

    public void Awake()
    {
        sprite = GetComponent<SpriteRenderer>().sprite;

        var firstTowerPosition = new Vector2(1, 3);
        var secondTowerPosition = new Vector2(1, 3);

        obstaclePositions =
            GetTowerPositions(firstTowerPosition);

        obstaclePositions.UnionWith(
            GetTowerPositions(secondTowerPosition)
        );
    }

    /**
     * Returns whether the position is uncrossable 
     */
    public bool IsObstacle(Vector2 position)
    {
        var isOutOfBounds = (position.x < 0 && position.y < 0 && position.x > mapSize.x && position.y > mapSize.y);
        var isTower = obstaclePositions.Contains(position);

        return isTower || isOutOfBounds;
    }
    
    public Vector2 WorldPositionToGridCell(Vector2 position)
    {
        Vector2 spritePosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(position);

        Vector2 gridPosition =
            new Vector2(
                screenPosition.x - spritePosition.x,
                screenPosition.y - spritePosition.y
            );

        float cellHeight = sprite.rect.height / mapSize.y;
        float cellWidth = sprite.rect.width / mapSize.x;

        Vector2 cellPosition = new Vector2(
            Mathf.Floor(gridPosition.x / cellWidth),
            Mathf.Floor(gridPosition.y / cellHeight)
        );

        return cellPosition;
    }

    public Vector2 GridCellToWorldPosition(Vector2 cell)
    {
        Vector2 spritePosition = Camera.main.WorldToScreenPoint(transform.position);

        float cellHeight = sprite.rect.height / mapSize.y;
        float cellWidth = sprite.rect.width / mapSize.x;

        Vector2 screenPoint = new Vector2(
            spritePosition.x + (cell.x * cellWidth),
            spritePosition.y + (cell.y * cellHeight)
        );

        Debug.Log("Cell " + cell);
        Debug.Log("Screen point " + Camera.main.ScreenToWorldPoint(screenPoint));

        return Camera.main.ScreenToWorldPoint(screenPoint);
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

    private HashSet<Vector2> GetTowerPositions(Vector2 towerPosition, float towerSize = 3f)
    {
        var set = new HashSet<Vector2>();

        for (var x = 0f; x < towerSize; x++)
        {
            for (var y = 0f; y < towerSize; y++)
            {
                set.Add(new Vector2(towerPosition.x + x, towerPosition.y + y));
            }
        }

        Debug.Log("Tower positions " + string.Join(",", set));

        return set;
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
}
