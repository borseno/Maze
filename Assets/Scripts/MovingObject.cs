using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    protected Task currentCoroutine;

    private BoxCollider2D boxCollider2D;
    private new Rigidbody2D rigidbody2D;

	protected virtual void Start () {
        rigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
	}
    protected bool Move(float xDir, float yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;

        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider2D.enabled = false;

        hit = Physics2D.Linecast(start, end, blockingLayer);

        boxCollider2D.enabled = true;

        if (hit.transform == null)
        {
            Vector3 scale = 
                new Vector3(
                    xDir != 0 ? xDir * -1 : transform.localScale.x, 
                    transform.localScale.y
                    );
 
            transform.localScale = scale;

            currentCoroutine = new Task(SmoothMovement(end));

            //StartCoroutine(SmoothMovement(end));
            return true;
        }
        else
            return false;
    }
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        if (this.GetType().Name == "Player")
            Debug.Log(end);

        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rigidbody2D.position, end, moveTime * Time.deltaTime);

            rigidbody2D.MovePosition(newPosition);

            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            yield return null;
        }
    }
    protected virtual void AttemptMove<t>(float xDir, float yDir) 
        where t : Component
    {
        RaycastHit2D hit;

        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;

        t hitComponent = hit.transform.GetComponent<t>();

        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);
    }
    protected abstract void OnCantMove<T>(T Component) where T : Component;
}
