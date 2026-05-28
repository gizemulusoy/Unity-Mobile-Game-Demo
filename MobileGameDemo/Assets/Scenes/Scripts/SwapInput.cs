using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SwapInput : MonoBehaviour
{
    public GridManager gridManager;
    private Tile selected;

    void Update()
    {
        // block gameplay input 
        if (IsPointerOverUI())
            return;
        
        if (Input.GetMouseButtonDown(0))
            Debug.Log("CLICK!");

        
        if (!Input.GetMouseButtonDown(0)) return;

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f; 
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero);

        if (!hit) return;

        Tile clicked = hit.collider.GetComponent<Tile>();
        if (clicked == null) return;

        HandleClick(clicked);
    }
    
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        // mobile touch
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        // mouse 
        return EventSystem.current.IsPointerOverGameObject();
    }

    void HandleClick(Tile clicked)
    {
        if (selected == null)
        {
            selected = clicked;
            gridManager.Highlight(selected, true);
            return;
        }

        if (clicked == selected)
        {
            gridManager.Highlight(selected, false);
            selected = null;
            return;
        }

        if (!gridManager.AreAdjacent(selected, clicked))
        {
            gridManager.Highlight(selected, false);
            selected = clicked;
            gridManager.Highlight(selected, true);
            return;
        }

        gridManager.Highlight(selected, false);
        StartCoroutine(gridManager.TrySwap(selected, clicked));
        selected = null;
    }
}