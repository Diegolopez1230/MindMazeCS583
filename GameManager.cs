using System.Collections;
using UnityEngine;

// GameObject has an AudioSource for sound effects
[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [Header("References")]
    public MazeGenerator mazeGenerator;         // handles maze generation and visibility
    public PlayerController playerController;   // cntrols player movement/input
    public TimerController timerController;     // handles round countdown timer
    public UIManager uiManager;                 // manages UI panels and HUD

    [Header("Difficulty Scaling")]
    public int startCols = 4;             // starting maze width (logical columns)
    public int startRows = 4;             // starting maze height (logical rows)
    public int colsIncrement = 1;         // how much maze width increases each round
    public int rowsIncrement = 1;         // how much maze height increases each round
    public float startViewTime = 3f;      // starting time player can see the maze
    public float viewTimeDecrease = 0.2f; // amount reveal time decreases each round
    public float startRoundTime = 30f;    // starting amount of time to solve the maze
    public float roundTimeDecrease = 2f;  // amount round timer decreases each round

    [Header("Round Loop")]
    public int maxRounds = 4; // num of rounds before looping back to round 1

    [Header("Audio")]
    public AudioClip revealChime;   // sound played when maze is revealed
    public AudioClip fadeOutSound;  // sound played when maze disappears
    public AudioClip winSound;      // sound played when player wins
    public AudioClip loseSound;     // sound played when player loses

    public string goalTag = "Goal"; // tag used to find the goal object in the scene

    private int round = 1;              // current round number
    private bool roundActive = false;   // true when the player is actively playing a round
    private AudioSource audioSource;    // cached AudioSource component

    void Awake()
    {
        // cache the AudioSource attached to this GameObject
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // show title screen when the game first starts
        uiManager.ShowTitleScreen();
    }

    // starts a brand new game from round 1
    public void StartGame()
    {
        round = 1;
        StartRound();
    }

   // called by Retry button — goes back to title screen instead of retrying
public void RetryRound()
{
    mazeGenerator.SetMazeVisible(false);
    uiManager.ShowTitleScreen();
}

    // sets up and begins the current round
    private void StartRound()
    {
        // reset UI and show HUD for current round
        uiManager.HideAll();
        uiManager.ShowHUD(round);

        // scale maze size and timing depending on round number
        int cols = startCols + (round - 1) * colsIncrement;
        int rows = startRows + (round - 1) * rowsIncrement;

        // clamp times so they do not get too short
        float viewTime = Mathf.Max(1f, startViewTime - (round - 1) * viewTimeDecrease);
        float roundTime = Mathf.Max(10f, startRoundTime - (round - 1) * roundTimeDecrease);

        // make new maze and place player at the start cell
        Vector2Int startCell = mazeGenerator.GenerateMaze(cols, rows);
        playerController.SetStartPosition(startCell);

        // disable input while the player memorizes the maze
        playerController.EnableInput(false);

        // reposition the camera so the whole maze fits on screen
        CenterCamera(cols, rows);

        // start reveal/hide/gameplay flow
        StartCoroutine(RoundFlow(viewTime, roundTime));
    }

    // 1. Show maze
    // 2. Wait for memorization time
    // 3. Hide maze
    // 4. Start player input and timer
    private IEnumerator RoundFlow(float viewTime, float roundTime)
    {
        // show 
        mazeGenerator.SetMazeVisible(true);
        PlaySound(revealChime);
        uiManager.ShowRevealCountdown(viewTime);

        // wait 
        yield return new WaitForSeconds(viewTime);

        // hide
        mazeGenerator.SetMazeVisible(false);
        PlaySound(fadeOutSound);

        // start
        roundActive = true;
        playerController.EnableInput(true);

        // start countdown timer and call OnTimerExpired if time runs out
        timerController.StartTimer(roundTime, OnTimerExpired);
    }

    // checks if the player has reached the goal tile
    public void CheckGoalReached(Vector2Int playerGrid)
    {
        // ignore goal checks if round is not currently active
        if (!roundActive) return;

        // find goal GameObject by tag
        GameObject goal = GameObject.FindGameObjectWithTag(goalTag);
        if (goal == null) return;

        // convert goal world position to grid coordinates
        Vector2Int goalGrid = new Vector2Int(
            Mathf.RoundToInt(goal.transform.position.x),
            Mathf.RoundToInt(goal.transform.position.y));

        // if player position matches goal position, player wins
        if (playerGrid == goalGrid)
            OnWin();
    }

    // deals with player winning the round
private void OnWin()
{
    if (!roundActive) return;

    roundActive = false;

    // stop player interaction and timer
    playerController.EnableInput(false);
    timerController.StopTimer();

    // play win sound
    PlaySound(winSound);

    // show a game complete screen
    if (round >= maxRounds)
    {
        uiManager.ShowGameCompleteScreen();
        return;
    }

    // win screen for the completed round before next
    uiManager.ShowWinScreen(round);

    // next round
    round++;

    // start next round after a short delay
    StartCoroutine(AutoAdvance(2f));
}

    // starts the next round after delay
    private IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartRound();
    }

   // player losing because the timer expired
private void OnTimerExpired()
{
    if (!roundActive) return;

    roundActive = false;

    // get rid of movement since the round is over
    playerController.EnableInput(false);

    // play lose sound
    PlaySound(loseSound);

    // show the maze again so the player can see the path
    mazeGenerator.SetMazeVisible(true);

    // reset round back to 1 for when player returns to title
    round = 1;

    // lose screen
    uiManager.ShowLoseScreen(round);
}

    // centers and scales the camera so the full maze fits on screen
    private void CenterCamera(int cols, int rows)
    {
        // convert logical maze size into full tile dimensions
        int mazeW = cols * 2 + 1;
        int mazeH = rows * 2 + 1;

        // move camera to center of maze
        Camera.main.transform.position = new Vector3(mazeW / 2f, mazeH / 2f, -10f);

        // adjust orthographic size so the maze fits both vertically and horizontally
        float aspect = (float)Screen.width / Screen.height;
        float sizeByHeight = (mazeH / 2f) + 1f;
        float sizeByWidth = (mazeW / 2f) / aspect + 1f;

        Camera.main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }

    // plays sound effect if the clip exists
    private void PlaySound(AudioClip clip)
    {
        if (clip)
            audioSource.PlayOneShot(clip);
    }
}