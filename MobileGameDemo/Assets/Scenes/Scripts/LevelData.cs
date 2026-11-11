using UnityEngine;

public enum LevelGoalType {ClearColor, BreakIce }

[System.Serializable]
public class LevelGoal
{
    public LevelGoalType type = LevelGoalType.ClearColor;
    public TileColor color;
    public int amount;
}

[System.Serializable]
public struct IcePosition
{
    public int x;
    public int y;
}

[System.Serializable]
public struct StartingTile
{
    public int x;
    public int y;
    public TileColor color;
    public TileKind kind;
}

[CreateAssetMenu(menuName = "Match3/Level Data", fileName = "Level_")]
public class LevelData : ScriptableObject
{
    public int moveLimit = 20;
    public LevelGoal[] goals;

    [Header("Ice")]
    public IcePosition[] icePositions;

    [Header("Starting Board")]
    public StartingTile[] startingTiles = new StartingTile[0];
}