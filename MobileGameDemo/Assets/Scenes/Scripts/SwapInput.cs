using UnityEngine;
using System.Collections;

public class SwapInput : MonoBehaviour
{
    public GridManager gridManager;
    private Tile selected;

    void Update()
    {
        
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