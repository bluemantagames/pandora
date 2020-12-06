using Pandora.Engine;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Pandora {
    public class EntityHighlighter: MonoBehaviour {
        List<EntityHighlight> highlightsQueue = new List<EntityHighlight>(300);
        public EntityHighlight Current = null;
        Color originalTint;
        SpriteRenderer spriteRenderer = null;

        void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalTint = spriteRenderer.color;
        }

        public void Highlight(EntityHighlight highlight) {
            var colorHighlighter = highlight as ColorHighlight;

            spriteRenderer.color = colorHighlighter.Color;

            Current = highlight;
        }

        public void Dehighlight(EntityHighlight highlight) {
            Current = null;

            spriteRenderer.color = originalTint;
        }
    }
}