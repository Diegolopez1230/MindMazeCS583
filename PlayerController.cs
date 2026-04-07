using System.Collections;
using UnityEngine;

// makes sure GameObject always has an AudioSource
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f; // speed the player moves between grid tiles

    [Header("Audio")]
    public AudioClip wallBumpClip; // sound when hitting a wall

    // player position on the maze grid
    // matches coordinate system used by MazeGenerator
    private Vector2Int gridPos;

    // prevents new movement input while already moving
    private bool isMoving = false;

    // GameManager or other scripts to enable/disable player control
    private bool inputEnabled = false;

    // other systems in the scene
    private MazeGenerator mazeGen;
    private AudioSource audioSource;
    private GameManager gameManager;

    void Awake()
    {
        // scene references when the object is created
        audioSource = GetComponent<AudioSource>();
        mazeGen = FindObjectOfType<MazeGenerator>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        // ignore if player control is disabled or player is moving
        if (!inputEnabled || isMoving) return;

        // direction = no movement
        Vector2Int dir = Vector2Int.zero;

        // check for movement input from arrow keys or WASD
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            dir = Vector2Int.right;

        // valid direction was pressed, move
        if (dir != Vector2Int.zero)
            TryMove(dir);
    }

    private void TryMove(Vector2Int dir)
    {
        // calc the next grid position
        Vector2Int next = gridPos + dir;

        // see if target position is blocked by a wall
        if (mazeGen.IsWall(next.x, next.y))
        {
            // wall bump sound if assigned
            if (wallBumpClip)
                audioSource.PlayOneShot(wallBumpClip);

            // small bump animation to show movement was blocked
            StartCoroutine(BumpAnimation(dir));
        }
        else
        {
            // update grid position
            gridPos = next;

            // move the player to the new world position
            StartCoroutine(SmoothMove(new Vector3(next.x, next.y, 0)));

            // see if player has reached the goal after moving
            gameManager.CheckGoalReached(gridPos);
        }
    }


    // moves the player from the current position to the target position
    private IEnumerator SmoothMove(Vector3 target)
    {
        isMoving = true;

        Vector3 start = transform.position;
        float elapsed = 0f;

        float duration = 1f / moveSpeed;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap exactly to target at end to avoid precision issues
        transform.position = target;
        isMoving = false;
    }

    // short bump animation when the player runs into a wall
    private IEnumerator BumpAnimation(Vector2Int dir)
    {
        isMoving = true;

        Vector3 origin = transform.position;

        // move slightly in the blocked direction for the bump effect
        Vector3 bumpTarget = origin + new Vector3(dir.x, dir.y, 0) * 0.2f;

        float duration = 0.08f;
        float t = 0f;

        // move to the wall a little
        while (t < duration)
        {
            transform.position = Vector3.Lerp(origin, bumpTarget, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        // move to original position
        t = 0f;
        while (t < duration)
        {
            transform.position = Vector3.Lerp(bumpTarget, origin, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        // make sure exact final position
        transform.position = origin;
        isMoving = false;
    }

    // set the player's starting position
    public void SetStartPosition(Vector2Int startGrid)
    {
        gridPos = startGrid;
        transform.position = new Vector3(startGrid.x, startGrid.y, 0);
    }

    // enables or disables player movement input
    public void EnableInput(bool enable)
    {
        inputEnabled = enable;
    }
}