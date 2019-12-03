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
using Pandora.Engine;
using Pandora.Command;

namespace Pandora
{
    public class MapComponent : MonoBehaviour
    {
        int bottomMapSizeX = 16;
        public int bottomMapSizeY = 13;
        public int mapSizeX;
        public int mapSizeY;
        bool isLockedOnMiddle = false;
        public Vector2 worldMapSize;
        public bool debugHitboxes = false;
        Vector2 bottomMapSize;
        GameObject lastPuppet;
        public float localTime = 0;
        public Dictionary<int, HashSet<GridCell>> TowerPositionsDictionary = new Dictionary<int, HashSet<GridCell>>();
        public Dictionary<string, GameObject> Units = new Dictionary<string, GameObject> { };
        float firstLaneX = 2, secondLaneX = 13;

        List<GridCell> spawningCells;

        HashSet<GridCell> _riverPositions = new HashSet<GridCell>();

        HashSet<GridCell> riverPositions
        {
            get
            {
                if (_riverPositions.Count == 0)
                {
                    var riverY = 13;

                    for (var x = 0; x < bottomMapSize.x; x++)
                    {
                        if (x != firstLaneX && x != secondLaneX)
                        {
                            _riverPositions.Add(new GridCell(x, riverY));
                        }
                    }
                }

                return _riverPositions;
            }
        }

        public float cellHeight;
        public float cellWidth;
        uint frameStep = 40, remainingStep = 0, timeSinceLastStep; // milliseconds
        public PandoraEngine engine;
        public GameObject textObject;
        List<GameObject> debug = new List<GameObject> { };
        public Vector2 TopLeftTowerPosition, TopRightTowerPosition, TopMiddleTowerPosition,
            BottomLeftTowerPosition, BottomRightTowerPosition, BottomMiddleTowerPosition,
            TopLeftAggroOrigin, TopLeftAggroEnd,
            TopMiddleAggroOrigin, TopMiddleAggroEnd,
            TopRightAggroOrigin, TopRightAggroEnd,
            BottomLeftAggroOrigin, BottomLeftAggroEnd,
            BottomMiddleAggroOrigin, BottomMiddleAggroEnd,
            BottomRightAggroOrigin, BottomRightAggroEnd;

        static MapComponent _instance = null;

        public static MapComponent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.Find("Arena").GetComponent<MapComponent>();
                }

                return _instance;
            }
        }

        void OnGUI()
        {
            if (debugHitboxes) engine?.DrawDebugGUI();
        }

        public void Awake()
        {
            mapSizeX = bottomMapSizeX;
            mapSizeY = (bottomMapSizeY * 2) + 1;

            worldMapSize = new Vector2(mapSizeX * cellWidth, mapSizeY * cellHeight);

            bottomMapSize = new Vector2(bottomMapSizeX, bottomMapSizeY);

            timeSinceLastStep = frameStep; // initialize time since last step so we don't skip frames

            Screen.fullScreen = false;
            Screen.SetResolution(1080, 1920, false);

            Application.targetFrameRate = -1;
            //QualitySettings.vSyncCount = 0;

            var topArena = GameObject.Find("top_arena");
            var topArenaPosition = topArena.transform.position;
            var topArenaSize = topArena.GetComponent<SpriteRenderer>().bounds.size;

            Debug.Log($"Top arena position y {topArenaPosition.y}");
            Debug.Log($"Top arena position {(topArenaPosition.y + topArenaSize.y)}");

            cellWidth = topArenaSize.x / mapSizeX;
            cellHeight = ((topArenaPosition.y + topArenaSize.y) - transform.position.y) / mapSizeY;

            for (var x = 0; x < mapSizeX; x++)
            {
                var gridPosition = GridCellToWorldPosition(new GridCell(new Vector2Int(x, mapSizeY + 1)));

                SpawnText(gridPosition, x.ToString());
            }

            for (var y = 0; y < mapSizeY; y++)
            {
                var gridPosition = GridCellToWorldPosition(new GridCell(new Vector2Int(-1, y)));

                SpawnText(gridPosition, y.ToString());
            }

            engine = ScriptableObject.CreateInstance<PandoraEngine>();

            engine.Init(this);
        }

        void RefreshTowerHash(TeamComponent team)
        {
            var hashSet = new HashSet<GridCell>();

            foreach (var position in GetComponentsInChildren<TowerPositionComponent>())
            {
                // Count them as obstacles only for allied structures
                if (position.gameObject.GetComponent<TeamComponent>().team == team.team)
                    hashSet.UnionWith(position.GetTowerPositions());
            }

            TowerPositionsDictionary[team.team] = hashSet;
        }

        /**
         * Returns whether the position is uncrossable 
         */
        public bool IsObstacle(GridCell cell, bool isFlying, TeamComponent team)
        {
            var cellVector = cell.vector;

            var isOutOfBounds = (cellVector.x < 0 && cellVector.y < 0 && cellVector.x >= bottomMapSize.x && cellVector.y >= mapSizeY);

            if (!TowerPositionsDictionary.ContainsKey(team.team))
            {
                RefreshTowerHash(team);
            }

            var isTower = TowerPositionsDictionary[team.team].Contains(cell);

            var isRiver = riverPositions.Contains(cell);

            if (isFlying)
            {
                isRiver = false;
                isTower = false;
            }

            return isRiver || isOutOfBounds;
        }

        public GridCell WorldPositionToGridCell(Vector2 position)
        {

            Vector2 gridPosition =
                new Vector2(
                    position.x - transform.position.x,
                    position.y - transform.position.y
                );

            var cellPosition = new Vector2Int(
                Mathf.FloorToInt(gridPosition.x / cellWidth),
                Mathf.FloorToInt(gridPosition.y / cellHeight)
            );

            return new GridCell(cellPosition);
        }

        public Vector2 GridCellToWorldPosition(GridCell cell)
        {
            Vector2 worldPosition = new Vector2(
                transform.position.x + (cell.vector.x * cellWidth) + cellWidth / 2,
                transform.position.y + (cell.vector.y * cellHeight) + cellHeight / 2
            );

            return worldPosition;
        }

        public void Update()
        {
            StepMessage step = null;

            timeSinceLastStep += (uint)Mathf.FloorToInt(Time.deltaTime * 1000);

            if (NetworkControllerSingleton.instance.stepsQueue.TryDequeue(out step))
            {
                if (remainingStep > 0)
                {
                    Debug.LogWarning($"We're being too slow, we might possibly desync (we are {remainingStep}ms behind)");

                    engine.Process(remainingStep);
                }

                remainingStep = step.StepTimeMs;

                foreach (var command in step.Commands)
                {
                    if (command is SpawnMessage spawn)
                    {
                        Debug.Log($"Received {spawn} - spawning unit");

                        SpawnUnit(new UnitSpawn(spawn));
                    }

                    if (command is CommandMessage commandMessage)
                    {
                        var unit = Units[commandMessage.unitId];

                        unit?.GetComponent<CommandBehaviour>()?.InvokeCommand();
                    }
                }

                if (step.mana != null)
                {
                    ManaSingleton.UpdateMana((float)step.mana);
                }
            }

            if (!NetworkControllerSingleton.instance.matchStarted)
            {
                localTime += Time.deltaTime;

                if (localTime * 1000 > engine.TickTime)
                {
                    engine.Process(engine.TickTime);

                    localTime = 0;
                }
            }
            else
            {
                var processTime = Math.Min(frameStep, remainingStep);

                if (remainingStep != 0 && processTime != 0 && timeSinceLastStep >= frameStep)
                {
                    engine.Process(processTime);

                    remainingStep -= processTime;

                    timeSinceLastStep = 0;
                }
            }

            EnableAggroPoints();
        }

        public void DestroyPuppet()
        {
            if (lastPuppet != null)
                Destroy(lastPuppet);

            ResetAggroPoints();
        }

        public void SpawnCard(string cardName, int team, int requiredMana = 0)
        {
            ResetAggroPoints();

            var mapCell = GetPointedCell().vector;
            var id = System.Guid.NewGuid().ToString();
            var manaEnabled = GetComponent<LocalManaBehaviourScript>()?.Enabled ?? true;

            // TODO: Notify player somehow if they lack mana
            if (manaEnabled && ManaSingleton.manaValue < requiredMana)
            {
                return;
            }

            var message =
                new SpawnMessage
                {
                    unitName = cardName,
                    cellX = mapCell.x,
                    cellY = mapCell.y,
                    team = TeamComponent.assignedTeam,
                    unitId = id,
                    manaUsed = requiredMana
                };

            NetworkControllerSingleton.instance.EnqueueMessage(message);

            if (!NetworkControllerSingleton.instance.matchStarted)
            {
                message.team = team;
                message.timestamp = DateTime.Now;

                SpawnUnit(new UnitSpawn(message));

                ManaSingleton.UpdateMana(ManaSingleton.manaValue - requiredMana);
                ManaSingleton.manaUnit -= requiredMana;
            }
        }

        public GameObject LoadCard(string unitName) => Resources.Load($"Units/{unitName}") as GameObject;

        /// <summary>Spawns a unit</summary>
        public void SpawnUnit(UnitSpawn spawn)
        {
            Debug.Log($"Spawning {spawn.UnitName} in {spawn.CellX}, {spawn.CellY} Team {spawn.Team}");

            var card = LoadCard(spawn.UnitName);

            if (spawn.Team == TeamComponent.topTeam)
            { // flip Y if top team
                spawn.CellY = mapSizeY - spawn.CellY;
            }

            var unitGridCell = new GridCell(spawn.CellX, spawn.CellY);
            var cardPosition = GridCellToWorldPosition(unitGridCell);
            var cardObject = Instantiate(card, cardPosition, Quaternion.identity, transform);

            cardObject.name += $"-{spawn.Id}";

            var spawner = cardObject.GetComponent<Spawner>();

            if (spawner != null)
            {
                spawner.Spawn(this, spawn);
            }
            else
            {
                InitializeComponents(cardObject, unitGridCell, spawn.Team, spawn.Id, spawn.Timestamp);
            }
        }

        /// <summary>Initializes unit components, usually called on spawn</summary>
        public void InitializeComponents(GameObject unit, GridCell cell, int team, string id, DateTime? timestamp)
        {
            unit.GetComponent<TeamComponent>().team = team;

            var movement = unit.GetComponent<MovementComponent>();
            var movementBehaviour = unit.GetComponent<MovementBehaviour>();
            var projectileSpell = unit.GetComponent<ProjectileSpellBehaviour>();

            if (movementBehaviour != null) movementBehaviour.map = this;

            if (projectileSpell != null)
            {
                var towerPosition = GetTowerPositionComponent(TowerPosition.BottomMiddle);

                projectileSpell.map = this;
            }

            var engineEntity = engine.AddEntity(unit, movementBehaviour?.Speed ?? projectileSpell.Speed, projectileSpell?.StartCell ?? cell, projectileSpell == null, timestamp);

            if (movement != null) engineEntity.CollisionCallback = movement;

            if (projectileSpell != null)
            {
                engineEntity.SetTarget(cell);

                projectileSpell.Target = cell;
            }

            unit.GetComponent<EngineComponent>().Entity = engineEntity;
            unit.AddComponent<UnitIdComponent>().Id = id;

            Units.Add(id, unit);
        }

        public Enemy GetEnemy(GameObject unit, GridCell position, TeamComponent team)
        {
            float? minDistance = null;
            GameObject inRangeEnemy = null;
            var cellVector = position.vector;

            var combatBehaviour = unit.GetComponent<CombatBehaviour>();

            foreach (TeamComponent component in GetComponentsInChildren<TeamComponent>())
            {
                var targetGameObject = component.gameObject;
                var gameObjectPosition = GetCell(targetGameObject);
                var engineEntity = GetEngineEntity(unit);
                var targetEngineEntity = GetEngineEntity(targetGameObject);
                var distance = Vector2.Distance(gameObjectPosition.vector, position.vector);
                var lifeComponent = targetGameObject.GetComponent<LifeComponent>();

                if (lifeComponent == null || lifeComponent.IsDead) continue; // skip spells

                var canUnitsFight = // Units can fight if:
                    (targetGameObject.layer == unit.layer) || // same layer (ground & ground, flying & flying, etc.)
                    (unit.layer == Constants.SWIMMING_LAYER) ||
                    (targetGameObject.layer == Constants.FLYING_LAYER && unit.GetComponent<CombatBehaviour>().combatType == CombatType.Ranged) || // target is flying and we are ranged
                    (unit.layer == Constants.FLYING_LAYER); // we're flying

                var isInRange = combatBehaviour.IsInAggroRange(new Enemy(targetGameObject));

                var isTargetValid =
                    (minDistance == null || minDistance > distance) && isInRange && component.IsOpponent() != unit.GetComponent<TeamComponent>().IsOpponent() && !lifeComponent.IsDead && canUnitsFight && (
                        (isLockedOnMiddle && targetEngineEntity.IsStructure) ? targetGameObject.GetComponent<TowerPositionComponent>().EngineTowerPosition.IsMiddle() : true
                    );

                if (isTargetValid)
                {
                    minDistance = distance;
                    inRangeEnemy = targetGameObject;
                }
            }

            if (inRangeEnemy != null)
            {
                isLockedOnMiddle = false; // reset lock if engaging an enemy

                return new Enemy(inRangeEnemy);
            }

            TowerPosition targetTowerPosition;

            if (cellVector.x < bottomMapSizeX / 2)
            {
                targetTowerPosition = team.IsTop() ? TowerPosition.BottomLeft : TowerPosition.TopLeft;
            }
            else
            {
                targetTowerPosition = team.IsTop() ? TowerPosition.BottomRight : TowerPosition.TopRight;
            }

            TowerPositionComponent towerPositionComponent = null, middleTowerPositionComponent = null;

            foreach (var component in GetComponentsInChildren<TowerPositionComponent>())
            {
                var towerCombatBehaviour = component.gameObject.GetComponent<TowerCombatBehaviour>();
                var towerTeamComponent = component.gameObject.GetComponent<TowerTeamComponent>();

                if (towerCombatBehaviour.isMiddle && towerTeamComponent.engineTeam != team.team)
                {
                    middleTowerPositionComponent = component;
                }

                if (component.EngineTowerPosition == targetTowerPosition && !component.gameObject.GetComponent<LifeComponent>().IsDead)
                {
                    towerPositionComponent = component;
                }
            }

            if (towerPositionComponent == null)
            {
                isLockedOnMiddle = true;
            }

            var towerObject = towerPositionComponent?.gameObject ?? middleTowerPositionComponent?.gameObject;

            if (isLockedOnMiddle)
            {
                towerObject = middleTowerPositionComponent.gameObject;
            }

            return new Enemy(towerObject);
        }

        // Finds units in a gridcell-space rectangle
        public List<GameObject> GetUnitsInRect(Vector2 origin, int widthCells, int heightCells)
        {
            var units = new List<GameObject>(30);
            var lowerRange = Math.Min(origin.y, origin.y + heightCells);
            var higherRange = Math.Max(origin.y, origin.y + heightCells);

            foreach (var component in GetComponentsInChildren<UnitBehaviour>())
            {
                var cellVector = GetCell(component.gameObject).vector;

                var isDead = component.gameObject.GetComponent<LifeComponent>()?.IsDead ?? true;

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

        public void ResetAggroPoints()
        {
            var combatBehaviours =
                from component in GetComponentsInChildren<CombatBehaviour>()
                where
                    !(component is TowerCombatBehaviour) &&
                    (component as MonoBehaviour).gameObject.GetComponent<TeamComponent>().team != TeamComponent.assignedTeam
                select component;

            foreach (var combatBehaviour in combatBehaviours)
            {
                if (combatBehaviour is TowerCombatBehaviour) continue;

                var behaviour = combatBehaviour as MonoBehaviour;

                behaviour.gameObject.GetComponentInChildren<AggroExclamPointBehaviour>().gameObject.GetComponent<Image>().enabled = false;
            }

            spawningCells = null;
        }

        public void EnableAggroPoints()
        {
            if (spawningCells == null) return;

            var combatBehaviours =
                            from component in GetComponentsInChildren<CombatBehaviour>()
                            where
                                !(component is TowerCombatBehaviour) &&
                                (component as MonoBehaviour).gameObject.GetComponent<TeamComponent>().team != TeamComponent.assignedTeam
                            select component;

            foreach (var combatBehaviour in combatBehaviours)
            {
                var isInRange = false;

                foreach (var unitCell in spawningCells)
                {
                    if (combatBehaviour.IsInAggroRange(unitCell))
                    {
                        isInRange = true;

                        break;
                    }
                }

                if (combatBehaviour is TowerCombatBehaviour || !isInRange) continue;

                var behaviour = combatBehaviour as MonoBehaviour;

                behaviour.gameObject.GetComponentInChildren<AggroExclamPointBehaviour>().gameObject.GetComponent<Image>().enabled = true;
            }
        }

        /// <summary></summary>
        public bool OnUICardCollision(GameObject puppet, bool isAquatic, bool isGlobal, GameObject unit)
        {
            DestroyPuppet();

            var cell = GetPointedCell();

            spawningCells =
                (from position in unit.GetComponent<Spawner>()?.CellPositions ?? new Vector2Int[] { new Vector2Int(0, 0) }
                 select new GridCell(cell.vector + position)).ToList();

            if ((!isGlobal && cell.vector.y > 13) || (isAquatic && !riverPositions.Contains(cell))) return false;

            lastPuppet = Instantiate(puppet, GridCellToWorldPosition(cell), Quaternion.identity, transform);

            lastPuppet.transform.SetAsFirstSibling();

            EnableAggroPoints();

            return true;
        }

        private HashSet<GridCell> GetTowerPositions(GridCell towerCell, float towerSize = 3f)
        {
            var set = new HashSet<GridCell>();

            for (var x = 0; x < towerSize; x++)
            {
                for (var y = 0; y < towerSize; y++)
                {
                    set.Add(new GridCell(towerCell.vector.x + x, towerCell.vector.y + y));
                }
            }

            Debug.Log("Tower positions " + string.Join(",", set));

            return set;
        }

        private GridCell GetPointedCell()
        {
            Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 mousePosition =
                new Vector2(
                    worldMouse.x - transform.position.x,
                    worldMouse.y - transform.position.y
                );

            var cellPosition = new Vector2Int(
                Mathf.FloorToInt(mousePosition.x / cellWidth),
                Mathf.FloorToInt(mousePosition.y / cellHeight)
            );

            return new GridCell(cellPosition);
        }

        private Vector2 GetWorldPointedCell()
        {
            var cell = GetPointedCell().vector;

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

        private EngineEntity GetEngineEntity(GameObject gameObject)
        {
            return gameObject.GetComponent<EngineComponent>().Entity;
        }

        public GridCell GetCell(GameObject gameObject)
        {
            return GetEngineEntity(gameObject).GetCurrentCell();
        }

        public GridCell Flip(GridCell cell)
        {
            var flipped = new GridCell(cell.vector.x, cell.vector.y);

            flipped.vector.y = mapSizeY - flipped.vector.y;

            return flipped;
        }

        public Vector2? GetTowerPosition(TowerPosition position)
        {
            return
                (position == TowerPosition.TopLeft) ? TopLeftTowerPosition :
                (position == TowerPosition.TopRight) ? TopRightTowerPosition :
                (position == TowerPosition.TopMiddle) ? TopMiddleTowerPosition :
                (position == TowerPosition.BottomLeft) ? BottomLeftTowerPosition :
                (position == TowerPosition.BottomRight) ? BottomRightTowerPosition :
                (position == TowerPosition.BottomMiddle) ? BottomMiddleTowerPosition : (Vector2?)null;
        }


        public Vector2? GetTowerAggroBoxOrigin(TowerPosition position)
        {
            return
                (position == TowerPosition.TopLeft) ? TopLeftAggroOrigin :
                (position == TowerPosition.TopRight) ? TopRightAggroOrigin :
                (position == TowerPosition.TopMiddle) ? TopMiddleAggroOrigin :
                (position == TowerPosition.BottomLeft) ? BottomLeftAggroOrigin :
                (position == TowerPosition.BottomRight) ? BottomRightAggroOrigin :
                (position == TowerPosition.BottomMiddle) ? BottomMiddleAggroOrigin : (Vector2?)null;
        }


        public Vector2? GetTowerAggroBoxEnd(TowerPosition position)
        {
            return
                (position == TowerPosition.TopLeft) ? TopLeftAggroEnd :
                (position == TowerPosition.TopRight) ? TopRightAggroEnd :
                (position == TowerPosition.TopMiddle) ? TopMiddleAggroEnd :
                (position == TowerPosition.BottomLeft) ? BottomLeftAggroEnd :
                (position == TowerPosition.BottomRight) ? BottomRightAggroEnd :
                (position == TowerPosition.BottomMiddle) ? BottomMiddleAggroEnd : (Vector2?)null;
        }

        public TowerPositionComponent GetTowerPositionComponent(TowerPosition worldPosition)
        {
            TowerPositionComponent position = null;

            foreach (var component in GetComponentsInChildren<TowerPositionComponent>())
            {
                if (component.WorldTowerPosition == worldPosition)
                {
                    position = component;
                }
            }

            return position;
        }
    }
}