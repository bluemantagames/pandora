using UnityEngine;
using Pandora.Network;
using Pandora.Engine;
using Pandora.Combat;
using Pandora.Resource.Mana;
using System.Collections.Generic;
using Pandora.UI;

namespace Pandora.Command {
    public class DeadZombieBehaviour: MonoBehaviour {
        public Sprite RedSprite, BlueSprite;
        public GameObject RespawnVFX;

        public void RefreshColor(TeamComponent team) {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            if (team.Team == TeamComponent.assignedTeam) {
                spriteRenderer.sprite = BlueSprite;
            } else {
                spriteRenderer.sprite = RedSprite;
            }
        }

        public void PlayVFX() {
            var vfx = Instantiate(RespawnVFX, transform.position, RespawnVFX.transform.rotation);

            vfx.GetComponent<ParticleSystem>().Play();
        }
    }
}