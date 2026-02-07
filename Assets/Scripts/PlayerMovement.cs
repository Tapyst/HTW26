using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    // public Camera targetCamera;
    // private float minX;
    // private float maxX;
    // private float minY;
    // private float maxY;
    
    // [Header("Movement Settings")]
    // public float moveSpeed = 5f;
    // public bool clampToBounds = true;
    
    // private Vector2 currentPosition;
    // private Rigidbody2D rb;
    // private void Start()
    // {
    //     rb = GetComponent<Rigidbody2D>();
    //     if (rb == null)
    //     {
    //         Debug.LogWarning("Rigidbody2D not found on PlayerMovement object!");
    //     }
        
    //     if (targetCamera == null)
    //     {
    //         targetCamera = Camera.main;
    //     }
        
    //     CalculateBounds();
    //     currentPosition = transform.position;
    // }
    
    // private void Update()
    // {
    //     HandleInput();
    // }
    
    // private void FixedUpdate()
    // {
    //     ApplyMovement();
    // }
    // private void HandleInput()
    // {
    //     Vector2 moveDirection = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
    //     currentPosition += moveDirection * moveSpeed * Time.deltaTime;
        
    //     // Clamp position to camera bounds if enabled
    //     if (clampToBounds)
    //     {
    //         currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
    //         currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);
    //     }
    // }
    // private void ApplyMovement()
    // {
    //     if (rb != null)
    //     {
    //         rb.linearVelocity = Vector2.zero;
    //         transform.position = currentPosition;
    //     }
    //     else
    //     {
    //         transform.position = currentPosition;
    //     }
    // }
    // public void SetPosition(Vector2 newPosition)
    // {
    //     currentPosition = newPosition;
    //     transform.position = currentPosition;
    // }

    // public void CalculateBounds()
    // {
    //     if (targetCamera == null)
    //     {
    //         Debug.LogError("Camera reference is null!");
    //         return;
    //     }
    //     // Get the camera's viewport corners in world space
    //     Vector3 bottomLeft = targetCamera.ViewportToWorldPoint(new Vector3(0, 0, targetCamera.nearClipPlane));
    //     Vector3 topRight = targetCamera.ViewportToWorldPoint(new Vector3(1, 1, targetCamera.nearClipPlane));

    //     minX = bottomLeft.x;
    //     maxX = topRight.x;
    //     minY = bottomLeft.y;
    //     maxY = topRight.y;
    // }
    // public float GetMinX() => minX;
    // public float GetMaxX() => maxX;
    // public float GetMinY() => minY;
    // public float GetMaxY() => maxY;
    // public bool IsPositionInBounds(Vector3 position)
    // {
    //     return position.x >= minX && position.x <= maxX &&
    //            position.y >= minY && position.y <= maxY;
    // }
    // public Rect GetBoundsAsRect()
    // {
    //     return new Rect(minX, minY, maxX - minX, maxY - minY);
    // }
}