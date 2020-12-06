using UnityEngine;
using Pandora;
using Pandora.UI;

public class SpellRadiusBehaviour : MonoBehaviour
{
    public GameObject CircleIndicator;
    public int CellRadius = 3;

    GridCell gridCellPosition
    {
        get => MapComponent.Instance.WorldPositionToGridCell(transform.position);
    }

    void Start()
    {
        var circle = Instantiate(CircleIndicator, transform, false);

        var radiusEngineUnits = MapComponent.Instance.engine.UnitsPerCell * CellRadius;

        circle.GetComponent<CircleIndicatorBehaviour>().Initialize(radiusEngineUnits);
    }

}