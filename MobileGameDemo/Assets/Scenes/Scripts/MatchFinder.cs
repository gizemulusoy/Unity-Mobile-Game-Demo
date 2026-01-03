using System.Collections.Generic;
using UnityEngine;

public class MatchFinder : MonoBehaviour
{
    public HashSet<Tile> FindAllMatches(Tile[,] grid, int width, int height)
    {
        var result = new HashSet<Tile>();

        
        for (int y = 0; y < height; y++)
        {
            int run = 1;
            for (int x = 1; x < width; x++)
            {
                if (grid[x, y].ColorType == grid[x - 1, y].ColorType) run++;
                else
                {
                    if (run >= 3)
                        for (int k = 0; k < run; k++) result.Add(grid[(x - 1) - k, y]);
                    run = 1;
                }
            }
            if (run >= 3)
                for (int k = 0; k < run; k++) result.Add(grid[(width - 1) - k, y]);
        }

        
        for (int x = 0; x < width; x++)
        {
            int run = 1;
            for (int y = 1; y < height; y++)
            {
                if (grid[x, y].ColorType == grid[x, y - 1].ColorType) run++;
                else
                {
                    if (run >= 3)
                        for (int k = 0; k < run; k++) result.Add(grid[x, (y - 1) - k]);
                    run = 1;
                }
            }
            if (run >= 3)
                for (int k = 0; k < run; k++) result.Add(grid[x, (height - 1) - k]);
        }

        return result;
    }
}