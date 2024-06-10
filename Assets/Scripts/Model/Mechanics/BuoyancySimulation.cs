using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class BuoyancySimulation : MonoBehaviour
{
    public float density = 997.0f; // KG/M^3
    public float stickiness = 0.15f;
    Vector3 minPoint = Vector3.zero;
    Vector3 maxPoint = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        minPoint = collider.bounds.min;
        maxPoint = collider.bounds.max;
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        RegisterForceOn(collider);
    }
    void OnTriggerStay2D(Collider2D collider)
    {
        RegisterForceOn(collider);
    }
    void RegisterForceOn(Collider2D collider)
    {
        PlayerController buoyancy = collider.gameObject.GetComponent<PlayerController>();
        SpriteRenderer renderer = collider.gameObject.GetComponent<SpriteRenderer>();
        if (renderer == null || buoyancy == null)
        {
            return;
        }
        Bounds bounds = renderer.bounds;
        Vector2 castPoint = new Vector2(bounds.center.x, bounds.max.y);
        RaycastHit2D cast;
        int layerMask = 1 << LayerMask.NameToLayer("Water");
        cast = Physics2D.Raycast(castPoint, -Vector2.up, 1.0f, layerMask);
        if (cast.collider == null || cast.collider.gameObject != this.gameObject)
        {
            return;
        }
        //Object is above water surface
        float volumeSubmerged = 0.0f;
        float totalVolume = bounds.size.x * bounds.size.y;

        float width = bounds.size.x;
        if (castPoint.y > maxPoint.y)
        {
            float height = Mathf.Abs(cast.point.y - bounds.min.y);
            volumeSubmerged = width * height;
        }
        else
        {
            float height = bounds.size.y;
            volumeSubmerged = width * height;
        }

        float buoyantForce = (density * 9.8f * volumeSubmerged);

        float sign = Mathf.Sign(buoyancy.velocity.y) * -1.0f;
        float drag = 0.5f * (density) * (buoyancy.velocity.y * buoyancy.velocity.y) * 0.47f * volumeSubmerged * sign;
        float arrest = stickiness * (1.0f - volumeSubmerged / totalVolume) * (1.0f/750.0f) * -(buoyancy.velocity.y) / Time.fixedDeltaTime;
        //This aims to keep object in liquid
        buoyancy.RegisterForce(new Vector3(0, buoyantForce + drag + arrest, 0));
    }
}
