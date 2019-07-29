using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRclone;

namespace CRclone.Combat
{
    public class TowerCombatBehaviour : MonoBehaviour
    {
        public MapListener map;
        TeamComponent teamComponent;
        Vector2 aggroBoxOrigin;

        public int aggroBoxHeight = 6;
        public int aggroBoxWidth = 3;

        void Awake()
        {
            aggroBoxOrigin = GetComponent<TowerPositionComponent>().position;

            teamComponent = GetComponent<TeamComponent>();

            if (teamComponent.team == TeamComponent.assignedTeam)
            {
                aggroBoxOrigin.y += 3;
            }
            else
            {
                aggroBoxOrigin.y += 1;

                // flip the rectangle on enemies
                aggroBoxHeight = -aggroBoxHeight;
            }
        }

        // Update is called once per frame
        void Update()
        {
            var units = map.GetUnitsInRect(aggroBoxOrigin, aggroBoxWidth, aggroBoxHeight);

            foreach (var unit in units)
            {
                Debug.Log($"Attacking unit {unit}");
            }
        }
    }
}