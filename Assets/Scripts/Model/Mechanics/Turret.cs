using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public PlayerController playerObject;
    public float detectionDistance, fireRate, rotationModifier, rotationLimit1, rotationLimit2;
    public GameObject turretProjectile;
    public int projectileType, turretDmg, turretHP;

    private float fireCooldown;
    private bool turretFire;

    // Start is called before the first frame update
    void Start()
    {
        fireCooldown = 0;
        turretFire = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (turretHP <= 0)
        {
            Destroy(gameObject);
        }

        float dist = Vector3.Distance(transform.position, playerObject.transform.position);

        if (dist <= detectionDistance) 
        {
            Vector3 vectorToTarget = transform.position - playerObject.transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - rotationModifier;
            if (angle >= rotationLimit1)
            {
                turretFire = false;
                angle = rotationLimit1;
            }
            else if (angle <= rotationLimit2)
            {
                turretFire = false;
                angle = rotationLimit2;
            }
            else
            {
                turretFire = true;
            }
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            Transform turretHead = transform.GetChild(0);
            turretHead.rotation = q;

            if (turretFire)
            {
                if (fireCooldown <= 0)
                {
                    Transform turretFirePoint = turretHead.transform.GetChild(0);
                    var projectilePos = turretFirePoint.position;
                    GameObject gb = Instantiate(turretProjectile, projectilePos, Quaternion.identity);
                    Projectile projectile = gb.GetComponent<Projectile>();

                    //Projectile Type
                    projectile.projectileType = projectileType;

                    //Damage Value
                    projectile.damageValue = turretDmg;

                    //Projectile Speed
                    float bulletSpeed = 10.0f;
                    var projVel = Vector3.zero;
                    float xVel = bulletSpeed / dist * (playerObject.transform.position.x - projectilePos.x);
                    projVel.x = xVel;
                    float yVel = bulletSpeed / dist * (playerObject.transform.position.y - projectilePos.y);
                    projVel.y = yVel;
                    projectile.initialVel = projVel;

                    fireCooldown = fireRate;
                }
            }
        }
        fireCooldown -= Time.deltaTime;
    }
    //Damage
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Projectile>() != null)
        {
            Projectile collidedProjectile = collision.gameObject.GetComponent<Projectile>();
            turretHP -= collidedProjectile.damageValue;
            Destroy(collision.gameObject);
        }
    }
    
}
