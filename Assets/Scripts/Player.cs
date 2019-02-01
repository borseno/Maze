using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject
{
    private int _coins = 0;
    private Vector2 touchOrigin = -Vector2.one;

    public int Coins
    {
        get { return _coins; }
        set { if (value >= 0) _coins = value; }
    }

    static public string Name { get; set; }

    private void Update()
    {
        PlayerMove();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
        {
            Coins++;
            Destroy(collision.gameObject);
        }
    }
    private void PlayerMove()
    {
        if (currentCoroutine != null && currentCoroutine.Running)
            return;

#if UNITY_STANDALONE || UNITY_WEBPLAYER

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        if (x == 0f && y == 0f)
        {
            return;
        }

        //  x OR y must be 0, only 1 unit move
        if (x != 0)
        {
            x = x > 0 ? 1 : -1;
            y = 0;
        }
        else if (y != 0)
        {
            y = y > 0 ? 1 : -1;
            x = 0;
        }
        Debug.Log("x: " + x + " y: " + y);

#else
        int x = 0;
        int y = 0;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];

            if (touch.phase == TouchPhase.Began)
            {
                touchOrigin = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = touch.position;

                float horizontal = touchEnd.x - touchOrigin.x;
                float vertical = touchEnd.y - touchOrigin.y;

                if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
                {
                    x = horizontal > 0 ? 1 : -1;
                }
                else
                    y = vertical > 0 ? 1 : -1;

            }

        }
#endif
        AttemptMove<Enemy>(x, y);
    }

    protected override void OnCantMove<T>(T Component)
    {
        Enemy enemy = Component as Enemy;

        if (enemy != null)
            GameManager.instance.OnPlayerHasBeenHit(enemy);
    }
    protected override void Start()
    {
        base.Start();

        GameManager.instance.Player = this;
        GameManager.instance.SpawnPlayer();

        Coins = GameManager.instance.PlayerTotalCoins;
    }

    public void LoseCoins()
    {
        Coins = 0;
    }
}
