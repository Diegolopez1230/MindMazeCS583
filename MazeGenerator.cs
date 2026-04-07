using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject wallPrefab;   // prefab used for wall cells
    public GameObject floorPrefab;  // prefab used for open path cells
    public GameObject goalPrefab;   // prefab used for the goal/end cell

    [Header("Tile Sprites")]
    public Sprite wallVisibleSprite; // normal wall sprite shown during reveal
    public Sprite wallGhostSprite;   // faded/hidden wall sprite shown after reveal

    // true  = wall
    // false = open path
    private bool[,] maze;

    // final generated maze dimensions in grid coordinates
    private int width, height;

    // track of all spawned tile GameObjects so they can be cleared later
    private List<GameObject> spawnedTiles = new List<GameObject>();

    // make a new maze with the given logical size (cols x rows),
    // make all tile GameObjects, and returns the player start position.
    public Vector2Int GenerateMaze(int cols, int rows)
    {
        // convert logical maze size into full grid size.
        // use odd dimensions so walls and passages alternate cleanly.
        width = cols * 2 + 1;
        height = rows * 2 + 1;

        maze = new bool[width, height];

        // start by filling the entire maze with walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = true;

        // carve out passages starting from (1,1) using the recursive backtracker algorithm
        CarvePassages(1, 1);

        // find the farthest reachable open cell from the start = goal position
        Vector2Int goalCell = FindFarthestCell(1, 1);

        // put in visible GameObjects for each maze cell
        SpawnTiles(goalCell);

        // player's starting grid position
        return new Vector2Int(1, 1);
    }


    // recursively carves paths through the maze by visiting next cells
    private void CarvePassages(int cx, int cy)
    {
        // directions -> 0 = North, 1 = East, 2 = South, 3 = West
        int[] dirs = { 0, 1, 2, 3 };

        // change directions so each maze is random
        ShuffleArray(dirs);

        foreach (int dir in dirs)
        {
            int nx = cx, ny = cy; // next cell coordinates
            int wx = cx, wy = cy; // wall coordinates between current and next cell

            // move 2 spaces to the next cell and 1 space to the wall in between
            switch (dir)
            {
                case 0: ny += 2; wy += 1; break; // north
                case 1: nx += 2; wx += 1; break; // east
                case 2: ny -= 2; wy -= 1; break; // south
                case 3: nx -= 2; wx -= 1; break; // west
            }

            // if next cell is inside bounds and still a wall, carve through to it and continue recursively
            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && maze[nx, ny])
            {
                maze[cx, cy] = false; // current cell becomes open
                maze[wx, wy] = false; // remove wall between cells
                maze[nx, ny] = false; // next cell becomes open

                CarvePassages(nx, ny);
            }
        }
    }


    // uses Breadth-First Search to find the farthest reachable open cell
    private Vector2Int FindFarthestCell(int startX, int startY)
    {
        bool[,] visited = new bool[width, height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // assuming the start cell is the farthest
        Vector2Int farthest = new Vector2Int(startX, startY);

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        // movement in 4 directions, skipping walls
        int[] dx = { 0, 2, 0, -2 };
        int[] dy = { 2, 0, -2, 0 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // the last valid node processed will be the farthest
            farthest = current;

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                // position of wall between current and next cell
                int wx = current.x + dx[i] / 2;
                int wy = current.y + dy[i] / 2;

                // see that next cell is inside bounds, not visited, next cell is open, and wall between is open
                if (nx > 0 && nx < width && ny > 0 && ny < height
                    && !visited[nx, ny] && !maze[nx, ny] && !maze[wx, wy])
                {
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return farthest;
    }


    // makes a tile GameObject for each position in the maze
    private void SpawnTiles(Vector2Int goalCell)
    {
        // get rid of any previously spawned maze tiles
        foreach (var tile in spawnedTiles)
            Destroy(tile);

        spawnedTiles.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                GameObject tile;

                // make the correct prefab depending on cell type
                if (maze[x, y])
                {
                    tile = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
                else if (x == goalCell.x && y == goalCell.y)
                {
                    tile = Instantiate(goalPrefab, pos, Quaternion.identity, transform);
                }
                else
                {
                    tile = Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                }

                // give each tile a readable name in the Hierarchy
                tile.name = $"Tile_{x}_{y}";

                // save reference to manage tiles later
                spawnedTiles.Add(tile);
            }
        }
    }

    // returns true if the given grid cell is a wall.
    // out of bounds positions are walls.
    public bool IsWall(int gridX, int gridY)
    {
        if (gridX < 0 || gridX >= width || gridY < 0 || gridY >= height)
            return true;

        return maze[gridX, gridY];
    }

    // shows or hides maze tiles.
    // when the maze is revealed for memorizing, then hidden for gameplay then use 
    public void SetMazeVisible(bool visible)
    {
        foreach (var tile in spawnedTiles)
        {
            if (tile == null) continue;

            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            // wall tiles switch between visible and ghost sprite
            if (tile.CompareTag("Wall"))
            {
                sr.sprite = visible ? wallVisibleSprite : wallGhostSprite;

                Color c = sr.color;
                c.a = visible ? 1f : 0f; // 1 = visible, 0 = invisible
                sr.color = c;
            }
            else
            {
                // floor and goal tiles only change transparency
                Color c = sr.color;
                c.a = visible ? 1f : 0f;
                sr.color = c;
            }
        }
    }

    // changes an array in place using the Fisher-Yates algorithm
    private void ShuffleArray(int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}