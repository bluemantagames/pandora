using UnityEngine;
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
        Vector2 mapSize = new Vector2(16, 13);
        Sprite sprite;
        GameObject lastPuppet;
        HashSet<Vector2> obstaclePositions;
        int team = 0;

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
            SpawnMessage spawn;

            if (NetworkControllerSingleton.instance.spawnQueue.TryDequeue(out spawn))
            {
                Debug.Log($"Received {spawn} - spawning unit");

                SpawnUnit(spawn.unitName, spawn.cellX, spawn.cellY);
            }
        }

        public void DestroyPuppet()
        {
            if (lastPuppet != null)
                Destroy(lastPuppet);
        }

        public void SpawnCard(string cardName)
        {
            var mapCell = GetPointedCell();

            NetworkControllerSingleton.instance.EnqueueMessage(
                new SpawnMessage
                {
                    unitName = cardName,
                    cellX = (int)Math.Floor(mapCell.x),
                    cellY = (int)Math.Floor(mapCell.y)
                }
            );
        }

        public void SpawnUnit(string unitName, int cellX, int cellY)
        {
            Debug.Log($"Spawning {unitName} in {cellX}, {cellY}");

            var card = Resources.Load($"Cards/{unitName}") as GameObject;
            var cardPosition = GridCellToWorldPosition(new Vector2(cellX, cellY));

            var cardObject = Instantiate(card, cardPosition, Quaternion.identity, transform);

            cardObject.GetComponent<TeamComponent>().team = ++team;

            var movement = cardObject.GetComponent<MovementComponent>();
            var projectileSpell = cardObject.GetComponent<ProjectileSpellBehaviour>();

            if (movement != null) movement.map = this;
            if (projectileSpell != null) projectileSpell.map = this;
        }

        public Enemy GetNearestEnemy(GameObject unit, Vector2 position, int team)
        {
            float? minDistance = null;
            GameObject enemy = null;

            foreach (TeamComponent component in GetComponentsInChildren<TeamComponent>())
            {
                var targetGameObject = component.gameObject;
                var gameObjectPosition = WorldPositionToGridCell(targetGameObject.transform.position);
                var distance = Vector2.Distance(gameObjectPosition, position);
                var lifeComponent = targetGameObject.GetComponent<LifeComponent>();

                if (lifeComponent == null) continue; // skip spells

                Debug.Log($"Our layer {unit.layer}");
                Debug.Log($"Target layer {targetGameObject.layer}");
                Debug.Log($"CombatBehaviour from nearest enemy {unit.GetComponent<CombatBehaviour>()}");

                var canUnitsFight = // Units can fight if:
                    (targetGameObject.layer == gameObject.layer) || // same layer (ground & ground, flying & flying)
                    (targetGameObject.layer == Constants.FLYING_LAYER && unit.GetComponent<CombatBehaviour>().combatType == CombatType.Ranged) || // target is flying and we are ranged
                    (unit.layer == Constants.FLYING_LAYER); // we're flying

                var isTargetValid =
                    (minDistance == null || minDistance > distance) && component.team != team && !lifeComponent.isDead && canUnitsFight;

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

        public Vector2 GetTarget(GameObject unit, Vector2 position, int team)
        {
            Vector2? lanePosition = null;
            float firstLaneX = 2, secondLaneX = 11;

            var enemyPosition = GetNearestEnemy(unit, position, team)?.enemyCell;

            // if no enemies found and not on a lane, go back on a lane
            if (enemyPosition == null && position.x != firstLaneX && position.x != secondLaneX)
            {
                float xTarget, increment;

                Vector2 targetLanePosition = position;

                if (position.x <= mapSize.x / 2 && position.x >= firstLaneX) // if in the middle and near the first lane
                {
                    xTarget = firstLaneX;
                    increment = -1f;
                }
                else if (position.x < firstLaneX) // if on the left
                {
                    xTarget = firstLaneX;
                    increment = 1f;
                }
                else if (position.x > mapSize.x / 2 && position.x > secondLaneX) // if on the right
                {
                    xTarget = secondLaneX;
                    increment = -1f;
                }
                else // if in the middle and near the second lane 
                {
                    xTarget = secondLaneX;
                    increment = 1f;
                }

                while (targetLanePosition.x != xTarget)
                {
                    targetLanePosition.y += 1;
                    targetLanePosition.x += increment;
                }

                lanePosition = targetLanePosition;
            }

            Debug.Log(enemyPosition ?? lanePosition);

            // go to enemy position, or a lane, or to the end of the world
            return enemyPosition ?? lanePosition ?? new Vector2(position.x, 90);
        }

        public void OnUICardCollision(GameObject puppet)
        {
            DestroyPuppet();

            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 1;

            var cell = GetWorldPointedCell();

            lastPuppet = Instantiate(puppet, cell, Quaternion.identity, transform);
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
}