using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;

    GameControls controls;
    private Vector2 topBorder;
    private Vector2 bottomBorder;
    // Start is called before the first frame update
    void Start()
    {
        controls = GameManager.Manager.GameControls;

        Vector2 offset = new Vector2(transform.position.x, transform.position.z);
        Vector2 worldBorders = SquareGrid.mainGrid.GetWorldBorders();
        topBorder = new Vector2(worldBorders.x + offset.x, worldBorders.y + offset.y);
        bottomBorder = new Vector2(offset.x, offset.y);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Rotate();
    }
    private void Move()
    {
        transform.Translate(new Vector3(Input.GetAxis(controls.cameraHorizontalMove), 0, Input.GetAxis(controls.cameraVerticalMove)));
        Vector3 position = transform.position;
        if (position.x > topBorder.x) position.x = topBorder.x;
        else if (position.x < bottomBorder.x) position.x = bottomBorder.x;
        if (position.z > topBorder.y) position.z = topBorder.y;
        else if (position.z < bottomBorder.y) position.z = bottomBorder.y;
        transform.position = position;
    }
    private void Rotate()
    {
        transform.Rotate(new Vector3(0, Input.GetAxis(controls.cameraRotate) * rotationSpeed, 0));
    }
}
