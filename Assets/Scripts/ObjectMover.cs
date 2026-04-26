using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] GameObject objectToMove;
    [SerializeField] Vector3 moveOffset;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] bool permanentMove = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        if (objectToMove != null)
        {
            startPosition = objectToMove.transform.position;
            targetPosition = startPosition + moveOffset;
        }
    }

    void Update()
    {
        if (objectToMove == null) return;

        Vector3 destination = isMoving ? targetPosition : startPosition;
        objectToMove.transform.position = Vector3.MoveTowards(
            objectToMove.transform.position, 
            destination, 
            moveSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        // Now checks for Kickable objects as well
        if (other.CompareTag("Player") || other.CompareTag("Movable") || other.CompareTag("Kickable"))
        {
            isMoving = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Now checks for Kickable objects as well
        if ((other.CompareTag("Player") || other.CompareTag("Movable") || other.CompareTag("Kickable")) && !permanentMove)
        {
            isMoving = false;
        }
    }
}