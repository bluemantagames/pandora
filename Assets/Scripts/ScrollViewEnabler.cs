using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Disable a ScrollRect if a scroll is
/// not needed.
/// </summary>
public class ScrollViewEnabler : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Skip a frame so the UI will be calculated
        yield return null;

        var scrollRect = GetComponent<ScrollRect>();

        if (scrollRect != null)
        {
            var viewport = scrollRect.viewport;
            var child = scrollRect.content;
            var isVerticalScrollNeeded = IsVerticalScrollNeeded(viewport, child);
            scrollRect.vertical = isVerticalScrollNeeded;
        }
    }

    /// <summary>
    /// Check whether a children inside the viewport
    /// is breaking out of the parent vertically
    /// </summary>
    /// <param name="viewport">The viewport RectTransform</param>
    /// <param name="children">The children RectTransform</param>
    /// <returns></returns>
    bool IsVerticalScrollNeeded(RectTransform viewport, RectTransform children)
    {
        var viewportHeight = viewport.rect.size.y;
        var childrenHeightFromVieport = children.rect.size.y;

        return viewportHeight < childrenHeightFromVieport;
    }
}
