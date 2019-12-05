using UnityEngine;
using Pandora;

public class SpellRadiusBehaviour : MonoBehaviour
{
    public GameObject CellCover;
    public int CellRadius = 3;

    GridCell currentPosition = null;

    GridCell gridCellPosition
    {
        get => MapComponent.Instance.WorldPositionToGridCell(transform.position);
    }

    void Update()
    {
        if (currentPosition == gridCellPosition) return;

        currentPosition = gridCellPosition;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (var x = 0; x < MapComponent.Instance.mapSizeX; x++)
        {
            for (var y = 0; y < MapComponent.Instance.mapSizeY; y++)
            {
                var cell = new GridCell(x, y);

                Logger.Debug($"Checking {currentPosition}");

                var distance = Vector2.Distance(currentPosition.vector, cell.vector);

                if (distance < CellRadius)
                {
                    var coverPosition = (Vector3) MapComponent.Instance.GridCellToWorldPosition(cell);

                    coverPosition.z = -1;

                    var cover = Instantiate(CellCover, coverPosition, Quaternion.identity, transform);

                    cover.GetComponent<Renderer>().sortingOrder = 100;
                }
            }
        }

    }

}