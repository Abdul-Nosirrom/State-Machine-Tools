using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A series of unrelated helper methods
/// </summary>
public static class Helpers
{
    /*---------------------------------------------------------------------------*/
    private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();

    /// <summary>
    /// A memory friendly non-allocating wait for seconds method - as opposed to generating a
    /// new WaitForSeconds each time store in a dictionary and just repeatedly call to get the stored value.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static WaitForSeconds GetWait(float time)
    {
        if (WaitDictionary.TryGetValue(time, out var wait)) return wait;

        WaitDictionary[time] = new WaitForSeconds(time);
        return WaitDictionary[time];
    }
    
    /*---------------------------------------------------------------------------*/
    private static PointerEventData _eventDataCurrentPosition;
    private static List<RaycastResult> _results;
    /// <summary>
    /// Helper to check whether the cursor is over ANY UI element
    ///
    /// NEED TO MODIFY FOR NEW INPUT SYSTEM!!!!!!!!!!
    /// </summary>
    /// <returns></returns>
    //public static bool IsOverUI()
    //{
    //    _eventDataCurrentPosition = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
    //    _results = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(_eventDataCurrentPosition, _results);
    //    return _results.Count > 0;
    //}
    
    /*---------------------------------------------------------------------------*/
    /// <summary>
    /// Tie a game object (2D/3D) to a canvas/UI element by just passing in the canvas elements rect transform
    /// to this helper method - then set this game objects transform.position to the resulting Vector2
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera.main, out var result);
        return result;
    }
    
    /*---------------------------------------------------------------------------*/
    /// <summary>
    /// Destroy all children of a given game object by passing in the parents transform
    /// </summary>
    /// <param name="t"></param>
    public static void DeleteChildren(this Transform t)
    {
        foreach (Transform child in t) Object.Destroy(child.gameObject);
    }

}