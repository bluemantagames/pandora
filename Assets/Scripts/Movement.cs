using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Priority_Queue;

public class Movement : MonoBehaviour
{
    Rigidbody2D body;
    float speed = 1f;
    bool calculationDone = false;

    public MapListener map;

    /**
     * Very simple and probably shitty and not at all optimized A* implementation
     */
    List<Vector2> FindPath(Vector2 end)
    {
        var evaluatedPositions = new HashSet<Vector2>();
        var priorityQueue = new SimplePriorityQueue<List<Vector2>>();
        var currentPosition = map.WorldPositionGridCell(transform.position);

        var evaluatingPosition = 
            new List<Vector2> { map.WorldPositionGridCell(transform.position) };

        Vector2 item;

        // get the last item in the queue
        while ((item = evaluatingPosition.Last()) != end )
        {
            var positionsCount = evaluatingPosition.Count();
            Vector2? lastPosition = (positionsCount >= 2) ? evaluatingPosition[positionsCount - 2] : (null as Vector2?);

            // check all surrounding positions
            for (var x = -1f; x <= 1f; x++)
            {
                for (var y = -1f; y <= 1; y++)
                {
                    var advance = new Vector2(item.x + x, item.y + y);

                    if (advance != item && !map.IsObstacle(advance) && advance != lastPosition) // except the current positions, obstacles or going back
                    {
                        var distanceToEnd = Vector2.Distance(advance, end); // use the distance between this point and the end as h(n)
                        var distanceFromStart = Vector2.Distance(currentPosition, advance); // use the distance between this point and the start as g(n)
                        var priority = distanceFromStart + distanceToEnd; // priority is h(n) ++ g(n)
                        var currentPositions = new List<Vector2>(evaluatingPosition) { advance };

                        priorityQueue.Enqueue(
                            currentPositions,
                            priority
                        );
                    }
                }
            }

            evaluatingPosition = priorityQueue.Dequeue();
        }

        return evaluatingPosition;
    }

    // Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (!calculationDone)
        {
            Debug.Log("Found path " + string.Join(",", FindPath(new Vector2(2, 8))));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 position = transform.position;

        position.y = position.y + (Time.deltaTime * speed);

        body.MovePosition(position);
    }
}
