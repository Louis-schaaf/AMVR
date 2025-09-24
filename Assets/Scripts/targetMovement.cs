using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetMovement : MonoBehaviour
{
    [Header("Movement Boundaries")]
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;

    private bool movingRight = true;

    void Update()
    {
        // Move horizontally
        float step = speed * Time.deltaTime;

        if (movingRight)
        {
            transform.position += Vector3.right * step;

            if (transform.position.x >= maxX)
                movingRight = false;
        }
        else
        {
            transform.position += Vector3.left * step;

            if (transform.position.x <= minX)
                movingRight = true;
        }
    }
}
