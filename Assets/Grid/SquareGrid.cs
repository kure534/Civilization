using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SquareGrid : MonoBehaviour
{
    [SerializeField] Grid grid;

    private Input input;
    private SquareCell[,] squareCells;
    private Canvas gridCanvas;
    public static SquareGrid fieldGrid { get; private set; }
    void Awake()
    {
        // Just filling the fields
        fieldGrid = this;
        grid.gridCanvas = GetComponentInChildren<Canvas>();
    }
    /// <summary>
    /// Move <paramref name="unit"/> on a <paramref name="vector"/>
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="vector"></param>
    public void MoveUnit(UnitController unit, Vector2Int vector)
    {
        unit.coordinates += vector;
        grid.TransferCoordinates(unit.coordinates, out float x, out float y);
        unit.transform.position = new Vector3(x, unit.transform.position.y, y);
        squareCells[unit.coordinates.X, unit.coordinates.Y].AddUnit(unit);
    }
    /// <summary>
    /// A* algorithm pathfinder
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="movingLevels"></param>
    public Coordinates[] QuickestPath(SquareCell start, SquareCell end, MovingLevels movingLevels)
    {
        (int, int) endCoords = end.coordinates;
        int[,] possibleField = new int[grid.height, grid.width];
        for (int i = 0; i < grid.height; i++)
        {
            for (int j = 0; j < grid.width; j++)
            {
                if((i == endCoords.Item1) && (j == endCoords.Item2))
                {
                    possibleField[i, j] = 1;
                }
                else
                {
                    switch (movingLevels)
                    {
                        case MovingLevels.simpleMoving:
                            break;
                        case MovingLevels.ableToCrossWater:
                            break;
                        case MovingLevels.ableToStayInWater:
                            break;
                        case MovingLevels.ableToFly:
                            break;
                        default:
                            break;
                    }
                    possibleField[i, j] = 1;
                }
                
            }
        }
        var res = AStarAlgorithm.FindPath(possibleField, start.coordinates, end.coordinates);
        return res;
    }
    [System.Serializable]
    private class Grid
    {
        [SerializeField] SquareCell prefab;
        [SerializeField] float prefabLength = 10;
        [Space(5)]
        [SerializeField] Text labelPrefab;
        public int height;
        public int width;

        private SquareCell[,] squareCells;
        public Canvas gridCanvas;

        /// <summary>
        /// Get field borders in world space coordinates
        /// </summary>
        /// <returns></returns>
        public Vector2 GetWorldBorders()
        {
            return new Vector2(height * prefabLength, width * prefabLength);
        }
        /// <summary>
        /// Initialize the field
        /// </summary>
        public void Initialize(in Transform parentObject)
        {
            squareCells = new SquareCell[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    CreateCell(i, j, in parentObject);
                }
            }
        }
        /// <summary>
        /// Transfer cell grid coordinates to world space coordinates
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TransferCoordinates(Coordinates coords, out float x, out float y)
        {
            x = coords.X * prefabLength;
            y = coords.Y * prefabLength;
        }
        private void CreateCell(int x, int y, in Transform parentObject)
        {
            Vector3 position = new Vector3();

            position.x = x * prefabLength;
            position.y = 0;
            position.z = y * prefabLength;

            SquareCell cell = GameObject.Instantiate(prefab, position, Quaternion.identity, parentObject);
            squareCells[x, y] = cell;
            cell.coordinates = Coordinates.FromOffsetCoordinates(x, y);

            Text label = GameObject.Instantiate<Text>(labelPrefab);
            label.rectTransform.SetParent(gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
        }
    }
    private class Input
    {
        /// <summary>
        /// Select square cell from mouse input
        /// </summary>
        /// <returns></returns>
        public SquareCell SelectCellFromMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Cells"));
            return hit.transform.gameObject.GetComponent<SquareCell>();
        }
        /// <summary>
        /// Will invoke <paramref name="getCell"/> after selecting a square
        /// </summary>
        /// <param name="getCell"></param>
        /// <returns>ID to interrupt waiting <see cref="InterruptWaiting(int)"/></returns>
        public int WaitForSelection(Action<SquareCell> getCell)
        {
            Func<bool> checker = () =>
            {
                if (UnityEngine.Input.GetMouseButton(0))
                {
                    var cell = SelectCellFromMouse();
                    if (cell != null)
                    {
                        getCell.Invoke(cell);
                        return true;
                    }
                }
                return false;
            };
            return GameManager.Manager.StartGenericCoroutine(checker);
        }
        /// <summary>
        /// Interrupt <see cref="WaitForSelection(Action{SquareCell})"/>
        /// </summary>
        public void InterruptWaiting(int id)
        {
            GameManager.Manager.StopCoroutine(id);
        }
    }
}

public enum MovingLevels
{
    simpleMoving,
    ableToCrossWater,
    ableToStayInWater,
    ableToFly
}
public class AStarAlgorithm
{
    
    public class PathNode
    {
        public Coordinates Position { get; set; }
        public int PathLengthFromStart { get; set; }
        public PathNode CameFrom { get; set; }
        public int HeuristicEstimatePathLength { get; set; }
        public int EstimateFullPathLength
        {
            get
            {
                return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
            }
        }
    }
    public static Coordinates[] FindPath(int[,] field, Coordinates start, Coordinates goal)
    {

        var closedSet = new Collection<PathNode>();
        var openSet = new Collection<PathNode>();

        PathNode startNode = new PathNode()
        {
            Position = start,
            CameFrom = null,
            PathLengthFromStart = 0,
            HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
        };
        openSet.Add(startNode);
        while (openSet.Count > 0)
        {

            var currentNode = openSet.OrderBy(node =>
              node.EstimateFullPathLength).First();

            if (currentNode.Position == goal)
                return GetPathForNode(currentNode);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (var neighbourNode in GetNeighbours(currentNode, goal, field))
            {

                if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0)
                    continue;
                var openNode = openSet.FirstOrDefault(node =>
                  node.Position == neighbourNode.Position);

                if (openNode == null)
                    openSet.Add(neighbourNode);
                else
                  if (openNode.PathLengthFromStart < neighbourNode.PathLengthFromStart)
                {

                    openNode.CameFrom = currentNode;
                    openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                }
            }
        }

        return null;
    }
    private static int GetDistanceBetweenNeighbours()
    {
        return 1;
    }
    private static int GetHeuristicPathLength(Coordinates from, Coordinates to)
    {
        return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }
    private static Collection<PathNode> GetNeighbours(PathNode pathNode, Coordinates goal, int[,] field)
    {
        var result = new Collection<PathNode>();


        Coordinates[] neighbourPoints = new Coordinates[4];
        neighbourPoints[0] = new Coordinates(pathNode.Position.X + 1, pathNode.Position.Y);
        neighbourPoints[1] = new Coordinates(pathNode.Position.X - 1, pathNode.Position.Y);
        neighbourPoints[2] = new Coordinates(pathNode.Position.X, pathNode.Position.Y + 1);
        neighbourPoints[3] = new Coordinates(pathNode.Position.X, pathNode.Position.Y - 1);

        foreach (var point in neighbourPoints)
        {
            if (point.X < 0 || point.X >= field.GetLength(0))
                continue;
            if (point.Y < 0 || point.Y >= field.GetLength(1))
                continue;
            if(field[point.X, point.Y] == 0)
                continue;
            var neighbourNode = new PathNode()
            {
                Position = point,
                CameFrom = pathNode,
                PathLengthFromStart = pathNode.PathLengthFromStart +
                GetDistanceBetweenNeighbours(),
                HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
            };
            result.Add(neighbourNode);
        }
        return result;
    }
    private static Coordinates[] GetPathForNode(PathNode pathNode)
    {
        var result = new List<Coordinates>();
        var currentNode = pathNode;
        while (currentNode != null)
        {
            result.Add(currentNode.Position);
            currentNode = currentNode.CameFrom;
        }
        result.Reverse();
        return result.ToArray();
    }
}