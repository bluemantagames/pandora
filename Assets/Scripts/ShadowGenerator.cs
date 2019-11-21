using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

public class ShadowGenerator : MonoBehaviour
{
    public GameObject Spawner;
    public GameObject SingleShadow;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!Spawner || !SingleShadow) {
            return;
        }

        var spawnerBehaviour = Spawner.GetComponent<SpawnerBehaviour>();
        var positions = spawnerBehaviour.Positions;
        
        var cellH = MapComponent.Instance.cellHeight;
        var cellW = MapComponent.Instance.cellWidth;

        foreach(var position in positions) {
            var newShadow = Instantiate(SingleShadow);
            var newPosition = new Vector3(position.x * cellW, position.y * cellH, 0);

            newShadow.transform.parent = gameObject.transform;
            newShadow.transform.localPosition = newPosition;
        }
    }
}
