using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Platformer.Mechanics.PlayerController;

public class PlayerControl : MonoBehaviour
{
    public AudioClip jumpAudio;
    public AudioClip respawnAudio;
    public AudioClip ouchAudio;

    // Max horizontal speed of the player.
    public float maxSpeed = 4.0f;
    public float crouchSpeed = 1.0f;
    public float magBootsSpeed = 1.50f;
    public float baseSpeed = 2.0f;
    private float playerSpeed;

    public bool playerCrouch;
    // Initial jump velocity at the start of a jump.
    public float jumpTakeOffSpeed = 7;

    public JumpState jumpState = JumpState.Grounded;
    private bool stopJump;
    /*internal new*/
    public Collider2D collider2d;
    /*internal new*/
    public AudioSource audioSource;
    public Health health;
    public bool controlEnabled = true;

    bool jump;
    Vector2 move;
    SpriteRenderer spriteRenderer;
    internal Animator animator;
    readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

    public Bounds Bounds => collider2d.bounds;

    //Mouse UI
    public GameObject Cursor;
    private float cursorLimit = 2.0f;

    //Projectile Prefab
    public GameObject ProjectilePrefab;

    //Fire Rate Timer
    public float fireRate_ex = 3.0f, fireRate_pr = 1.0f, fireRate_tp = 0.5f, fireTimer1 = 0, fireTimer2 = 0, fireTimer3 = 0;

    //Magnetic Boots
    public bool magBoots;

    //Trap Variables
    public bool InAcid;

    private void Awake()
    {
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();
        collider2d = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerSpeed = baseSpeed;

        playerCrouch = false;
        magBoots = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.transform.position = new Vector2(1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
