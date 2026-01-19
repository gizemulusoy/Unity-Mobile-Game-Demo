using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Data")]
    public LevelDatabase database;
    public int startLevelIndex = 0;

    [Header("Refs")]
    public GridManager grid;
    
    [Header("UI")]
    public GameObject levelStartPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    public int CurrentLevelIndex { get; private set; }
    public LevelData CurrentLevel { get; private set; }
    public int MovesLeft { get; private set; }

    public LevelGoal[] RuntimeGoals { get; private set; }

    private bool isTransitioning;

    private void Start()
    {
        // Always start from startLevelIndex
        CurrentLevelIndex = Mathf.Clamp(startLevelIndex, 0, database.levels.Length - 1);
        StartLevel(CurrentLevelIndex);
    }

    public void StartLevel(int index)
    {
        CurrentLevelIndex = Mathf.Clamp(index, 0, database.levels.Length - 1);

        CurrentLevel = database.levels[CurrentLevelIndex];
        MovesLeft = CurrentLevel.moveLimit;

        // Push move limit into the grid system
        if (grid != null)
        {
            grid.SetMovesFromLevel(MovesLeft);

            // Reset board only for levels after Level 1
            // because it contains temporary debug elements
            if (CurrentLevelIndex > 0)
                grid.ResetBoardForNewLevel();//reset the board when a new level starts
        }
        
        RuntimeGoals = new LevelGoal[CurrentLevel.goals.Length];
        for (int i = 0; i < RuntimeGoals.Length; i++)
        {
            RuntimeGoals[i] = new LevelGoal
            {
                type = CurrentLevel.goals[i].type,
                color = CurrentLevel.goals[i].color,
                amount = CurrentLevel.goals[i].amount
            };
        }
        
        if (grid != null)
            grid.onColorCleared = HandleColorCleared;
        else
            Debug.LogError("LevelManager: Grid reference was not assigned in the Inspector!");

        Debug.Log($"Level {CurrentLevelIndex + 1} started | Moves: {MovesLeft} | Goal: {RuntimeGoals[0].color} x {RuntimeGoals[0].amount}");

        if (winPanel != null)
            winPanel.SetActive(false);
        
        if (losePanel != null)
            losePanel.SetActive(false);

    }

    // Called when tiles of a specific color are cleared from the grid.
    // Updates level goals and checks for win condition.
    private void HandleColorCleared(TileColor color, int count)
    {
        DecreaseColorGoal(color, count);

        if (!isTransitioning && AllGoalsDone())
        {
            isTransitioning = true;
            Debug.Log("WIN! -> Next Level");
            //NextLevel();
            //isTransitioning = false;
            if (winPanel != null)
                winPanel.SetActive(true);
        }
    }

    public void NextLevel()
    {
        int next = Mathf.Min(CurrentLevelIndex + 1, database.levels.Length - 1);
        Debug.Log("NextLevel -> Level " + (next + 1));
        StartLevel(next);
    }

    public void DecreaseColorGoal(TileColor color, int count)
    {
        if (RuntimeGoals == null) return;

        for (int i = 0; i < RuntimeGoals.Length; i++)
        {
            var g = RuntimeGoals[i];

            if (g.type == LevelGoalType.ClearColor && g.color == color && g.amount > 0)
            {
                g.amount = Mathf.Max(0, g.amount - count);
                RuntimeGoals[i] = g;

                Debug.Log($"Goal update: {g.color} kaldı {g.amount}");
            }
        }
    }

    public bool AllGoalsDone()
    {
        if (RuntimeGoals == null) return false;

        for (int i = 0; i < RuntimeGoals.Length; i++)
            if (RuntimeGoals[i].amount > 0) return false;

        return true;
    }
    
    public void OnPressStartFromBeginning()
    {
        if (levelStartPanel != null)
            levelStartPanel.SetActive(false);
        
        startLevelIndex = 0;
        StartLevel(0);
    }

    public void OnPressGoNextLevel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        isTransitioning = false; // allow win to trigger again in the next level
        NextLevel();
    }

    public void OnPressBackToMainMenu()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (levelStartPanel != null)
            levelStartPanel.SetActive(true);

        isTransitioning = false;
    }
    
    public void OnPressRestartCurrentLevel()
    {
        Debug.Log("Restarting current level: " + (CurrentLevelIndex + 1));

        isTransitioning = false;

        // fresh board
        if (grid != null)
            grid.ResetBoardForNewLevel();

        StartLevel(CurrentLevelIndex);
    }
    
    public void OnMoveConsumed(int remainingMoves)
    {
        MovesLeft = remainingMoves;

        if (MovesLeft <= 0 && !AllGoalsDone())
        {
            Debug.Log("LOSE!");

            if (losePanel != null)
                losePanel.SetActive(true);
        }
    }
    
}
