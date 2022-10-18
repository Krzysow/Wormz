using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    Vector2 leftMouseDownPoint;
    Vector2 rightMouseDownPoint;
    Vector2 middleMouseDownPoint;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        leftMouseDownPoint = transform.position;
        rightMouseDownPoint = transform.position;
        middleMouseDownPoint = transform.position;
    }

    // Update is called once per frame
    override protected void Update()
    {
        if (health <= 0)
            Die();
        else if (hasTurn)
        {
            // Left, right movement
            dynamicBody.AddVelocity(Vector2.right * Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime);

            // Jump
            if (Input.GetMouseButtonDown(0))
            {
                leftMouseDownPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0) && dynamicBody.GetIsColliding())
            {
                Vector2 mouseUpPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = leftMouseDownPoint - mouseUpPoint;
                Jump(direction);
            }

            // Shoot
            if (Input.GetMouseButtonDown(1))
            {
                rightMouseDownPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(1))
            {
                Vector2 mouseUpPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = rightMouseDownPoint - mouseUpPoint;
                Shoot(direction);
            }

            // Rocket
            if (Input.GetMouseButtonDown(2))
            {
                middleMouseDownPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(2))
            {
                Vector2 mouseUpPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = middleMouseDownPoint - mouseUpPoint;
                Rocket(direction);
            }
        }

        base.Update();
    }
}
