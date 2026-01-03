using System.Collections.Generic;
using UnityEngine;
using System.Collections;              



public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    public int width = 8;
    public int height = 8;
    public float spacing = 1.1f;
    public float tileSize = 0.95f;

    [Header("References")]
    public MatchFinder matchFinder;
    public MovesUI movesUI;

    [Header("Moves")]
    public int startMoves = 20;

    private int moves;
    private Tile[,] grid;
    private Sprite whiteSprite;


    private void Awake()
    {
        grid = new Tile[width, height];
        whiteSprite = CreateWhiteSprite();
    }

    private void Start()
    {
        if (matchFinder == null)
            matchFinder = FindFirstObjectByType<MatchFinder>();

        GenerateGrid();
        
        moves = startMoves;
        if (movesUI != null) movesUI.SetMoves(moves);

        if (matchFinder != null)
            RemoveInitialMatches();
    }

    public Vector3 GridToWorld(int x, int y)
    {
        
        return new Vector3(x * spacing, y * spacing, 0f);
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            SpawnTile(x, y, RandomColor());
        }
    }

    private void SpawnTile(int x, int y, TileColor color)
    {
        var go = new GameObject();
        go.transform.parent = transform;
        go.transform.position = GridToWorld(x, y);

        var tile = go.AddComponent<Tile>();
        go.AddComponent<BoxCollider2D>(); 

        tile.Init(new Vector2Int(x, y), color, whiteSprite, tileSize);
        grid[x, y] = tile;
        
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one; // 1x1

    }

    public Tile GetTile(Vector2Int p)
    {
        if (!IsInside(p)) return null;
        return grid[p.x, p.y];
    }

    public bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    }

    public IEnumerator RemoveMatchesAndRefill()
    {
        if (matchFinder == null) yield break;

        const int maxCascades = 20;

        for (int i = 0; i < maxCascades; i++)
        {
            var matches = matchFinder.FindAllMatches(grid, width, height);
            if (matches.Count == 0) break;
            
            foreach (var t in matches)
            {
                StartCoroutine(t.FlashWhite());
            }
            
            yield return new WaitForSeconds(0.3f);
            
            foreach (var t in matches)
            {
                t.SetColor(RandomColor());
            }

            // TODO: Add gravity and refill
        }
    }


    private void RemoveInitialMatches()
    {
        
        if (matchFinder == null) return;

         
        for (int i = 0; i < 10; i++)
        {
            var matches = matchFinder.FindAllMatches(grid, width, height);
            if (matches.Count == 0) break;
            foreach (var t in matches) t.SetColor(RandomColor());
        }
    }

    private TileColor RandomColor()
    {
        int n = System.Enum.GetValues(typeof(TileColor)).Length;
        return (TileColor)Random.Range(0, n);
    }

    private Sprite CreateWhiteSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
    
    public bool AreAdjacent(Tile a, Tile b)
    {
        Vector2Int d = a.GridPos - b.GridPos;
        return Mathf.Abs(d.x) + Mathf.Abs(d.y) == 1;
    }

    public void Highlight(Tile t, bool on)
    {
        if (t == null) return;
        t.transform.localScale = Vector3.one * (on ? tileSize * 1.08f : tileSize);
    }

    public IEnumerator TrySwap(Tile a, Tile b)
    {
        if (moves <= 0) yield break;
        
        SwapTiles(a, b);
        
        var matches = (matchFinder != null)
            ? matchFinder.FindAllMatches(grid, width, height)
            : new HashSet<Tile>();

        if (matches.Count > 0)
        {
            moves--;
            if (movesUI != null) movesUI.SetMoves(moves);

            StartCoroutine(RemoveMatchesAndRefill());
            yield break;
        }
        
        // revert swap 
        yield return new WaitForSeconds(0.05f);
        SwapTiles(a, b);
    }

    private void SwapTiles(Tile a, Tile b)
    {
        Vector2Int posA = a.GridPos;
        Vector2Int posB = b.GridPos;

        grid[posA.x, posA.y] = b;
        grid[posB.x, posB.y] = a;

        a.SetGridPos(posB);
        b.SetGridPos(posA);

        a.transform.position = GridToWorld(posB.x, posB.y);
        b.transform.position = GridToWorld(posA.x, posA.y);
    }

}
