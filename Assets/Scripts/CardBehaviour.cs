﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler
{
    Vector3 originalPosition;
    MapListener mapListener;

    public GameObject puppet;
    public GameObject card;

    private void CleanUpDrag(bool returnToPosition)
    {
        if (mapListener != null)
        {
            mapListener.DestroyPuppet();
            mapListener = null;
        }

        GetComponent<Image>().enabled = true;

        if (returnToPosition)
            transform.position = originalPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;

        var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(transform.position), Vector2.up, 0f, LayerMask.GetMask("Default"));

        if (hit.collider != null) {
            mapListener = hit.collider.gameObject.GetComponent<MapListener>();

            mapListener.OnUICardCollision(puppet);

            GetComponent<Image>().enabled = false;
        } else
        {
            CleanUpDrag(false);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (mapListener != null)
            mapListener.SpawnCard(card);

        CleanUpDrag(true);
    }


    // Start is called before the first frame update
    void Awake()
    {
        if (originalPosition == null)
            originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
