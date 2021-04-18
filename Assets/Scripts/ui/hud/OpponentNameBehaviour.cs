using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Network;
using Pandora.Network.Data;
using DG.Tweening;
using UnityEngine.UI;

namespace Pandora.UI.HUD
{
    public class OpponentNameBehaviour : MonoBehaviour
    {
        // Start is called before the first frame update
        public float FadeDuration = 1.5f;
        public Color[] PositionColors;

        void Start()
        {
            var opponent = TeamComponent.Opponent;

            if (opponent != null)
            {
                var image = GetComponent<Image>();
                image.DOFade(1f, FadeDuration);

                var text = GetComponentInChildren<Text>();

                text.text = opponent.Name;

                text.DOFade(1f, FadeDuration);

                if (opponent.Position <= 3 && PositionColors.Length >= opponent.Position) {
                    image.color = PositionColors[opponent.Position.Value - 1];
                }
            }
        }
    }
}