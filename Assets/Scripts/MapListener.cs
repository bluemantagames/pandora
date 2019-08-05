using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using CRclone.Movement;
using CRclone.Spell;
using CRclone.Combat;
using CRclone.Network;
using CRclone.Network.Messages;

namespace CRclone
{
    public class MapListener : MonoBehaviour
    {
        int bottomMapSizeX = 16;
        int bottomMapSizeY = 13;
        int mapSizeX;
        int mapSizeY;
        Vector2 bottomMapSize;
        Sprite sprite;
        GameObject lastPuppet;
        HashSet<Vector2> obstaclePositions;
        float firstLaneX = 2, secondLaneX = 13;

        public GameObject textObject;

        public void Awake()
        {
            mapSizeX = bottomMapSizeX;
            mapSizeY = (bottomMapSizeY * 2) + 1;

            bottomMapSize = new Vector2(bottomMapSizeX, bottomMapSizeY);

            Screen.fullScreen = false;
            Screen.SetResolution(1080, 1920, false);

            sprite = GetComponent<SpriteRenderer>().sprite;

            var firstTowerPosition = new Vector2(1, 3);
            var secondTowerPosition = new Vector2(12, 3);
            var thirdTowerPosition = new Vector2(1, 21);
            var fourthTowerPosition = new Vector2(12, 21);

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
                var gridPosition = GridCellToWorldPosition(new Vector2(x, mapSizeY + 1));

                SpawnText(gridPosition, x.ToString());
            }


            for (var y = 0; y < mapSizeY; y++)
            {
                var gridPosition = GridCellToWorldPosition(new Vector2(-1, y));

                SpawnText(gridPosition, y.ToString());
            }
        }

        /**
         * Returns whether the position is uncrossable 
         */
        public bool IsObstacle(Vector2 position)
        {
            var riverY = 13f;
            var riverPositions = new List<Vector2>();

            for (var x = 0; x < bottomMapSize.x; x++)
            {
                if (x != firstLaneX && x != secondLaneX)
                {
                    riverPositions.Add(new Vector2(x, riverY));
                }
            }

            var isOutOfBounds = (position.x < 0 && position.y < 0 && position.x >= bottomMapSize.x && position.y >= bottomMapSize.y);
            var isTower = obstaclePositions.Contains(position);
            var isRiver = riverPositions.Contains(position);

            return isRiver || isTower || isOutOfBounds;
        }

        public Vector2 WorldPositionToGridCell(Vector2 position)
        {

            float cellHeight = GetComponent<SpriteRenderer>().bounds.size.y / bottomMapSize.y;
            float cellWidth = GetComponent<SpriteRenderer>().bounds.size.x / bottomMapSize.x;

            Vector2 gridPosition =
                new Vector2(
                    position.x - transform.position.x,
                    position.y - transform.position.y
                );

            Vector2 cellPosition = new Vector2(
                Mathf.Floor(gridPosition.x / cellWidth),
                Mathf.Floor(gridPosition.y / cellHeight)
            );

            return cellPosition;
        }

        public Vector2 GridCellToWorldPosition(Vector2 cell)
        {
            float cellHeight = GetComponent<SpriteRenderer>().bounds.size.y / bottomMapSize.y;
            float cellWidth = GetComponent<SpriteRenderer>().bounds.size.x / bottomMapSize.x;

            Vector2 worldPosition = new Vector2(
                transform.position.x + (cell.x * cellWidth) + cellWidth / 2,
                transform.position.y + (cell.y * cellHeight) + cellHeight / 2
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

            var cardPosition = GridCellToWorldPosition(new Vector2(cellX, cellY));

            var cardObject = Instantiate(card, cardPosition, Quaternion.identity, transform.parent);

            cardObject.GetComponent<TeamComponent>().team = team;

            var movement = cardObject.GetComponent<MovementComponent>();
            var projectileSpell = cardObject.GetComponent<ProjectileSpellBehaviour>();

            if (movement != null) movement.map = this;
            if (projectileSpell != null) projectileSpell.map = this;
        }

        public Enemy GetNearestEnemy(GameObject unit, Vector2 position, int team, float range)
        {
            float? minDistance = null;
            GameObject enemy = null;

            foreach (TeamComponent component in transform.parent.GetComponentsInChildren<TeamComponent>())
            {
                Debug.Log($"Checking {component}");

                var targetGameObject = component.gameObject;
                var gameObjectPosition = WorldPositionToGridCell(targetGameObject.transform.position);

                var towerPositionComponent = targetGameObject.GetComponent<TowerPositionComponent>();

                if (towerPositionComponent != null)
                {
                    gameObjectPosition = towerPositionComponent.position;
                }

                var distance = Vector2.Distance(gameObjectPosition, position);
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

            foreach (var component in transform.parent.GetComponentsInChildren<UnitBehaviour>())
            {
                var cell = WorldPositionToGridCell(component.gameObject.transform.position);

                Debug.Log($"Checking {cell} in {origin.x} / {origin.x + widthCells}");
                Debug.Log($"Checking {cell} in {origin.y} / {origin.y + heightCells}");

                var isDead = component.gameObject.GetComponent<LifeComponent>()?.isDead ?? true;

                if (
                    cell.x >= origin.x &&
                    cell.x <= origin.x + widthCells &&
                    cell.y >= lowerRange &&
                    cell.y <= higherRange &&
                    !isDead
                )
                {
                    units.Add(component.gameObject);
                }
            }

            return units;
        }

        public Vector2 GetTarget(GameObject unit, Vector2 position, int team, float aggroRange)
        {
            Vector2? lanePosition = null;

            var enemyPosition = GetNearestEnemy(unit, position, team, aggroRange)?.enemyCell;

            var towerY = 20;

            var middleTowerY = 24;
            var middleTowerX = 6;

            var isOpponent = unit.GetComponent<TeamComponent>().IsOpponent();

            TowerPosition targetTowerPosition;

            if (position.x < bottomMapSizeX / 2) {
                targetTowerPosition = isOpponent ? TowerPosition.BottomLeft : TowerPosition.TopLeft;
            } else {
                targetTowerPosition = isOpponent ? TowerPosition.BottomRight : TowerPosition.BottomLeft;
            }

            // if no enemies found and not on a lane, go back on a lane
            if (enemyPosition == null && position.x != firstLaneX && position.x != secondLaneX)
            {
                float xTarget, increment;

                Vector2 targetLanePosition = position;

                if (position.x <= bottomMapSize.x / 2 && position.x >= firstLaneX) // if in the middle and near the first lane
                {
                    xTarget = firstLaneX;
                    increment = -1f;
                }
                else if (position.x < firstLaneX) // if on the left
                {
                    xTarget = firstLaneX;
                    increment = 1f;
                }
                else if (position.x > bottomMapSize.x / 2 && position.x > secondLaneX) // if on the right
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

                lanePosition = targetLanePosition;
            }

            if (isOpponent)
            { // flip the tower Y objective if opponent
                towerY = mapSizeY - 1 - towerY;
                middleTowerY = mapSizeY - 1 - middleTowerY;
            }

            var isFrontTowerPresent = false;

            foreach (var component in transform.parent.GetComponentsInChildren<TowerPositionComponent>())
            {
                if (component.towerPosition == targetTowerPosition && !component.gameObject.GetComponent<LifeComponent>().isDead)
                {

                    Debug.Log($"Going to {component.gameObject.GetComponent<LifeComponent>().isDead} {component.gameObject.GetComponent<LifeComponent>().lifeValue}");
                    isFrontTowerPresent = true;
                }
            }

            var towerPosition = (isFrontTowerPresent) ?
             new Vector2(position.x, towerY) : new Vector2(middleTowerX, middleTowerY);

            var endPosition = enemyPosition ?? lanePosition ?? towerPosition;

            Debug.Log($"Going to {endPosition} (target is {targetTowerPosition}) ({isFrontTowerPresent})");

            // go to enemy position, or a lane, or to the end of the world
            return endPosition;
        }

        public void OnUICardCollision(GameObject puppet)
        {
            DestroyPuppet();

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
                "Cell Mouse " + Input.mousePosition
            );

            Debug.Log($"Cell X bounds {GetComponent<SpriteRenderer>().bounds.size.x}");

            float cellHeight = GetComponent<SpriteRenderer>().bounds.size.y / bottomMapSize.y;
            float cellWidth = GetComponent<SpriteRenderer>().bounds.size.x / bottomMapSize.x;

            Debug.Log($"Cell width {cellWidth}");
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

            float cellHeight = GetComponent<SpriteRenderer>().bounds.size.y / bottomMapSize.y;
            float cellWidth = GetComponent<SpriteRenderer>().bounds.size.x / bottomMapSize.x;

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