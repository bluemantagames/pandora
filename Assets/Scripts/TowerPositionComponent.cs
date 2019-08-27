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
        public Vector2 Position {
            get {
                return MapComponent.Instance.GetTowerPosition(EngineTowerPosition).Value;
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

        public GridCell GetMapTarget()
        {
            if (EngineTowerPosition == TowerPosition.TopLeft || EngineTowerPosition == TowerPosition.TopRight)
            {
                return new GridCell(Position.x + 1, Position.y - 1);
            }
            else if (EngineTowerPosition == TowerPosition.BottomLeft || EngineTowerPosition == TowerPosition.BottomRight)
            {
                return new GridCell(Position.x + 1, Position.y + 3);
            }
            else if (EngineTowerPosition == TowerPosition.BottomMiddle)
            {
                return new GridCell(8, 3);
            }
            else if (EngineTowerPosition == TowerPosition.TopMiddle)
            {
                return new GridCell(8, 23);
            }
            else
            {
                return TowerCell;
            }
        }

        public GridCell GetTowerCenter()
        {
            if (EngineTowerPosition == TowerPosition.TopLeft || EngineTowerPosition == TowerPosition.TopRight)
            {
                return new GridCell(Position.x + 1, Position.y + 1);
            }
            else if (EngineTowerPosition == TowerPosition.BottomLeft || EngineTowerPosition == TowerPosition.BottomRight)
            {
                return new GridCell(Position.x + 1, Position.y + 1);
            }
            else if (EngineTowerPosition == TowerPosition.BottomMiddle)
            {
                return new GridCell(Position.x + 2, Position.y + 2);
            }
            else if (EngineTowerPosition == TowerPosition.TopMiddle)
            {
                return new GridCell(Position.x + 2, Position.y + 2);
            }
            else
            {
                return TowerCell;
            }
        }

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
            TowerEntity = MapComponent.Instance.engine.AddEntity(gameObject, 0f, GetTowerCenter(), true, null);
            TowerEntity.IsStructure = true;

            GetComponent<EngineComponent>().Entity = TowerEntity;
        }

        public void RemoveTower() {
            MapComponent.Instance.engine.RemoveEntity(TowerEntity);
        }
    }
}
