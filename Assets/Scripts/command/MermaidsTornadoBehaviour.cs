using Pandora.Engine;
using Pandora.Combat.Effects;
using UnityEngine;
using System.Collections.Generic;

namespace Pandora.Command {
    public class MermaidsTornadoBehaviour : MonoBehaviour, EngineBehaviour
    {
        public string ComponentName => "MermaidsTornadoBehaviour";

        public GameObject TornadoEffectObject;
        public int EngineUnitsRadius, EngineUnitsSpeed = 100, ChooseTargetTicksNum = 10, DurationMs = 2000;
        int ticksElapsed = 0;
        uint totalTimeElapsed = 0;
        List<EngineEntity> targets = new List<EngineEntity> {};
        

        // TODO: Remove this, make the mermaids add it to the engine
        void Start() {
            var engine = MapComponent.Instance.engine;
            var entityPosition = engine.GridCellToPhysics(MapComponent.Instance.WorldPositionToGridCell(transform.position));

            Debug.Log($"Spawning tornado in {entityPosition}");
        
            var entity = engine.AddEntity(gameObject, 0, entityPosition, false, null);

            var engineComponent = gameObject.AddComponent<EngineComponent>();

            engineComponent.Entity = entity;


            var teamComponent = gameObject.GetComponent<TeamComponent>();
            
            teamComponent.team = 1;
        }

        public void TickUpdate(uint timeLapsed)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var teamComponent = GetComponent<TeamComponent>();

            ticksElapsed++;

            totalTimeElapsed += timeLapsed;

            var entity = engineComponent.Entity;
            var engine = engineComponent.Engine;

            transform.position = engine.PhysicsToMap(entity.Position);

            foreach (var target in engine.FindInRadius(entity.Position, EngineUnitsRadius, false)) {
                if (target == entity || !target.IsRigid) continue;
                
                TornadoEffectObject.GetComponent<MermaidsTornadoEffect>().Apply(gameObject, target.GameObject);

                targets.Add(target);
            }

            if (ticksElapsed % ChooseTargetTicksNum == 0) 
            {
                EngineEntity middleTower = null;

                foreach (var tower in MapComponent.Instance.gameObject.GetComponentsInChildren<TowerPositionComponent>()) {
                    Debug.Log($"Inspecting {tower.EngineTowerPosition.IsMiddle()} {tower.GetComponent<TowerTeamComponent>().engineTeam} {teamComponent.team}");

                    if (tower.EngineTowerPosition.IsMiddle() && tower.GetComponent<TowerTeamComponent>().engineTeam == teamComponent.team) {
                        middleTower = tower.GetComponent<EngineComponent>().Entity;

                        break;
                    }
                }

                if (middleTower == null) {
                    Debug.LogWarning("Could not find middle tower");

                    return;
                }

                var target = engine.FindClosest(middleTower.Position, foundEntity => 
                    !foundEntity.IsStructure &&
                    !foundEntity.IsMapObstacle &&
                    foundEntity.GameObject.GetComponent<TeamComponent>().team != teamComponent.team
                );

                if (target != null) {
                    entity.SetTarget(target);
                    entity.SetSpeed(EngineUnitsSpeed);
                }
            }
        }
    }
}