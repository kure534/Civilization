using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareCell : MonoBehaviour
{
    public Coordinates coordinates;
    [SerializeReference]
    public Terrain terrain;
    private List<UnitController> units = new List<UnitController>();
    /// <summary>
    /// Number of team that staying at the moment on this cell
    /// </summary>
    private int belonging = -1;
    public UnitController[] Units { get => units.ToArray(); }
    public void AddUnit(UnitController unit)
    {
        units.Add(unit);
        if (units.Count == 0)
        {
            belonging = unit.belonging;
        }
        else
        {
            if (unit.belonging != belonging)
            {
                // Start fight between them
            }
        }
    }
}
[System.Serializable]
public class Terrain
{
    public TerrainType Type { get; private set; }
    public int Production { get => production; }
    public int Trade { get => trade; }
    public int Culture { get => culture; }
    public int Coin { get => coin; }
    public Terrain(TerrainType type)
    {
        Type = type;
    }
    [SerializeField] int production;
    [SerializeField] int trade;
    [SerializeField] int culture;
    [SerializeField] int coin;
}
public enum TerrainType
{
    Desert,
    Water,
    Mountain,
    Grassland,
    Forest
}
[System.Serializable]
public struct Coordinates
{
    [SerializeField]
    private int x, y;
    public int X { get => x; }  
    public int Y { get => y; }
    public Coordinates(int x, int y)  
    {
        this.x = x;
        this.y = y;
    }
    public static Coordinates FromOffsetCoordinates(int x, int y)
    {
        return new Coordinates(x, y);
    }
    public static Coordinates operator +(Coordinates coords, Vector2Int vector)
    {
        coords.x += vector.x;
        coords.y += vector.y;
        return coords;
    }
    public static Coordinates operator +(Coordinates coords, Coordinates coords2)
    {
        coords.x += coords2.x;
        coords.y += coords2.y;
        return coords;
    }
    public static Coordinates operator -(Coordinates coords, Coordinates coords2)
    {
        coords.x -= coords2.x;
        coords.y -= coords2.y;
        return coords;
    }
    public static implicit operator Vector2Int(Coordinates coords)
    {
        return new Vector2Int(coords.x, coords.y);
    }
    public static explicit operator Coordinates(Vector2Int vector)
    {
        return new Coordinates(vector.x, vector.y);
    }
    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ")";
    }
    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Y.ToString();
    }
    public static implicit operator Coordinates((int,int) tuple)
    {
        return new Coordinates(tuple.Item1, tuple.Item2);
    }
    public static implicit operator (int, int)(Coordinates coords)
    {
        return (coords.x, coords.y);
    }
    public static bool operator ==(Coordinates coords, Coordinates coords2)
    {
        return (coords.X == coords2.X) && (coords.Y == coords2.Y);
    }
    public static bool operator !=(Coordinates coords, Coordinates coords2)
    {
        return (coords.X == coords2.X) && (coords.Y == coords2.Y);
    }
}