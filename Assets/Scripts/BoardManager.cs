using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    private class Cell
    {
        private Cell north, south, east, west;

        public readonly GameObject gameObject;
        public bool visited;

        private Cell North
        {
            get
            {
                if (north == null)
                {
                    Cell value;

                    Vector2 position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - distanceBetweenCells);
                    if (Cells.TryGetValue(position, out value))
                    {
                        value.south = this;
                        return north = value;
                    }
                    else
                        return null;
                }
                else
                    return north;
            }
        }
        private Cell South
        {
            get
            {
                if (south == null)
                {
                    Cell value;

                    Vector2 position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + distanceBetweenCells);
                    if (Cells.TryGetValue(position, out value))
                    {
                        value.north = this;
                        return south = value;
                    }
                    else
                        return null;
                }
                else
                    return south;
            }
        }
        private Cell West
        {
            get
            {
                if (west == null)
                {
                    Cell value;

                    Vector2 position = new Vector2(gameObject.transform.position.x - distanceBetweenCells, gameObject.transform.position.y);
                    if (Cells.TryGetValue(position, out value))
                    {
                        value.east = this;
                        return west = value;
                    }
                    else
                        return null;
                }
                else
                    return west;
            }
        }
        private Cell East
        {
            get
            {
                if (east == null)
                {
                    Cell value;
                    Vector2 vector2 = new Vector2(gameObject.transform.position.x + distanceBetweenCells, gameObject.transform.position.y);

                    if (Cells.TryGetValue(vector2, out value))
                    {
                        value.west = this;
                        return east = value;
                    }
                    else
                        return null;
                }
                else
                    return east;
            }
        }

        public Cell[] Neighbours
        {
            get
            {
                return new Cell[] { North, West, East, South };
            }
        }
        public Cell[] UnvisitedNeighbours
        {
            get
            {
                return (from t in Neighbours where t != null && t.visited == false select t).ToArray();
            }
        }

        public Cell GetRandomUnvisitedNeighbour()
        {
            Cell[] unvisitedNeighbours = UnvisitedNeighbours;

            if (unvisitedNeighbours.Length > 0)
                return unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Length)];
            return null;
        }
        public void ClearNeighBours()
        {
            north = west = east = south = null;
            visited = false;
        }

        public Cell()
        {
        }
        public Cell(GameObject gameObject) : this()
        {
            this.gameObject = gameObject;
        }

        public override string ToString()
        {
            return gameObject.transform.position.ToString();
        }
    }

    static private Dictionary<Vector2, Cell> Cells;
    static private Dictionary<Vector2, GameObject> Walls;
    static private int distanceBetweenCells;
    private GameObject[] Coins;
    private Cell startingCell;

    public GameObject wallTile;
    public GameObject floorTile;
    public GameObject zombieTile;
    public GameObject mummyTile;
    public GameObject coinTile;

    public Transform boardholder;

    public int columns = 9;
    public int rows = 9;

    void BoardSetup()
    { 
        boardholder = new GameObject("Board").transform;

        columns = columns + columns - 1; // 
        rows = rows + rows - 1; // it is because walls are units too

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject toInstantiate;

                bool isWall;

                if (isWall = (y % 2 == 1) || (x % 2 != y % 2))
                {
                    toInstantiate = wallTile;
                }
                else
                    toInstantiate = floorTile;

                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y), Quaternion.identity);

                instance.transform.SetParent(boardholder);

                if (isWall)
                    Walls.Add(instance.transform.position, instance);
                else
                {
                    Cell cellToAdd = new Cell(instance);

                    if (x == 0 && y == 0)
                        startingCell = cellToAdd;

                    Cells.Add(instance.transform.position, cellToAdd);
                }
            }
        }
    }
    void CreateOuterWalls(int outerWallsLength = 1)
    {
        if (outerWallsLength <= 0)
            return;

        for (int x = -outerWallsLength; x < columns - 1 + 1 + outerWallsLength; x++)
        {
            for (int y = -1; y >= -outerWallsLength; y--)
            {
                GameObject toInstantiate = wallTile;

                GameObject instance = Instantiate(wallTile, new Vector2(x, y), Quaternion.identity);
            }
            for (int y = rows - 1 + 1; y < rows - 1 + 1 + outerWallsLength; y++)
            {
                GameObject toInstantiate = wallTile;

                GameObject instance = Instantiate(wallTile, new Vector2(x, y), Quaternion.identity);
            }
        }
        for (int y = -outerWallsLength; y < rows - 1 + 1 + outerWallsLength; y++)
        {
            for (int x = -1; x >= -outerWallsLength; x--)
            {
                GameObject toInstantiate = wallTile;

                GameObject instance = Instantiate(wallTile, new Vector2(x, y), Quaternion.identity);
            }
            for (int x = columns - 1 + 1; x < columns - 1 + 1 + outerWallsLength; x++)
            {
                GameObject toInstantiate = wallTile;

                GameObject instance = Instantiate(wallTile, new Vector2(x, y), Quaternion.identity);
            }
        }
    }
    void CreateMaze() // DFS Recursive backtracker
    {
        Cell current = startingCell;
        current.visited = true;

        Stack<Cell> stack = new Stack<Cell>();

        while (true)
        {
            Cell randomNeighbour = current.GetRandomUnvisitedNeighbour();

            if (randomNeighbour != null)
            {
                randomNeighbour.visited = true;

                stack.Push(current);

                // replace the wall that is between the random cell and 
                // and the current cell with the floor material, digging the maze
                ReplaceWallWithFloorBetween(current, randomNeighbour);

                current = randomNeighbour;
            }
            else if (stack.Count != 0)
            {
                current = stack.Pop();
            }
            else break;
        }
    }
    void ReplaceWallWithFloorBetween(Cell cell1, Cell cell2)
    {
        // Deletes the wall between 2 cells and creates another cell there

        Vector2 index = (cell1.gameObject.transform.position + cell2.gameObject.transform.position) / 2;

        Destroy(Walls[index]);
        Walls.Remove(index);

        GameObject instance = Instantiate(floorTile, index, Quaternion.identity);
        instance.transform.SetParent(boardholder);

        Cells.Add(instance.transform.position, new Cell(instance));
    }
    void UpdateCells()
    {
        // After maze is generated, Cells no longer have the distance between them of 2 points.
        // Instead, they are either connected or not, and the distance between connected is 1 point.
        // Also, their neighbours need to be refreshed, i.e 1) been set to null 2) been gotten again next time using properties

        distanceBetweenCells = 1;
        foreach (var i in Cells)
            i.Value.ClearNeighBours();
    }
    bool WallExistsAt(Vector3 vector3)
    {
        GameObject value;

        return Walls.TryGetValue(vector3, out value);
    }

    public void SetupScene()
    {
        Debug.Log("BM hash: " + this.GetHashCode());

        Cells = new Dictionary<Vector2, Cell>();
        Walls = new Dictionary<Vector2, GameObject>();
        distanceBetweenCells = 2;

        BoardSetup();
        CreateMaze();
        CreateOuterWalls(5);
        UpdateCells();
    }
    public IEnumerator UpdateCoins()
    {
        while (true)
        {
            if (Coins == null)
                Coins = new GameObject[10];

            int position = Array.FindIndex(Coins, t => t == null);

            if (position != -1)
            {
                GameObject toInstantiate = coinTile;

                Vector2 randomCoords;
                do // while coords aren't special - need to generate coords there isn't already a coin at.
                {
                    randomCoords = GetRandomCellCoords();
                } while (Coins.Any(t => t != null && (Vector2)t.transform.position == randomCoords));


                GameObject instance = Instantiate(coinTile, randomCoords, Quaternion.identity);
                instance.transform.SetParent(boardholder);
                Coins[position] = instance;
            }
            yield return new WaitForSeconds(5f);
        }
    }
    public void SpawnEnemy(Vector2 coords, bool Mummy = false)
    {
        GameObject toInstantiate = Mummy ? mummyTile : zombieTile;
        GameObject instance = Instantiate(toInstantiate, coords, Quaternion.identity);
        instance.transform.SetParent(boardholder);
    }

    public Vector2 GetRandomCellCoords()
    {
        return Cells.Values.ElementAt(Random.Range(0, Cells.Values.Count)).gameObject.transform.position;
    }
    public Vector2[] GetPathFromTo(Vector2 start, Vector2 end)
    {
        // Returns the path from start to end excluding start vector
        // uses DFS since there's always only 1 path from one point to another 

        Cell current, final;
        Cell[] resultingCells;

        /* //////////// DEBUG ////////// */
        bool gottenStartValue = Cells.TryGetValue(start, out current);
        bool gottenEndValue = Cells.TryGetValue(end, out final);

        if (!gottenStartValue)
        {
            Debug.LogError(
               String.Format(
                   "Start Error in RandomPathWithin: start = {0}, end = {1}", start.ToString(), end.ToString()
                   )
               );
            return null;
        }
        if (!gottenEndValue)
        {
            Debug.LogError(
                String.Format(
                   "End Error in RandomPathWithin: start = {0}, end = {1}", start.ToString(), end.ToString()
                    )
                );
            return null;
        }
        /* /////////// DEBUG END /////// */

        current.visited = true;

        Stack<Cell> stack = new Stack<Cell>();

        while (current != final) // push to stack path from current to final.
        {
            Cell randomNeighbour = current.GetRandomUnvisitedNeighbour();

            if (randomNeighbour != null)
            {
                randomNeighbour.visited = true;

                stack.Push(current);

                current = randomNeighbour;
            }
            else if (stack.Count != 0)
            {
                current = stack.Pop();
            }
        }
        stack.Push(final);

        resultingCells = stack.Reverse().Where(t => t != Cells[start]).ToArray(); // excluding start

        foreach (var i in Cells.Values)
            i.visited = false;

        return resultingCells.Select(t => (Vector2)t.gameObject.transform.position).ToArray();
    }
    public Vector2[] GetRandomPathWithin(Vector2 start, int Steps)
    {
        Cell current;
        bool gottenValue = Cells.TryGetValue(start, out current);

        if (!gottenValue)
            Debug.LogError(String.Format("Error in RandomPathWithin: X was {0}, Y was {1}", start.x, start.y));

        Cell previous = current;

        Vector2[] path = new Vector2[Steps];
        for (int i = 0; i < Steps; i++)
        {
            Cell[] AvailablePath = current.Neighbours.Where(t => t != previous && t != null).ToArray();
            if (AvailablePath.Length == 0)
                break;

            previous = current;
            current = AvailablePath[Random.Range(0, AvailablePath.Length)];

            path[i] = current.gameObject.transform.position;
        }

        return path.Where(t => t != default(Vector2)).ToArray();
    }
}
