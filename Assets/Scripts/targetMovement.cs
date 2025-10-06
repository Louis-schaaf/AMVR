using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetMovement : MonoBehaviour
{
    [Header("Horizontal Movement Boundaries")]
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;

    [Header("Vertical Movement Boundaries")]
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 3f;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;

    private bool movingPositive = true;

    [SerializeField] private bool moveHorizontally = true; // true = horizontal, false = vertical

    void Update()
    {
        // Toggle between horizontal and vertical movement
        if (Input.GetKeyDown(KeyCode.T))
        {
            moveHorizontally = !moveHorizontally;
            movingPositive = true; // reset direction when toggling
        }

        float step = speed * Time.deltaTime;

        if (moveHorizontally)
        {
            // Horizontal movement
            if (movingPositive)
            {
                transform.position += Vector3.right * step;
                if (transform.position.x >= maxX)
                    movingPositive = false;
            }
            else
            {
                transform.position += Vector3.left * step;
                if (transform.position.x <= minX)
                    movingPositive = true;
            }
        }
        else
        {
            // Vertical movement
            if (movingPositive)
            {
                transform.position += Vector3.up * step;
                if (transform.position.y >= maxY)
                    movingPositive = false;
            }
            else
            {
                transform.position += Vector3.down * step;
                if (transform.position.y <= minY)
                    movingPositive = true;
            }
        }
    }
}
