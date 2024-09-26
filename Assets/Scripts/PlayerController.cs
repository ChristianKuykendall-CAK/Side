using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public float H;
    public Vector2 facingDirection = Vector2.right;
    public float moveForce;

    private SpriteRenderer rend;
    private Rigidbody2D rbody;
    private Animator anim;
    private float delay = .8f;
    private bool isJumping = false;
    private bool Falling = false; // Helps toggle platform

    private TilemapCollider2D tilemapCollider;

    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        rbody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        // Set up change tilemap collider to turn into trigger so player can drop through
        GameObject tilemapObject = GameObject.Find("Tilemap_Dropdown_Platform");
        if (tilemapObject != null) 
        {
            tilemapCollider = tilemapObject.GetComponent<TilemapCollider2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        H = Input.GetAxis("Horizontal");
        
        if (H < 0 && facingDirection == Vector2.right)
        {
            FlipX();
            facingDirection = Vector2.left;
        }
        else if (H > 0 && facingDirection == Vector2.left)
        {
            FlipX();
            facingDirection = Vector2.right;
        }
        //if (Input.GetKey(KeyCode.W) && !isJumping)
        //{
          //  StartCoroutine(JumpPeriod());
        //}
    }

    private void FixedUpdate()
    {
        LayerMask PlayerMask = LayerMask.GetMask("playerLayer"); // player has their own layer so raycast can ignore it

        if (rbody.velocity.x < 10 && rbody.velocity.x > -10)
            rbody.AddForce(Vector2.right * Mathf.Round(H) * moveForce);

        Vector2 raycastStart = new Vector2(transform.position.x, transform.position.y - 1f);
        RaycastHit2D hit = Physics2D.Raycast(raycastStart, Vector2.down, .1f, ~PlayerMask);

        //Debug.DrawRay(raycastStart, Vector2.down, Color.red);
        //Debug.Log(hit.collider);

        if (hit.collider != null && hit.collider.CompareTag("Platform") && !Falling)
        {
            tilemapCollider.isTrigger = false;
            if (Input.GetKey(KeyCode.S))
                StartCoroutine(FallThrough());
        }
        else
        {
            tilemapCollider.isTrigger = true;
        }

        //supposed to call the jump anim but only does it once...
        if (hit.collider != null && hit.collider.CompareTag("Ground"))
            anim.SetBool("isJumping", false);
        else if(Input.GetKey(KeyCode.W) && !isJumping)
        {
            anim.SetBool("isJumping", true);
            StartCoroutine(JumpPeriod());
        }
    }
    // Forces player to jump once
    IEnumerator JumpPeriod()
    {
        isJumping = true;

        rbody.AddForce(Vector2.up * (moveForce / 2), ForceMode2D.Impulse);

        yield return new WaitForSeconds(delay);

        isJumping = false;
    }
    // Keeps platform turned off long enough for player to fall through
    IEnumerator FallThrough()
    {
        Falling = true;
        tilemapCollider.isTrigger = true;
        yield return new WaitForSeconds(1);
        Falling = false;

    }
    void FlipX()
    {
        Vector3 theScale = transform.localScale;
        theScale.x = theScale.x * -1;
        transform.localScale = theScale;
    }

    //collision with item pickups
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ammo"))
        {
            GameManager.instance.ammo += 2;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Health"))
        {
            GameManager.instance.health += 50;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Coin"))
        {
            GameManager.instance.coins += 1;
            Destroy(collision.gameObject);
        }
    }
}
