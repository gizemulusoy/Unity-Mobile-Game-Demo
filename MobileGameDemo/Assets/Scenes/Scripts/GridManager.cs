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

    private Tile lastSwapA;
    private Tile lastSwapB;

    private struct MatchLine
    {
        public List<Tile> tiles;
        public bool horizontal;
        public bool isSquare;  //2x2

        public MatchLine(List<Tile> tiles, bool horizontal, bool isSquare = false)
        {
            this.tiles = tiles;
            this.horizontal = horizontal;
            this.isSquare = isSquare;  //2x2
        }
    }
    
    // for Level System 
    public System.Action<TileColor, int> onColorCleared;
    
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
        
        if (matchFinder != null)
            RemoveInitialMatches();
        
        // TEST: Spawn a ColorBomb at start for debugging
        SpawnTestColorBomb(new Vector2Int(width / 2, height / 2));
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

        lastSwapA = a;
        lastSwapB = b;

        // Swap Animation
        yield return StartCoroutine(SwapTilesAnimated(a, b, swapDuration));

        if ((a.Kind == TileKind.ColorBomb && !b.IsEmpty) || (b.Kind == TileKind.ColorBomb && !a.IsEmpty))
        {
            ConsumeMove();

            if (a.Kind == TileKind.ColorBomb && b.Kind == TileKind.ColorBomb)
            {
                yield return StartCoroutine(ResolveColorBombSwap(null));
            }
            else
            {
                TileColor target = (a.Kind == TileKind.ColorBomb) ? b.ColorType : a.ColorType;
                yield return StartCoroutine(ResolveColorBombSwap(target));
            }

            lastSwapA = null;
            lastSwapB = null;
            yield break;
        }

        var lines = FindMatchLines();
        if (lines.Count > 0)
        {
            ConsumeMove();
            
            yield return StartCoroutine(RemoveMatchesAndRefill());
            yield break;
        }

        
        yield return StartCoroutine(SwapTilesAnimated(a, b, swapDuration));

        lastSwapA = null;
        lastSwapB = null;
    }
    
    // Cascade Loop
     public IEnumerator RemoveMatchesAndRefill()
{
    if (matchFinder == null)
        yield break;

    const int maxCascades = 20;

    for (int i = 0; i < maxCascades; i++)
    {
        var lines = FindMatchLines();
        if (lines.Count == 0)
            break;

        HashSet<Tile> matched = new HashSet<Tile>();
        foreach (var line in lines)
        {
            for (int k = 0; k < line.tiles.Count; k++)
                matched.Add(line.tiles[k]);
        }

        HashSet<Tile> toClear = new HashSet<Tile>(matched);

        foreach (var t in matched)
        {
            if (t.Kind == TileKind.RocketRow)
            {
                int y = t.GridPos.y;
                for (int x = 0; x < width; x++)
                {
                    if (!grid[x, y].IsEmpty)
                        toClear.Add(grid[x, y]);
                }
            }
            else if (t.Kind == TileKind.RocketCol)
            {
                int x = t.GridPos.x;
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y].IsEmpty)
                        toClear.Add(grid[x, y]);
                }
            }
        }

        Tile specialTile = null;
        TileKind specialKind = TileKind.Normal;

        for (int li = 0; li < lines.Count; li++)
        {
            var line = lines[li];
            if (line.isSquare)
                continue; // 2x2

            if (line.tiles.Count >= 5)
            {
                specialKind = TileKind.ColorBomb;

                if (lastSwapA != null && line.tiles.Contains(lastSwapA))
                    specialTile = lastSwapA;
                else if (lastSwapB != null && line.tiles.Contains(lastSwapB))
                    specialTile = lastSwapB;
                else
                    specialTile = line.tiles[line.tiles.Count / 2];

                break;
            }
        }

        if (specialTile == null)
        {
            for (int li = 0; li < lines.Count; li++)
            {
                var line = lines[li];
                if (line.isSquare)
                    continue; // 2x2

                if (line.tiles.Count >= 4)
                {
                    specialKind = line.horizontal
                        ? TileKind.RocketRow
                        : TileKind.RocketCol;

                    if (lastSwapA != null && line.tiles.Contains(lastSwapA))
                        specialTile = lastSwapA;
                    else if (lastSwapB != null && line.tiles.Contains(lastSwapB))
                        specialTile = lastSwapB;
                    else
                        specialTile = line.tiles[line.tiles.Count / 2];

                    break;
                }
            }
        }

        if (specialTile != null && !specialTile.IsEmpty)
        {
            specialTile.SetKind(specialKind);
            toClear.Remove(specialTile);
        }

        foreach (var t in toClear)
            StartCoroutine(t.FlashWhite(flashDuration));

        yield return new WaitForSeconds(flashDuration + afterFlashDelay);

        // for Level System : Count how many tiles of each color will be cleared in this cascade
        Dictionary<TileColor, int> counts = new Dictionary<TileColor, int>();
        foreach (var t in toClear)
        {
            if (t == null || t.IsEmpty) continue;

            if (!counts.ContainsKey(t.ColorType))
                counts[t.ColorType] = 0;

            counts[t.ColorType]++;
        }

        foreach (var t in toClear)
            t.SetEmpty(true);

        // for Level System :  Notify the LevelManager about how many tiles of each color were cleared in this cascade
        foreach (var kvp in counts)
            onColorCleared?.Invoke(kvp.Key, kvp.Value);

        yield return StartCoroutine(ApplyGravityAnimated());
        yield return StartCoroutine(RefillEmptyTilesAnimated());

        lastSwapA = null;
        lastSwapB = null;
    }
}


    
    // check again ResolveColorBombSwap !!! 
    private IEnumerator ResolveColorBombSwap(TileColor? targetColor)
    {
        HashSet<Tile> toClear = new HashSet<Tile>();

        if (targetColor.HasValue)
        {
            TileColor c = targetColor.Value;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];
                if (!t.IsEmpty && t.Kind != TileKind.ColorBomb && t.ColorType == c)
                    toClear.Add(t);
            }
        }
        else
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];
                if (!t.IsEmpty && t.Kind != TileKind.ColorBomb)
                    toClear.Add(t);
            }
        }

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Tile t = grid[x, y];
            if (!t.IsEmpty && t.Kind == TileKind.ColorBomb)
                toClear.Add(t);
        }

        foreach (var t in toClear)
            StartCoroutine(t.FlashWhite(flashDuration));

        yield return new WaitForSeconds(flashDuration + afterFlashDelay);

        foreach (var t in toClear)
            t.SetEmpty(true);

        yield return StartCoroutine(ApplyGravityAnimated());
        yield return StartCoroutine(RefillEmptyTilesAnimated());

        yield return StartCoroutine(RemoveMatchesAndRefill());
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
        bool anyMove = false;

        for (int x = 0; x < width; x++)
        {
            List<Tile> nonEmpty = new List<Tile>(height);
            List<Tile> empty = new List<Tile>(height);

            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];
                if (t.IsEmpty) empty.Add(t);
                else nonEmpty.Add(t);
            }

            List<Tile> newCol = new List<Tile>(height);
            newCol.AddRange(nonEmpty);
            newCol.AddRange(empty);

            for (int y = 0; y < height; y++)
            {
                Tile t = newCol[y];
                grid[x, y] = t;

                Vector2Int newPos = new Vector2Int(x, y);
                t.SetGridPos(newPos);

                Vector3 target = GridToWorld(x, y);

                if (t.IsEmpty)
                {
                    t.transform.position = target;
                }
                else
                {
                    if (t.transform.position != target)
                    {
                        anyMove = true;
                        StartCoroutine(MoveTo(t, target, fallDuration));
                    }
                }
            }
        }

        if (anyMove)
            yield return new WaitForSeconds(fallDuration);
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
                    t.SetKind(TileKind.Normal);
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
                t.SetKind(TileKind.Normal);
                t.SetEmpty(false);
            }
        }
    }
    
    private bool IsSameColor(Tile a, Tile b)  //2x2
    {
        if (a == null || b == null) return false;
        if (a.IsEmpty || b.IsEmpty) return false;
        return a.ColorType == b.ColorType;
    }


    private List<MatchLine> FindMatchLines()
    {
        var lines = new List<MatchLine>();

        for (int y = 0; y < height; y++)
        {
            int run = 1;
            for (int x = 1; x < width; x++)
            {
                Tile prev = grid[x - 1, y];
                Tile cur = grid[x, y];

                bool same = !prev.IsEmpty && !cur.IsEmpty && prev.ColorType == cur.ColorType;
                if (same) run++;
                else
                {
                    if (run >= 3)
                    {
                        var tiles = new List<Tile>();
                        for (int k = x - run; k < x; k++) tiles.Add(grid[k, y]);
                        lines.Add(new MatchLine(tiles, true));
                    }
                    run = 1;
                }
            }

            if (run >= 3)
            {
                var tiles = new List<Tile>();
                for (int k = width - run; k < width; k++) tiles.Add(grid[k, y]);
                lines.Add(new MatchLine(tiles, true));
            }
        }

        for (int x = 0; x < width; x++)
        {
            int run = 1;
            for (int y = 1; y < height; y++)
            {
                Tile prev = grid[x, y - 1];
                Tile cur = grid[x, y];

                bool same = !prev.IsEmpty && !cur.IsEmpty && prev.ColorType == cur.ColorType;
                if (same) run++;
                else
                {
                    if (run >= 3)
                    {
                        var tiles = new List<Tile>();
                        for (int k = y - run; k < y; k++) tiles.Add(grid[x, k]);
                        lines.Add(new MatchLine(tiles, false));
                    }
                    run = 1;
                }
            }

            if (run >= 3)
            {
                var tiles = new List<Tile>();
                for (int k = height - run; k < height; k++) tiles.Add(grid[x, k]);
                lines.Add(new MatchLine(tiles, false));
            }
        }
        
        lines.AddRange(FindSquareLines()); // 2x2
        

        return lines;
    }
    
    private List<MatchLine> FindSquareLines() // 2x2
    {
        var squares = new List<MatchLine>();

        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                Tile a = grid[x, y];
                Tile b = grid[x + 1, y];
                Tile c = grid[x, y + 1];
                Tile d = grid[x + 1, y + 1];

                if (IsSameColor(a, b) && IsSameColor(a, c) && IsSameColor(a, d))
                {
                    squares.Add(new MatchLine(new List<Tile> { a, b, c, d }, false, true));
                }
            }
        }

        return squares;
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
    
    // TEST: Spawn a ColorBomb at start for debugging 
    private void SpawnTestColorBomb(Vector2Int pos)
    { 
        if (!IsInside(pos)) return;

        Tile t = grid[pos.x, pos.y];
        if (t == null) return;
        
        t.SetKind(TileKind.ColorBomb);
        t.SetEmpty(false);
    }
    
    // Called by LevelManager when a level starts
    public void SetMovesFromLevel(int moveLimit)
    {
        moves = moveLimit;
        if (movesUI != null) movesUI.SetMoves(moves);
    }
    
    private void ConsumeMove()
    {
        moves--;
        if (movesUI != null) movesUI.SetMoves(moves);
    }


}
