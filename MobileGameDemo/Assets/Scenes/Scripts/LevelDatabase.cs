using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Level Database", fileName = "LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    public LevelData[] levels;
}