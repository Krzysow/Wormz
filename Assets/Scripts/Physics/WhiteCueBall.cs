using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteCueBall : MonoBehaviour
{
    [SerializeField] float forceMultiplier = 10.0f;
    Vector2 forceApplicationPoint;

    // Start is called before the first frame update
    void Start()
    {
        forceApplicationPoint = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponent<SpriteRenderer>().color = Color.white;
        if (Input.GetMouseButtonDown(0))
        {
            forceApplicationPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mouseClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = forceApplicationPoint - mouseClickPosition;
            Vector2 normal = direction.normalized;
            float magnitude = direction.magnitude;
            Vector2 force = normal * magnitude * forceMultiplier;
            DynamicBody dynamicBody = GetComponent<DynamicBody>();
            dynamicBody.SetVelocity(force);
            dynamicBody.ApplyPointForce(forceApplicationPoint, force);
        }
    }
}
