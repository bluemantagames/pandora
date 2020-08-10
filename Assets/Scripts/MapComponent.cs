using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Pandora.Movement;
using Pandora.Spell;
using Pandora.Combat;
using Pandora.Deck;
using Pandora.Resource.Mana;
using Pandora.Resource.Gold.Rewards;
using Pandora.Network;
using Pandora.Network.Messages;
using Pandora.Engine;
using System.Threading;
using Pandora.Command;
using UnityEngine.EventSystems;

namespace Pandora
{
    public class MapComponent : MonoBehaviour, IPointerDownHandler
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
        CustomSampler aggroSampler, targetValidSampler;

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
        public Boolean IsLive = false;

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
            IsLive = ReplayControllerSingleton.instance.IsActive == true;

            aggroSampler = CustomSampler.Create("Check aggro");
            targetValidSampler = CustomSampler.Create("Check target valid");

            mapSizeX = bottomMapSizeX;
            mapSizeY = (bottomMapSizeY * 2) + 1;

            worldMapSize = new Vector2(mapSizeX * cellWidth, mapSizeY * cellHeight);

            bottomMapSize = new Vector2(bottomMapSizeX, bottomMapSizeY);

            timeSinceLastStep = frameStep; // initialize time since last step so we don't skip frames

            Screen.fullScreen = false;
            Screen.SetResolution(720, 1280, false);

            Application.targetFrameRate = 30;

            var topArena = GameObject.Find("top_arena");
            var topArenaPosition = topArena.transform.position;
            var topArenaSize = topArena.GetComponent<SpriteRenderer>().bounds.size;

            Logger.Debug($"Top arena position y {topArenaPosition.y}");
            Logger.Debug($"Top arena position {(topArenaPosition.y + topArenaSize.y)}");

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

            int availableThreads, complThreads;

            ThreadPool.GetMinThreads(out availableThreads, out complThreads);

            Logger.Debug($"Threads: {availableThreads}");

            Logger.Debug($"Map x size: {cellWidth * mapSizeX}");
            Logger.Debug($"Map y size: {cellHeight * mapSizeY}");

            engine.Init(this);
        }

        void RefreshTowerHash(TeamComponent team)
        {
            var hashSet = new HashSet<GridCell>();

            foreach (var position in GetComponentsInChildren<TowerPositionComponent>())
            {
                // Count them as obstacles only for allied structures
                if (position.gameObject.GetComponent<TeamComponent>().Team == team.Team)
                    hashSet.UnionWith(position.GetTowerPositions());
            }

            TowerPositionsDictionary[team.Team] = hashSet;
        }

        /**
         * Returns whether the position is uncrossable 
         */
        public bool IsObstacle(GridCell cell, bool isFlying, TeamComponent team)
        {
            var cellVector = cell.vector;

            var isOutOfBounds = (cellVector.x < 0 && cellVector.y < 0 && cellVector.x >= bottomMapSize.x && cellVector.y >= mapSizeY);

            if (!TowerPositionsDictionary.ContainsKey(team.Team))
            {
                RefreshTowerHash(team);
            }

            var isTower = TowerPositionsDictionary[team.Team].Contains(cell);

            var isRiver = riverPositions.Contains(cell);

            if (isFlying)
            {
                isRiver = false;
                isTower = false;
            }

            return isRiver || isTower || isOutOfBounds;
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

            var queue = NetworkControllerSingleton.instance.stepsQueue;

            var matchStarted = NetworkControllerSingleton.instance.matchStarted;

            if (queue.TryDequeue(out step))
            {
                if (remainingStep > 0)
                {
                    Logger.DebugWarning($"We're being too slow, we might possibly desync (we are {remainingStep}ms behind)");

                    engine.Process(remainingStep);
                }

                remainingStep = step.StepTimeMs;

                foreach (var command in step.Commands)
                {
                    if (command is SpawnMessage spawn)
                    {
                        Logger.Debug($"Received {spawn} - spawning unit");

                        SpawnUnit(new UnitSpawn(spawn));
                    }

                    if (command is CommandMessage commandMessage)
                    {
                        var unit = Units[commandMessage.unitId];

                        Debug.Log($"Commanded {commandMessage.unitId}");

                        unit.GetComponent<CommandBehaviour>().InvokeCommand();
                    }

                    if (command is GoldRewardMessage goldRewardMessage) {
                        var goldReward = RewardsRepository.Instance.GetReward(goldRewardMessage.rewardId);

                        goldReward.RewardApply(this, goldRewardMessage.team, goldRewardMessage.playerId);
                    }
                }

                if (step.mana != null)
                {
                    ManaSingleton.UpdateMana((float)step.mana);
                }
            }

            if (!matchStarted)
            {
                localTime += Time.deltaTime;

                if (localTime * 1000 >= engine.TickTime)
                {
                    engine.Process(engine.TickTime);

                    localTime = 0;
                }
            }
            else
            {
                //var processTime = Math.Min(frameStep, remainingStep);
                var processTime = frameStep;

                //if (remainingStep != 0 && processTime != 0 && timeSinceLastStep >= frameStep)
                if (remainingStep > 0 && processTime != 0)
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

        public void ApplyGoldReward(string rewardId, int goldCost) {
            var maybePlayerId = NetworkControllerSingleton.instance.PlayerId;
            var playerId = maybePlayerId.HasValue ? maybePlayerId.Value : 0;

            var message = new GoldRewardMessage {
                rewardId = rewardId,
                team = TeamComponent.assignedTeam,
                elapsedMs = (int) engine.TotalElapsed,
                goldSpent = goldCost,
                playerId = playerId
            };

            NetworkControllerSingleton.instance.EnqueueMessage(message);

            if (!NetworkControllerSingleton.instance.matchStarted)
            {
                var goldReward = RewardsRepository.Instance.GetReward(rewardId);

                goldReward.RewardApply(this, message.team, message.playerId);
            }
        }

        public bool SpawnCard(string cardName, int team, GridCell cell, int requiredMana = 0)
        {
            if (IsLive) return false;

            ResetAggroPoints();

            var spawnPosition = cell.vector;
            var id = System.Guid.NewGuid().ToString();
            var manaEnabled = GetComponent<LocalManaBehaviourScript>()?.Enabled ?? true;
            var elapsedMs = engine.TotalElapsed;

            // TODO: Notify player somehow if they lack mana
            if (manaEnabled && ManaSingleton.manaValue < requiredMana)
            {
                return false;
            }

            var message =
                new SpawnMessage
                {
                    unitName = cardName,
                    cellX = spawnPosition.x,
                    cellY = spawnPosition.y,
                    team = TeamComponent.assignedTeam,
                    unitId = id,
                    manaUsed = requiredMana,
                    elapsedMs = elapsedMs
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

            return true;
        }

        public GameObject LoadCard(string unitName) => Resources.Load($"Units/{unitName}") as GameObject;

        /// <summary>Spawns a unit</summary>
        public void SpawnUnit(UnitSpawn spawn)
        {
            Logger.Debug($"Spawning {spawn.UnitName} in {spawn.CellX}, {spawn.CellY} Team {spawn.Team}");

            var card = LoadCard(spawn.UnitName);
            var manaAnimationGridPosition = new Vector2Int(spawn.CellX, spawn.CellY);

            if (spawn.Team == TeamComponent.topTeam)
            {
                // flip Y if top team
                spawn.CellY = (mapSizeY - 1) - spawn.CellY;
            }

            // This exists because the ManaUsedAlert game object
            // needs the real y-position of the enemy team
            if (spawn.Team != TeamComponent.assignedTeam)
            {
                // flip Y if enemy team
                manaAnimationGridPosition.y = (mapSizeY - 1) - manaAnimationGridPosition.y;
            }

            var unitGridCell = new GridCell(spawn.CellX, spawn.CellY);
            var manaAnimationGridCell = new GridCell(manaAnimationGridPosition.x, manaAnimationGridPosition.y);
            var cardPosition = GridCellToWorldPosition(unitGridCell);
            var manaAnimationPosition = GridCellToWorldPosition(manaAnimationGridCell);

            var unitObject = Instantiate(card, cardPosition, Quaternion.identity, transform);

            unitObject.name += $"-{spawn.Id}";

            var spawner = unitObject.GetComponent<Spawner>();

            if (spawner != null)
            {
                spawner.Spawn(this, spawn);
            }
            else
            {
                InitializeComponents(unitObject, unitGridCell, spawn);
            }

            // This is tricky, we need the stuff below because the spawner and the units are actually
            // created in different positions. The spawner element (NOT THE UNITS) is created 
            // non-mirrored, while the single unit is created directly mirrored in the field.
            // This leads to inconcistencies in the ManaUsedAlert component position in the Team 2
            // (since the Team 2 field is mirrored).
            if (spawner == null && TeamComponent.assignedTeam == TeamComponent.topTeam)
            {
                ShowManaUsedAlert(unitObject, spawn.ManaUsed, cardPosition);
            }
            else
            {
                ShowManaUsedAlert(unitObject, spawn.ManaUsed, manaAnimationPosition);
            }

            if (spawn.Team == TeamComponent.assignedTeam && unitObject.GetComponent<ProjectileSpellBehaviour>() == null)
            {
                CommandViewportBehaviour.Instance.AddCommand(spawn.UnitName, spawn.Id);
            }
        }

        /// <summary>Initializes unit components, usually called on spawn</summary>
        public void InitializeComponents(GameObject unit, GridCell cell, UnitSpawn unitSpawn)
        {
            unit.GetComponent<TeamComponent>().Team = unitSpawn.Team;

            var movement = unit.GetComponent<MovementComponent>();
            var movementBehaviour = unit.GetComponent<MovementBehaviour>();
            var projectileSpell = unit.GetComponent<ProjectileSpellBehaviour>();

            if (movementBehaviour != null) movementBehaviour.map = this;

            if (projectileSpell != null)
            {
                projectileSpell.StartCell =
                    new GridCell(
                        ((unitSpawn.Team == TeamComponent.assignedTeam) ?
                            GetTowerPositionComponent(TowerPosition.BottomMiddle) : GetTowerPositionComponent(TowerPosition.TopMiddle)).Position
                    );

                projectileSpell.map = this;
            }

            var engineEntity = engine.AddEntity(
                unit, 
                movementBehaviour?.Speed ?? projectileSpell.Speed,
                projectileSpell?.StartCell ?? cell,
                projectileSpell == null,
                unitSpawn.Timestamp
            );

            if (movement != null) engineEntity.CollisionCallback = movement;

            if (projectileSpell != null)
            {
                engineEntity.SetTarget(cell);

                projectileSpell.Target = cell;
            }

            unit.GetComponent<EngineComponent>().Entity = engineEntity;

            var idComponent = unit.AddComponent<UnitIdComponent>();

            idComponent.Id = unitSpawn.Id;
            idComponent.UnitName = unitSpawn.UnitName;

            unit.GetComponentInChildren<HealthbarBehaviour>()?.RefreshColor();

            Units.Add(unitSpawn.Id, unit);

            var manaCostComponent = unit.AddComponent<ManaCostComponent>();

            manaCostComponent.ManaCost = unitSpawn.ManaUsed;
        }

        public void ShowManaUsedAlert(GameObject unit, int manaUsed, Vector2 position)
        {
            var manaUsedObject = unit.GetComponentInChildren<ManaUsedAlertBehaviour>()?.gameObject;

            if (manaUsedObject == null) return;

            var manaUsedText = manaUsedObject.GetComponentInChildren<Text>();

            manaUsedObject.transform.position = position;

            if (manaUsedText != null)
            {
                manaUsedText.text = $"-{manaUsed}";
            }
        }

        public bool CanFight(GameObject unit1, GameObject unit2)
        {
            return // Units can fight if:
                (unit2.layer == unit1.layer) || // same layer (ground & ground, flying & flying, etc.)
                (unit1.layer == Constants.SWIMMING_LAYER) ||
                ((unit2.layer == Constants.FLYING_LAYER || unit2.layer == Constants.SWIMMING_LAYER) && unit1.GetComponent<CombatBehaviour>().combatType == CombatType.Ranged) || // target is flying/swimming and we are ranged
                (unit1.layer == Constants.FLYING_LAYER); // we're flying
        }

        public Enemy GetEnemy(GameObject unit, GridCell position, TeamComponent team)
        {
            int? minDistance = null;
            GameObject inRangeEnemy = null;
            var cellVector = position.vector;

            var combatBehaviour = unit.GetComponent<CombatBehaviour>();

            var engineEntity = GetEngineEntity(unit);
            var unitTeam = unit.GetComponent<TeamComponent>();

            foreach (var entity in engine.Entities)
            {
                var component = entity.GameObject.GetComponent<TeamComponent>();

                if (component == null) continue;

                var targetGameObject = component.gameObject;
                var distance = engine.SquaredDistance(engineEntity.Position, entity.Position);

                if (minDistance != null && minDistance < distance) continue;
                if (component.Team == unitTeam.Team) continue;

                var lifeComponent = targetGameObject.GetComponent<LifeComponent>();

                if (lifeComponent == null || lifeComponent.IsDead) continue; // skip spells

                var canUnitsFight = CanFight(unit, targetGameObject);

                aggroSampler.Begin();
                var isInRange = combatBehaviour.IsInAggroRange(new Enemy(targetGameObject));
                aggroSampler.End();

                targetValidSampler.Begin();
                var isTargetValid =
                    (minDistance == null || minDistance > distance) && isInRange && component.IsOpponent() != unit.GetComponent<TeamComponent>().IsOpponent() && !lifeComponent.IsDead && canUnitsFight && (
                        (isLockedOnMiddle && entity.IsStructure) ? targetGameObject.GetComponent<TowerPositionComponent>().EngineTowerPosition.IsMiddle() : true
                    );
                targetValidSampler.End();

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

                if (towerCombatBehaviour.isMiddle && towerTeamComponent.EngineTeam != team.Team)
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

            foreach (var entity in engine.Entities)
            {
                var component = entity.GameObject.GetComponent<UnitBehaviour>();

                if (component == null) continue;

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
                    (component as MonoBehaviour).gameObject.GetComponent<TeamComponent>().Team != TeamComponent.assignedTeam
                select component;

            foreach (var combatBehaviour in combatBehaviours)
            {
                if (combatBehaviour is TowerCombatBehaviour) continue;

                var behaviour = combatBehaviour as MonoBehaviour;

                var image = behaviour?.gameObject?.GetComponentInChildren<AggroExclamPointBehaviour>()?.gameObject?.GetComponent<Image>();

                if (image != null)
                {
                    image.enabled = false;
                }
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
                    (component as MonoBehaviour).gameObject.GetComponent<TeamComponent>().Team != TeamComponent.assignedTeam
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

                var image = behaviour?.gameObject?.GetComponentInChildren<AggroExclamPointBehaviour>()?.gameObject?.GetComponent<Image>();

                if (image != null)
                {
                    image.enabled = true;
                }
            }
        }

        /// <summary></summary>
        public bool OnUICardCollision(GameObject puppet, bool isAquatic, bool isGlobal, GameObject unit, GridCell cell)
        {
            DestroyPuppet();

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

            Logger.Debug("Tower positions " + string.Join(",", set));

            return set;
        }

        public GridCell GetPointedCell()
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

            Logger.Debug($"Pointed cell {cell}");

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

        public void OnPointerDown(PointerEventData eventData)
        {
            var selectedCards = HandBehaviour.Instance.SelectedCards;

            if (selectedCards.Count > 0)
            {
                Logger.Debug($"Dragging {selectedCards}");

                selectedCards[0].CardObject.GetComponent<CardBehaviour>().Dragging = true;
            }
        }
    }
}