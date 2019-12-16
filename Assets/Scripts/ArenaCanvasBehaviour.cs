using UnityEngine;
using UnityEngine.EventSystems;
using Pandora.Deck;

namespace Pandora
{
    public class ArenaCanvasBehaviour : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            var hit = Physics2D.Raycast(
                Camera.main.ScreenToWorldPoint(transform.position), Vector2.up, 0f,
                LayerMask.GetMask("Map")
            );

            Logger.Debug("Pointer down");

            if (hit.collider != null && hit.collider.gameObject.GetComponent<MapComponent>() != null)
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
}