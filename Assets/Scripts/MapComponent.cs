using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Pandora.Movement;
using Pandora.Spell;
using Pandora.Combat;
using Pandora.Network;
using Pandora.Network.Messages;

namespace Pandora
{
    public class MapComponent : MonoBehaviour
    {
        int bottomMapSizeX = 16;
        int bottomMapSizeY = 13;
        int mapSizeX;
        int mapSizeY;
        Vector2 bottomMapSize;
        GameObject lastPuppet;
        HashSet<GridCell> obstaclePositions;
        float firstLaneX = 2, secondLaneX = 13;

        float cellHeight;
        float cellWidth;

        public GameObject textObject;

        public void Awake()
        {
            mapSizeX = bottomMapSizeX;
            mapSizeY = (bottomMapSizeY * 2) + 1;

            bottomMapSize = new Vector2(bottomMapSizeX, bottomMapSizeY);

            Screen.fullScreen = false;
            Screen.SetResolution(1080, 1920, false);

            var topArena = GameObject.Find("top_arena");
            var topArenaPosition = topArena.transform.position;
            var topArenaSize = topArena.GetComponent<SpriteRenderer>().bounds.size;

            Debug.Log($"Top arena position y {topArenaPosition.y}");
            Debug.Log($"Top arena position {(topArenaPosition.y + topArenaSize.y)}");

            cellWidth = topArenaSize.x / mapSizeX;
            cellHeight = ((topArenaPosition.y + topArenaSize.y) - transform.position.y) / mapSizeY;

            var firstTowerPosition = new GridCell(new Vector2(1, 3));
            var secondTowerPosition = new GridCell(new Vector2(12, 3));
            var thirdTowerPosition = new GridCell(new Vector2(1, 21));
            var fourthTowerPosition = new GridCell(new Vector2(12, 21));

            obstaclePositions =
                GetTowerPositions(firstTowerPosition);

            obstaclePositions.UnionWith(
                GetTowerPositions(secondTowerPosition)
            );

            obstaclePositions.UnionWith(
                GetTowerPositions(thirdTowerPosition)
            );

            obstaclePositions.UnionWith(
                GetTowerPositions(fourthTowerPosition)
            );

            for (var x = 0; x < mapSizeX; x++)
            {
                var gridPosition = GridCellToWorldPosition(new GridCell(new Vector2(x, mapSizeY + 1)));

                SpawnText(gridPosition, x.ToString());
            }


            for (var y = 0; y < mapSizeY; y++)
            {
                var gridPosition = GridCellToWorldPosition(new GridCell(new Vector2(-1, y)));

                SpawnText(gridPosition, y.ToString());
            }
        }

        /**
         * Returns whether the position is uncrossable 
         */
        public bool IsObstacle(GridCell cell)
        {
            var riverY = 13f;
            var riverPositions = new List<GridCell>();
            var cellVector = cell.vector;

            for (var x = 0; x < bottomMapSize.x; x++)
            {
                if (x != firstLaneX && x != secondLaneX)
                {
                    riverPositions.Add(new GridCell(x, riverY));
                }
            }

            var isOutOfBounds = (cellVector.x < 0 && cellVector.y < 0 && cellVector.x >= bottomMapSize.x && cellVector.y >= mapSizeY);
            var isTower = obstaclePositions.Contains(cell);
            var isRiver = riverPositions.Contains(cell);

            return isRiver || isTower || isOutOfBounds;
        }

        public GridCell WorldPositionToGridCell(Vector2 position)
        {

            Vector2 gridPosition =
                new Vector2(
                    position.x - transform.position.x,
                    position.y - transform.position.y
                );

            Vector2 cellPosition = new Vector2(
                Mathf.Floor(gridPosition.x / cellWidth),
                Mathf.Floor(gridPosition.y / cellHeight)
            );

            return new GridCell(cellPosition);
        }

        public Vector2 GridCellToWorldPosition(GridCell cell)
        {
            Vector2 worldPosition = new Vector2(
                transform.position.x + (cell.vector.x * cellWidth) + cellWidth / 2,
                transform.position.y + (cell.vector.y * cellHeight) + cellHeight / 2
            );

            Debug.Log($"Spawning in cell {cell}");
            Debug.Log($"World point GridCellToWorldPosition {worldPosition}");

            return worldPosition;
        }

        public void Update()
        {
            SpawnMessage spawn;

            if (NetworkControllerSingleton.instance.spawnQueue.TryDequeue(out spawn))
            {
                Debug.Log($"Received {spawn} - spawning unit");

                SpawnUnit(spawn.unitName, spawn.cellX, spawn.cellY, spawn.team);
            }
        }

        public void DestroyPuppet()
        {
            if (lastPuppet != null)
                Destroy(lastPuppet);
        }

        public void SpawnCard(string cardName, int team)
        {
            var mapCell = GetPointedCell();

            NetworkControllerSingleton.instance.EnqueueMessage(
                new SpawnMessage
                {
                    unitName = cardName,
                    cellX = (int)Math.Floor(mapCell.x),
                    cellY = (int)Math.Floor(mapCell.y),
                    team = TeamComponent.assignedTeam
                }
            );

            if (!NetworkControllerSingleton.instance.matchStarted)
            {
                SpawnUnit(cardName, (int)Math.Floor(mapCell.x), (int)Math.Floor(mapCell.y), team);
            }
        }

        public void SpawnUnit(string unitName, int cellX, int cellY, int team)
        {
            Debug.Log($"Spawning {unitName} in {cellX}, {cellY}");

            var card = Resources.Load($"Cards/{unitName}") as GameObject;

            if (team != TeamComponent.assignedTeam)
            { // flip Y if opponent
                cellY = mapSizeY - 1 - cellY;
            }

            var cardPosition = GridCellToWorldPosition(new GridCell(cellX, cellY));

            var cardObject = Instantiate(card, cardPosition, Quaternion.identity, transform);

            cardObject.GetComponent<TeamComponent>().team = team;

            var movement = cardObject.GetComponent<MovementComponent>();
            var projectileSpell = cardObject.GetComponent<ProjectileSpellBehaviour>();

            if (movement != null) movement.map = this;
            if (projectileSpell != null) projectileSpell.map = this;
        }

        public Enemy GetNearestEnemy(GameObject unit, GridCell position, int team, float range)
        {
            float? minDistance = null;
            GameObject enemy = null;

            foreach (TeamComponent component in GetComponentsInChildren<TeamComponent>())
            {
                Debug.Log($"Checking {component}");

                var targetGameObject = component.gameObject;
                var gameObjectPosition = WorldPositionToGridCell(targetGameObject.transform.position);

                var towerPositionComponent = targetGameObject.GetComponent<TowerPositionComponent>();

                if (towerPositionComponent != null)
                {
                    gameObjectPosition = towerPositionComponent.GetMapTarget();
                }

                var distance = Vector2.Distance(gameObjectPosition.vector, position.vector);
                var lifeComponent = targetGameObject.GetComponent<LifeComponent>();

                if (lifeComponent == null || lifeComponent.isDead) continue; // skip spells

                Debug.Log($"Our layer {unit.layer}");
                Debug.Log($"Target layer {targetGameObject.layer}");
                Debug.Log($"CombatBehaviour from nearest enemy {unit.GetComponent<CombatBehaviour>()}");

                var canUnitsFight = // Units can fight if:
                    (targetGameObject.layer == unit.layer) || // same layer (ground & ground, flying & flying)
                    (targetGameObject.layer == Constants.FLYING_LAYER && unit.GetComponent<CombatBehaviour>().combatType == CombatType.Ranged) || // target is flying and we are ranged
                    (unit.layer == Constants.FLYING_LAYER); // we're flying

                var isTargetValid =
                    (minDistance == null || minDistance > distance) && (distance <= range) && component.team != team && !lifeComponent.isDead && canUnitsFight;

                Debug.Log($"Distance {distance} Target valid {isTargetValid} units can fight {canUnitsFight}");

                if (isTargetValid)
                {
                    minDistance = distance;
                    enemy = targetGameObject;
                }
            }

            if (enemy != null)
            {
                return new Enemy(enemy);
            }
            else
            {
                return null;
            }
        }

        // Finds units in a gridcell-space rectangle
        public List<GameObject> GetUnitsInRect(Vector2 origin, int widthCells, int heightCells)
        {
            var units = new List<GameObject>();
            var lowerRange = Math.Min(origin.y, origin.y + heightCells);
            var higherRange = Math.Max(origin.y, origin.y + heightCells);

            foreach (var component in GetComponentsInChildren<UnitBehaviour>())
            {
                var cellVector = WorldPositionToGridCell(component.gameObject.transform.position).vector;

                Debug.Log($"Checking {cellVector} in {origin.x} / {origin.x + widthCells}");
                Debug.Log($"Checking {cellVector} in {origin.y} / {origin.y + heightCells}");

                var isDead = component.gameObject.GetComponent<LifeComponent>()?.isDead ?? true;

                if (
                    cellVector.x >= origin.x &&
                    cellVector.x <= origin.x + widthCells &&
                    cellVector.y >= lowerRange &&
                    cellVector.y <= higherRange &&
                    !isDead
                )
                {
                    units.Add(component.gameObject);
                }
            }

            return units;
        }

        public GridCell GetTarget(GameObject unit, GridCell cell, int team, float aggroRange)
        {
            GridCell? lanePosition = null;

            var cellVector = cell.vector;

            var enemyPosition = GetNearestEnemy(unit, cell, team, aggroRange)?.enemyCell;

            var isOpponent = unit.GetComponent<TeamComponent>().IsOpponent();

            TowerPosition targetTowerPosition;

            if (cellVector.x < bottomMapSizeX / 2)
            {
                targetTowerPosition = isOpponent ? TowerPosition.BottomLeft : TowerPosition.TopLeft;
            }
            else
            {
                targetTowerPosition = isOpponent ? TowerPosition.BottomRight : TowerPosition.BottomLeft;
            }

            // if no enemies found and not on a lane, go back on a lane
            if (enemyPosition == null && cellVector.x != firstLaneX && cellVector.x != secondLaneX)
            {
                float xTarget, increment;

                Vector2 targetLanePosition = cellVector;

                if (cellVector.x <= bottomMapSize.x / 2 && cellVector.x >= firstLaneX) // if in the middle and near the first lane
                {
                    xTarget = firstLaneX;
                    increment = -1f;
                }
                else if (cellVector.x < firstLaneX) // if on the left
                {
                    xTarget = firstLaneX;
                    increment = 1f;
                }
                else if (cellVector.x > bottomMapSize.x / 2 && cellVector.x > secondLaneX) // if on the right
                {
                    xTarget = secondLaneX;
                    increment = -1f;
                }
                else // if in the middle and near the second lane 
                {
                    xTarget = secondLaneX;
                    increment = 1f;
                }

                var yIncrement = isOpponent ? -1 : 1;

                while (targetLanePosition.x != xTarget)
                {
                    targetLanePosition.y += yIncrement;
                    targetLanePosition.x += increment;
                }

                lanePosition = new GridCell(targetLanePosition);
            }

            TowerPositionComponent towerPositionComponent = null, middleTowerPositionComponent = null;

            foreach (var component in GetComponentsInChildren<TowerPositionComponent>())
            {
                var combatBehaviour = component.gameObject.GetComponent<TowerCombatBehaviour>();

                if (combatBehaviour.isMiddle && combatBehaviour.isOpponent != isOpponent)
                {
                    middleTowerPositionComponent = component;
                }

                if (component.towerPosition == targetTowerPosition && !component.gameObject.GetComponent<LifeComponent>().isDead)
                {
                    towerPositionComponent = component;
                }
            }

            var towerPosition = towerPositionComponent?.GetMapTarget() ?? middleTowerPositionComponent.GetMapTarget();

            var endPosition = enemyPosition ?? lanePosition ?? towerPosition;

            Debug.Log($"Going to {endPosition} (target is {targetTowerPosition}) ({towerPositionComponent})");

            // go to enemy position, or a lane, or to the end of the world
            return endPosition;
        }

        public void OnUICardCollision(GameObject puppet)
        {
            DestroyPuppet();

            var cell = GetWorldPointedCell();

            lastPuppet = Instantiate(puppet, cell, Quaternion.identity);
        }

        private HashSet<GridCell> GetTowerPositions(GridCell towerCell, float towerSize = 3f)
        {
            var set = new HashSet<GridCell>();

            for (var x = 0f; x < towerSize; x++)
            {
                for (var y = 0f; y < towerSize; y++)
                {
                    set.Add(new GridCell(towerCell.vector.x + x, towerCell.vector.y + y));
                }
            }

            Debug.Log("Tower positions " + string.Join(",", set));

            return set;
        }

        private Vector2 GetPointedCell()
        {
            Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 mousePosition =
                new Vector2(
                    worldMouse.x - transform.position.x,
                    worldMouse.y - transform.position.y
                );

            Debug.Log(
                "Cell Rect " + transform.position
            );


            Debug.Log(
                "Cell Mouse " + worldMouse
            );

            Debug.Log($"Cell width {cellWidth}");
            Debug.Log($"Cell height {cellHeight}");
            Debug.Log($"Cell mouse position {mousePosition.x}");
            Debug.Log($"Cell position {mousePosition.x / cellWidth}");

            Vector2 cellPosition = new Vector2(
                Mathf.Floor(mousePosition.x / cellWidth),
                Mathf.Floor(mousePosition.y / cellHeight)
            );

            return cellPosition;
        }

        private Vector2 GetWorldPointedCell()
        {
            var cell = GetPointedCell();

            Debug.Log($"Pointed cell {cell}");

            var worldCellPoint = transform.position;

            worldCellPoint.x += cellWidth * cell.x + (cellWidth / 2);
            worldCellPoint.y += cellHeight * cell.y + (cellHeight / 2);
            worldCellPoint.z = 1;

            return worldCellPoint;
        }

        private void SpawnText(Vector2 position, string text)
        {
            var canvas = Instantiate(textObject, position, Quaternion.identity, transform);

            canvas.GetComponentInChildren<Text>().text = text;
        }
    }
}