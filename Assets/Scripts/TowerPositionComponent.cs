using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Combat;
using Pandora.Engine;
using Pandora.Network;

namespace Pandora
{

    /// <summary>
    /// This represents both the engine entity position and actually rendered position,
    /// which differs when the player is the top player (everything is flipped when rendered)
    /// </summary>
    public class TowerPositionComponent : MonoBehaviour
    {
        // this should not be used anywhere. It's here just to give a way to set tower positions in the unity editor
        public Vector2Int Position {
            get {
                return Vector2Int.FloorToInt(MapComponent.Instance.GetTowerPosition(EngineTowerPosition).Value);
            }
        }

        public GridCell TowerCell {
            get {
                return new GridCell(Position);
            }
        }

        public TowerPosition WorldTowerPosition = TowerPosition.TopLeft;
        public TowerPosition EngineTowerPosition
        {
            get
            {
                return
                    (TeamComponent.assignedTeam == TeamComponent.topTeam) ?
                        WorldTowerPosition.Flip() :
                        WorldTowerPosition;
            }
        }

        public EngineEntity TowerEntity;

        TowerTeamComponent teamComponent;
        MapComponent mapComponent;
        bool shouldRefresh = false;

        public List<GridCell> GetTowerPositions() {
            var positions = new List<GridCell> {};

            if (GetComponent<LifeComponent>().IsDead) {
                return positions;
            }

            var xLength = (EngineTowerPosition.IsMiddle()) ? 4 : 3;

            for (var x = -2; x < xLength; x++) {
                for (var y = -2; y < 3; y++) {

                    positions.Add(new GridCell(Position.x + x, Position.y + y));
                }
            }

            return positions;
        }

        public GridCell GetTowerCenter() => TowerCell;

        void Start()
        {
            teamComponent = GetComponent<TowerTeamComponent>();
            mapComponent = MapComponent.Instance;

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(RefreshTower);

            SpawnTower();
        }

        void Update() {
            if (shouldRefresh) {
                RemoveTower();
                SpawnTower();

                shouldRefresh = false;
            }
        }

        public void RefreshTower() {
            shouldRefresh = true;
        }

        public void SpawnTower()
        {
            var center = MapComponent.Instance.engine.GridCellToPhysics(GetTowerCenter());

            center.y += MapComponent.Instance.engine.UnitsPerCell / 2;

            if (!EngineTowerPosition.IsMiddle()) {
                center.x += MapComponent.Instance.engine.UnitsPerCell / 2;
            }

            var epoch = System.DateTime.MinValue;

            TowerEntity = MapComponent.Instance.engine.AddEntity(gameObject, 0, center, true, epoch.AddSeconds((int) EngineTowerPosition));
            TowerEntity.IsRigid = !EngineTowerPosition.IsMiddle();
            TowerEntity.IsStructure = true;

            GetComponent<EngineComponent>().Entity = TowerEntity;
        }

        public void RemoveTower() {
            MapComponent.Instance.engine.RemoveEntity(TowerEntity);
        }
    }
}
