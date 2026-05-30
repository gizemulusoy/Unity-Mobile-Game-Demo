using UnityEngine;

public enum LevelGoalType { ClearColor }

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

[CreateAssetMenu(menuName = "Match3/Level Data", fileName = "Level_")]
public class LevelData : ScriptableObject
{
    public int moveLimit = 20;
    public LevelGoal[] goals;

    [Header("Ice")]
    public IcePosition[] icePositions;
}