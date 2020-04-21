using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SquareGrid : MonoBehaviour, IGrid, IInput, IMarker, IUniter
{
    [SerializeField] Grid grid;
    [SerializeField] Marker marker;
    [SerializeField] Uniter uniter;

    private Input input;
    private SquareCell[,] squareCells;
    private Canvas gridCanvas;
    
    public static SquareGrid fieldGrid { get; private set; }
    public static IGrid MainGrid { get => fieldGrid as IGrid; }
    public static IInput GridInput { get => fieldGrid as IInput; }
    public static IMarker GridMarker { get => fieldGrid as IMarker; }
    public static IUniter GridUniter { get => fieldGrid as IUniter; }
    void Awake()
    {
        // Just filling the fields
        input = new Input();
        fieldGrid = this;
        grid.gridCanvas = GetComponentInChildren<Canvas>();
    }
    public void Initialize() => grid.Initialize(transform);

    Vector2 IGrid.GetWorldBorders() => grid.GetWorldBorders();
    void IGrid.TransferCoordinates(Coordinates coords, out float x, out float y) => grid.TransferCoordinates(coords, out x, out y);
    SquareCell IInput.SelectCellFromMouse() => input.SelectCellFromMouse();
    int IInput.WaitForSelection(Action<SquareCell> cellAction) => input.WaitForSelection(cellAction);
    void IInput.InterruptWaiting(int id) => input.InterruptWaiting(id);

    SquareCell[] IGrid.QuickestPath(SquareCell start, SquareCell end, MovingLevels movingLevels)
    {
        Coordinates[] coords = grid.QuickestPath(start, end, movingLevels);
        if (coords == null) return null;

        SquareCell[] cells = new SquareCell[coords.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            Coordinates crds = coords[i];
            cells[i] = grid.squareCells[crds.X, crds.Y];
        }
        return cells;
    }

    void IMarker.Mark(SquareCell cell) => marker.Mark(cell);
    void IMarker.UnmarkAll() => marker.UnmarkAll();
    void IUniter.Add(UnitType type, int belongility, Coordinates coords)
    {
        grid.TransferCoordinates(coords, out float x, out float y);
        var unit = uniter.CreateUnit(type, belongility, new Vector3(x,0,y), transform);
        unit.coordinates = coords;
        grid.squareCells[coords.X, coords.Y].AddUnit(unit);
    }

    void IUniter.Move(UnitController unit, Coordinates square)
    {
        Vector3 start = grid.TransferCoordinates(unit.coordinates);
        Vector3 end = grid.TransferCoordinates(square);
        uniter.Move(unit, end-start, end);
        unit.coordinates = square;
    }
    void IUniter.Move(UnitController unit, Coordinates[] squares)
    {
        var positions = grid.TransferCoordinates(squares);
        uniter.Move(unit, positions);
    }

    void IUniter.Move(UnitController unit, Coordinates[] squares, Action action)
    {
        var positions = grid.TransferCoordinates(squares);
        uniter.Move(unit, positions, action);
    }
    
    [System.Serializable]
    private class Uniter
    {
        [SerializeField] UnitController army;
        [SerializeField] UnitController scout;
        public UnitController CreateUnit(UnitType unit, int belongility, Vector3 position, Transform parent)
        {
            UnitController unitController = null;
            switch (unit)
            {
                case UnitType.Army:
                    unitController = Instantiate(army, position, Quaternion.identity, parent);
                    break;
                case UnitType.Scout:
                    unitController = Instantiate(scout, position, Quaternion.identity, parent);
                    break;
                default:
                    break;
            }
            unitController.belonging = belongility;
            return unitController;
        }
        [SerializeField] int frames;
        public void Move(UnitController unit, Vector3 vector, Vector3 endPos)
        {
            Vector3 vect = vector / frames;
            int i = 0;
            int frams = frames;
            GameManager.Manager.StartGenericCoroutine(() =>
            {
                unit.transform.Translate(vect);
                i++;
                if (i == frames)
                {
                    unit.transform.position = endPos;
                    return true;
                }
                return false;
            });
        }
        public void Move(UnitController unit, Vector3[] positions, Action action)
        {
            MoveUnits(unit, positions, action);
        }
        public void Move(UnitController unit, Vector3[] positions)
        {
            MoveUnits(unit, positions, null);
        }
        private void MoveUnits(UnitController unit, Vector3[] positions, Action action)
        {
            int frameIterator = 0;
            int posIterator = 1;
            Vector3 vector = (positions[1] - positions[0]) / frames;
            GameManager.Manager.StartGenericCoroutine(() =>
            {
                unit.transform.Translate(vector);
                frameIterator++;
                if (frameIterator == frames)
                {
                    unit.transform.position = positions[posIterator];
                    posIterator++;
                    if (positions.Length == posIterator)
                    {
                        action?.Invoke();
                        return true;
                    }
                    vector = (positions[posIterator] - positions[posIterator - 1]) / frames;
                    frameIterator = 0;
                }
                return false;
            });
        }
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

        public SquareCell[,] squareCells;
        [HideInInspector]
        public Canvas gridCanvas;

        [SerializeField] Terrainer terrainer;

        public bool[,] water;
        public bool[,] full;

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
            water = new bool[height, width];
            full = new bool[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    CreateCell(i, j, in parentObject);
                    full[i, j] = true;
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
        /// <summary>
        /// Transfer cell grid coordinates to world space coordinates
        /// </summary>
        /// <param name="coords"></param>
        public Vector3 TransferCoordinates(Coordinates coords)
        {
           float x = coords.X * prefabLength;
           float y = coords.Y * prefabLength;
            return new Vector3(x, 0, y);
        }
        public Vector3[] TransferCoordinates(Coordinates[] coords)
        {
            Vector3[] positions = new Vector3[coords.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = TransferCoordinates(coords[i]);
            }
            return positions;
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
            terrainer.SetTerrain(ref cell);
            if(cell.terrain.Type == TerrainType.Water)
            {
                water[x, y] = false;
            }
            else
            {
                water[x, y] = true;
            }
        }
        /// <summary>
        /// A* algorithm pathfinder
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="movingLevels"></param>
        public Coordinates[] QuickestPath(SquareCell start, SquareCell end, MovingLevels movingLevels)
        {
            bool[,] possibleField = null;
            switch (movingLevels)
            {
                case MovingLevels.simpleMoving:
                    possibleField = water;
                    if (end.terrain.Type == TerrainType.Water) return null;
                    break;
                case MovingLevels.ableToCrossWater:
                    if (end.terrain.Type == TerrainType.Water) return null;
                    break;
                case MovingLevels.ableToStayInWater:
                    // But enemies
                    possibleField = full;
                    break;
                case MovingLevels.ableToFly:
                    possibleField = full;
                    break;
                default:
                    break;
            }
            possibleField[end.coordinates.X, end.coordinates.Y] = true;
            var res = AStarAlgorithm.FindPath(possibleField, start.coordinates, end.coordinates);
            return res;
        }
        [Serializable]
        private class Terrainer
        {
            [SerializeField] TerrainPool pool; 
            public void SetTerrain(ref SquareCell cell)
            {
                var tuple = pool.NextTerrain();
                cell.terrain = tuple.terrain;
                cell.GetComponent<Renderer>().material = tuple.material;
            }
            [Serializable]
            private class TerrainPool
            {
                [SerializeField] Terrain desert = new Terrain(TerrainType.Desert);
                [SerializeField] Material desertMaterial;
                [Space(4)]
                [SerializeField] Terrain forest = new Terrain(TerrainType.Forest);
                [SerializeField] Material forestMaterial;
                [Space(4)]
                [SerializeField] Terrain water = new Terrain(TerrainType.Water);
                [SerializeField] Material waterMaterial;
                [Space(4)]
                [SerializeField] Terrain mountain = new Terrain(TerrainType.Mountain);
                [SerializeField] Material mountainMaterial;
                [Space(4)]
                [SerializeField] Terrain grassland = new Terrain(TerrainType.Grassland);
                [SerializeField] Material grasslandMaterial;

                public virtual (Terrain terrain, Material material) NextTerrain()
                {
                    int num = UnityEngine.Random.Range(1, 10);
                    switch (num)
                    {
                        case 1:
                        case 2:
                            return (desert, desertMaterial);
                        case 3:
                        case 4:
                            return (forest, forestMaterial);
                        case 5:
                        case 6:
                            return (mountain, mountainMaterial);
                        case 7:
                        case 8:
                            return (grassland, grasslandMaterial);
                        case 9:
                            return (water, waterMaterial);
                        default:
                            break;
                    }
                    throw new Exception();
                }
            }
        }
        private class AStarAlgorithm
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
            public static Coordinates[] FindPath(bool[,] field, Coordinates start, Coordinates goal)
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
            private static Collection<PathNode> GetNeighbours(PathNode pathNode, Coordinates goal, bool[,] field)
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
                    if (!field[point.X, point.Y])
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
            return hit.transform?.gameObject.GetComponent<SquareCell>();
        }
        /// <summary>
        /// Will invoke <paramref name="cellAction"/> after selecting a square
        /// </summary>
        /// <param name="cellAction"></param>
        /// <returns>ID to interrupt waiting <see cref="InterruptWaiting(int)"/></returns>
        public int WaitForSelection(Action<SquareCell> cellAction)
        {
            Func<bool> checker = () =>
            {
                if (UnityEngine.Input.GetMouseButton(0))
                {
                    var cell = SelectCellFromMouse();
                    if (cell != null)
                    {
                        cellAction.Invoke(cell);
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
    [System.Serializable]
    private class Marker
    {
        [SerializeField] Material markMaterial;

        List<(Renderer rend, Material mat)> marked = new List<(Renderer, Material)>();
        public void Mark(SquareCell cell)
        {
            Renderer rend = cell.GetComponent<Renderer>();
            var mat = rend.material;
            marked.Add((rend, mat));
            rend.material = markMaterial;
        }
        public void UnmarkAll()
        {
            foreach (var item in marked)
            {
                item.rend.material = item.mat;
            }
            marked.Clear();
        }
    }
}
public interface IGrid
{
    /// <summary>
    /// Get field borders in world space coordinates
    /// </summary>
    /// <returns></returns>
    Vector2 GetWorldBorders();
    /// <summary>
    /// Transfer cell's grid coordinates to world space coordinates
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void TransferCoordinates(Coordinates coords, out float x, out float y);
    /// <summary>
    /// A* algorithm pathfinder
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="movingLevels"></param>
    SquareCell[] QuickestPath(SquareCell start, SquareCell end, MovingLevels movingLevels);
}
public interface IInput
{
    /// <summary>
    /// Select square cell from mouse input
    /// </summary>
    /// <returns></returns>
    SquareCell SelectCellFromMouse();
    /// <summary>
    /// Will invoke <paramref name="getCell"/> after selecting a square
    /// </summary>
    /// <param name="getCell"></param>
    /// <returns>ID to interrupt waiting <see cref="InterruptWaiting(int)"/></returns>
    int WaitForSelection(Action<SquareCell> getCell);
    /// <summary>
    /// Interrupt <see cref="WaitForSelection(Action{SquareCell})"/>
    /// </summary>
    void InterruptWaiting(int id);
}
public interface IUniter    
{
    void Add(UnitType unitType, int belongility, Coordinates coords);
    void Move(UnitController unit, Coordinates square);
    void Move(UnitController unit, Coordinates[] squares);
    void Move(UnitController unit, Coordinates[] squares, Action action);
}
public interface IMarker
{
    void Mark(SquareCell cell);
    /// <summary>
    /// Unmark all marked cells
    /// </summary>
    void UnmarkAll();
}
public enum MovingLevels
{
    simpleMoving,
    ableToCrossWater,
    ableToStayInWater,
    ableToFly
}
public enum UnitType
{
    Army,
    Scout
}