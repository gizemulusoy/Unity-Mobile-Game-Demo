using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Animation")]
    public float swapDuration = 0.12f;
    public float fallDuration = 0.18f;
    public float refillDuration = 0.18f;

    [Header("VFX Timings")]
    public float flashDuration = 0.35f;
    public float afterFlashDelay = 0.15f;

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
            SpawnTile(x, y, RandomColor());
    }

    private void SpawnTile(int x, int y, TileColor color)
    {
        var go = new GameObject($"Tile_{x}_{y}");
        go.transform.parent = transform;
        go.transform.position = GridToWorld(x, y);

        var tile = go.AddComponent<Tile>();
        tile.Init(new Vector2Int(x, y), color, whiteSprite, tileSize);
        tile.SetEmpty(false);

        grid[x, y] = tile;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
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
    
    // Input Flow
    
    public IEnumerator TrySwap(Tile a, Tile b)
    {
        if (a == null || b == null) yield break;
        if (moves <= 0) yield break;

        // Swap Animation
        yield return StartCoroutine(SwapTilesAnimated(a, b, swapDuration));

        var matches = (matchFinder != null)
            ? matchFinder.FindAllMatches(grid, width, height)
            : new HashSet<Tile>();

        if (matches.Count > 0)
        {
            moves--;
            if (movesUI != null) movesUI.SetMoves(moves);

            yield return StartCoroutine(RemoveMatchesAndRefill());
            yield break;
        }

        
        yield return StartCoroutine(SwapTilesAnimated(a, b, swapDuration));
    }
    
    // Cascade Loop
    public IEnumerator RemoveMatchesAndRefill()
    {
        if (matchFinder == null) yield break;

        const int maxCascades = 20;

        for (int i = 0; i < maxCascades; i++)
        {
            var matches = matchFinder.FindAllMatches(grid, width, height);
            if (matches.Count == 0) break;
            
            foreach (var t in matches)
                StartCoroutine(t.FlashWhite(flashDuration));

            yield return new WaitForSeconds(flashDuration + afterFlashDelay);
            
            foreach (var t in matches)
                t.SetEmpty(true);
            
            yield return StartCoroutine(ApplyGravityAnimated());
            
            yield return StartCoroutine(RefillEmptyTilesAnimated());
        }
    }
    
    // Swap (animated)
    private IEnumerator SwapTilesAnimated(Tile a, Tile b, float duration)
    {
        Vector2Int posA = a.GridPos;
        Vector2Int posB = b.GridPos;
        
        grid[posA.x, posA.y] = b;
        grid[posB.x, posB.y] = a;

        a.SetGridPos(posB);
        b.SetGridPos(posA);

        Vector3 targetA = GridToWorld(posB.x, posB.y);
        Vector3 targetB = GridToWorld(posA.x, posA.y);

        yield return StartCoroutine(MoveTwo(a, targetA, b, targetB, duration));
    }

    private IEnumerator MoveTwo(Tile a, Vector3 aTarget, Tile b, Vector3 bTarget, float duration)
    {
        Vector3 aStart = a.transform.position;
        Vector3 bStart = b.transform.position;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration <= 0f ? 1f : (t / duration);

            a.transform.position = Vector3.Lerp(aStart, aTarget, k);
            b.transform.position = Vector3.Lerp(bStart, bTarget, k);

            yield return null;
        }

        a.transform.position = aTarget;
        b.transform.position = bTarget;
    }

    private IEnumerator MoveTo(Tile tile, Vector3 target, float duration)
    {
        Vector3 start = tile.transform.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration <= 0f ? 1f : (t / duration);
            tile.transform.position = Vector3.Lerp(start, target, k);
            yield return null;
        }

        tile.transform.position = target;
    }
    
    // Gravity 
    private IEnumerator ApplyGravityAnimated()
    {
        for (int x = 0; x < width; x++)
        {
            int emptyY = -1;

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].IsEmpty)
                {
                    if (emptyY == -1) emptyY = y;
                }
                else
                {
                    if (emptyY != -1)
                    {
                        yield return StartCoroutine(MoveTileDownAnimated(x, y, x, emptyY, fallDuration));
                        emptyY++;
                    }
                }
            }
        }
    }

    private IEnumerator MoveTileDownAnimated(int fromX, int fromY, int toX, int toY, float duration)
    {
        Tile moving = grid[fromX, fromY]; 
        Tile empty = grid[toX, toY];     
        
        grid[toX, toY] = moving;
        grid[fromX, fromY] = empty;
        
        moving.SetGridPos(new Vector2Int(toX, toY));
        empty.SetGridPos(new Vector2Int(fromX, fromY));
        
        empty.SetEmpty(true);
        moving.SetEmpty(false);
        
        Vector3 movingTarget = GridToWorld(toX, toY);
        yield return StartCoroutine(MoveTo(moving, movingTarget, duration));
        
        empty.transform.position = GridToWorld(fromX, fromY);
    }
    
    // Refill (animated from top)
    private IEnumerator RefillEmptyTilesAnimated()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].IsEmpty)
                {
                    Tile t = grid[x, y];
                    
                    Vector3 spawnPos = GridToWorld(x, height + 2);
                    t.transform.position = spawnPos;

                    t.SetColor(RandomColor());
                    t.SetEmpty(false);

                    Vector3 target = GridToWorld(x, y);
                    yield return StartCoroutine(MoveTo(t, target, refillDuration));
                }
            }
        }
    }
    
    // Initial Cleanup
    private void RemoveInitialMatches()
    {
        if (matchFinder == null) return;

        for (int i = 0; i < 10; i++)
        {
            var matches = matchFinder.FindAllMatches(grid, width, height);
            if (matches.Count == 0) break;

            foreach (var t in matches)
            {
                t.SetColor(RandomColor());
                t.SetEmpty(false);
            }
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
}
