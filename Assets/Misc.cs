using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misc : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(nameof(Inputer));
    }
    private IEnumerator Inputer()
    {
        bool isCreated = false; 
        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                if (!isCreated)
                {
                    SquareGrid.GridUniter.Add(UnitType.Army, 0, SquareGrid.GridInput.SelectCellFromMouse().coordinates);
                    isCreated = true;
                }
                else
                {
                    var cell = SquareGrid.GridInput.SelectCellFromMouse();
                    if (cell.Units.Length != 0)
                    {
                        GameManager.Manager.Delay(1, () => StartCoroutine(nameof(SelectPath), cell));
                    }
                }
                yield return new WaitForSeconds(1);
            }
            yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator SelectPath(SquareCell start)
    {
        SquareCell[] path = null;
        IInput gridInput = SquareGrid.GridInput;
        IGrid grid = SquareGrid.MainGrid;
        IMarker gridMarker = SquareGrid.GridMarker;
        while (!Input.GetMouseButton(0))
        {
            gridMarker.UnmarkAll();
            var end = gridInput.SelectCellFromMouse();
            path = grid.QuickestPath(start, end, MovingLevels.simpleMoving);
            if(path != null)
            {
                foreach (var square in path)
                {
                    gridMarker.Mark(square);
                }
            } 
            yield return new WaitForEndOfFrame();
        }
        Coordinates[] coords = new Coordinates[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            coords[i] = path[i].coordinates;
        }
        SquareGrid.GridUniter.Move(start.Units[0], coords);
    }
}
