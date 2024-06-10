using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using Platformer.Mechanics;

public class Projectile : MonoBehaviour
{
    public Vector3 initialVel;
    public Vector4 typeColour;
    public float lifespan;
    public float projectileType; 
    //1 for Explosives, 2 for Propulsion, 3 for Teleport
    //4 and Beyond for Enemy Projectiles
    
    //For Player Teleport
    public PlayerController playerScript;
    private Rigidbody2D rb;
    private Vector3 tpVel;

    //For Combat
    public int damageValue;

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = initialVel;
        if (projectileType == 1)
        {
            typeColour = new Color(0.682353f, 0.1098039f, 0.1803922f, 1.0f);
            spriteRenderer.color = typeColour;
            lifespan = 7.0f;
        }
        else if (projectileType == 2)
        {
            typeColour = new Color(0.8745098f, 0.4078431f, 0.09019608f, 1.0f);
            spriteRenderer.color = typeColour;
            lifespan = 3.0f;
        }
        else if (projectileType == 3)
        {
            typeColour = new Color(0.2f, 0.2f, 0.8f, 1.0f);
            spriteRenderer.color = typeColour;
            lifespan = 10.0f;
            rb.mass = 5.0f;
        }
        else if (projectileType == 4)
        {
            typeColour = new Color(1.0f, 0.25f, 0.0f, 1.0f);
            spriteRenderer.color = typeColour;
            lifespan = 3.0f;
            //rb.mass = 5.0f;
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
        tpVel = rb.velocity;
    }

    private void AreaDamagePlayer(Vector3 playerPos, Vector3 projectilePos)
    {
        float dist = Vector3.Distance(playerPos, projectilePos);
        int modifier;
        if (dist < 0.5f)
        {
            modifier = 3;
        }
        else if (dist >= 0.5f && dist < 1.00f)
        {
            modifier = 2;
        }
        else if (dist >= 1.00f && dist < 1.50f)
        {
            modifier = 1;
        }
        else
        {
            modifier = 0;
        }
        playerScript.RegisterSelfDamage(modifier, damageValue);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<TilemapCollider2D>() != null)
        {
            //Call Projectile Function
            if (projectileType == 1)
            {
                AreaDamagePlayer(playerScript.transform.position, transform.position);
            }
            else if (projectileType == 2)
            {
                playerScript.RocketJump(transform.position);
            }
            else if (projectileType == 3)
            {
                playerScript.GameTeleport(tpVel, transform.position);
            }
            else if (projectileType == 4)
            {

            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.tag == "Player")
        {
            if (projectileType == 1)
            {

            }
            else if (projectileType == 2)
            {

            }
            else if (projectileType == 4)
            {
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.tag == "Projectile")
        {
            Destroy(gameObject);
        }
    }

}
