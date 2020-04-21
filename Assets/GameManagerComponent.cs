using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[DisallowMultipleComponent]
public class GameManagerComponent : MonoBehaviour
{
    GameManager gameManager;

    [SerializeField] GameControls controls;
    [SerializeField] GameSettings settings;
    void Awake()
    {
        gameManager = new GameManager(controls, settings, this);
        gameManager.Initialize();
    }
    void Start()
    {
        SquareGrid.fieldGrid.Initialize();
    }
    public IEnumerator Delaying((float time, Action action) tuple)
    {
        yield return new WaitForSeconds(tuple.time);
        tuple.action.Invoke();
    }
    public IEnumerator GenericCoroutine(Func<bool> func)
    {
        while (true)
        {
            if (func.Invoke()) break;
            yield return new WaitForEndOfFrame();
        }
    }
}
public class GameManager
{
    public GameControls GameControls { get; private set; }
    public GameSettings GameSettings { get; private set; }
    public static GameManager Manager { get; private set; }
    private GameManagerComponent managerComponent;
    private List<Coroutine> coroutines;
    public GameManager(GameControls controls, GameSettings settings, GameManagerComponent managerComponent)
    {
        this.GameSettings = settings;
        this.GameControls = controls;
        this.managerComponent = managerComponent;
    }
    public void Initialize()
    {
        Manager = this;
        coroutines = new List<Coroutine>();
    }
    /// <summary>
    /// It will invoke <paramref name="action"/> after <paramref name="time"/> seconds
    /// </summary>
    /// <param name="time"></param>
    /// <param name="action"></param>
    public void Delay(float time, Action action)
    {
        managerComponent.StartCoroutine(nameof(managerComponent.Delaying), (time, action));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="func"></param>
    /// <returns>id of this coroutine to stop it</returns>
    public int StartGenericCoroutine(Func<bool> func)
    {
        Coroutine c = managerComponent.StartCoroutine(nameof(managerComponent.GenericCoroutine), func);
        coroutines.Add(c);

        return coroutines.Count - 1;
    }
    public void StopCoroutine(int id)
    {
        managerComponent.StopCoroutine(coroutines[id]);
        coroutines[id] = null;
    }
}
public class FightManager
{
    public void StartFight(Coordinates coords, UnitController[] units)
    {

    }
}
public enum Resource
{
    Wheat,
    Iron,
    Spy,
    Insence,
    Silk,
    Uranium,
    Any
}
public enum Stages
{
    Start,
    Trade,
    CityManagement,
    Movement,
    Battle, 
    AnyTime
}
public enum Unit
{
    Infantry,
    Mounted,
    Artillery,
    Aircraft
}
public enum Goverment
{
    Anarchy,
    Despotism,
    Republic,
    Democracy,
    Communism,
    Feudalism,
    Monarchy,
    Fundamentalism
}
[System.Serializable]
public class GameControls
{
    /// <summary>
    /// Name of axis
    /// </summary>
    public string cameraHorizontalMove;
    /// <summary>
    /// Name of axis
    /// </summary>
    public string cameraVerticalMove;
    /// <summary>
    /// Name of axis
    /// </summary>
    public string cameraRotate;
}
[System.Serializable]
public class GameSettings
{
    public float inputDelay;
}
public static class Extensions
{
    public static Vector2Int ToVector(this (int, int) tuple)
    {
        return new Vector2Int(tuple.Item1, tuple.Item2);
    }
}