using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class ProjectileEnemy : MonoBehaviour
{
    public Vector3 initialVel;
    public Vector4 typeColour;
    public float lifespan;
    private Rigidbody2D rb;
    public float projectileType; //To add in different enemy projectiles


    public PlayerController playerScript;


    public UnityEvent projectileDestroy;
    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = initialVel;
        if (projectileType == 1)
        {
            typeColour = new Color(1.0f, 0.2f, 0.1f, 1.0f);
            spriteRenderer.color = typeColour;
            lifespan = 10.0f;
            rb.mass = 5.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        lifespan -= Time.deltaTime;
        if (lifespan <= 0)
        {
            Destroy(gameObject);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TilemapCollider2D>() != null)
        {
            //Call Projectile Function
            if (projectileType == 1)
            {

            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Player")
        {
            if (projectileType == 1)
            {
                //playerScript.health.Decrement();
            }
            Destroy(gameObject);
        }
    }
}
