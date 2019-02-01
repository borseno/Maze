using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MovingObject  // Updated in GameManager
{
    public bool HasDirection { get; private set; }

    private Animator animator;
    private Queue<Vector2> RemainingPath { get; set; }

    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this);

        animator = GetComponent<Animator>();

        base.Start();
    }
    protected override void AttemptMove<t>(float xDir, float yDir)
    {
        base.AttemptMove<t>(xDir, yDir);
    }
    protected override void OnCantMove<T>(T component)
    {
        Player hitPlayer = component as Player;

        if (hitPlayer != null)
        {
            GameManager.instance.OnPlayerHasBeenHit(this);
        }
    }

    public void MoveEnemy(Vector2[] direction)
    {
        if (direction == null || direction.Length == 0)
            return;

        HasDirection = true;

        RemainingPath = new Queue<Vector2>(direction);

        Vector2 finalPoint = RemainingPath.Dequeue();

        Vector2 addedCoords = (finalPoint - (Vector2)this.transform.position);

        AttemptMove<Player>(addedCoords.x, addedCoords.y);
    }
    public void MoveEnemy()
    {
        if (currentCoroutine.Running)
            return;
        if (RemainingPath.Count == 0)
        {
            HasDirection = false;
            return;
        }
        Vector2 finalPoint = RemainingPath.Dequeue();

        Vector2 addedCoords = (finalPoint - (Vector2)this.transform.position);

        AttemptMove<Player>(addedCoords.x, addedCoords.y);
    }
    public void SetTrigger(string TriggerName)
    {
        animator.SetTrigger(TriggerName);
    }
}
