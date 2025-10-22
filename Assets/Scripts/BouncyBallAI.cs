using UnityEngine;
using System.Collections;
public class BouncyBallAI : MonoBehaviour
{
    public enum BallState { Idle, Alert }

    [Header("Behaviour Settings")]
    public float detectionRange = 3f;
    public float bounceForce = 4f;
    public float idleBounceInterval = 2f;
    public float alertBounceInterval = 0.8f;
    public float maxWanderRadius = 2f; // how far from spawn it can bounce

    [Header("Links")]
    public BouncyBallAI[] otherBalls;
    public Transform player;

    private Rigidbody rb;
    private Vector3 homePosition;      // where the ball started
    private BallState state = BallState.Idle;
    private float bounceTimer = 0f;
    private bool canDealDamage = true;
    public float damageCooldown = 5f; // seconds before it can damage again

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        homePosition = transform.position;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // make ball physically bouncy
        var mat = new PhysicsMaterial
        {
            bounciness = 0.9f,
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };
        GetComponent<Collider>().material = mat;

        rb.useGravity = true;
        rb.linearDamping = 0.2f;
        rb.angularDamping = 0.05f;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bounceTimer += Time.deltaTime;

        switch (state)
        {
            case BallState.Idle:
                if (distance < detectionRange)
                {
                    state = BallState.Alert;
                    AlertOthers();
                }
                else
                {
                    HandleIdleBounce(idleBounceInterval);
                }
                break;

            case BallState.Alert:
                if (distance > detectionRange * 1.2f) 
                {
                    state = BallState.Idle;
                }
                else
                {
                    HandleIdleBounce(alertBounceInterval, towardPlayer: true);
                }
                break;
        }

        KeepNearHome();
    }

    // Behaviour

    void HandleIdleBounce(float interval, bool towardPlayer = false)
    {
        if (bounceTimer >= interval)
        {
            bounceTimer = 0f;

            Vector3 dir;

            if (towardPlayer)
            {
                // bounce upward and slightly toward player
                dir = (player.position - transform.position).normalized + Vector3.up * 0.8f;
            }
            else
            {
                // random local bounce direction
                dir = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(-1f, 1f)
                );
            }

            rb.AddForce(dir.normalized * bounceForce, ForceMode.Impulse);
        }
    }

    void KeepNearHome()
    {
        Vector3 offset = transform.position - homePosition;
        if (offset.magnitude > maxWanderRadius)
        {
            // gently pull back toward home area
            rb.AddForce(-offset.normalized * bounceForce * 0.5f, ForceMode.Force);
        }
    }

    void AlertOthers()
    {
        foreach (var ball in otherBalls)
        {
            if (ball != null && ball.state == BallState.Idle)
                ball.state = BallState.Alert;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!canDealDamage) return;

        if (collision.gameObject.CompareTag("Player"))
        {
           PlayerStats.Instance.TakeDamage(1);
           StartCoroutine(DamageCooldownRoutine());
           //bounce away from player on hit
           rb.AddForce(-collision.contacts[0].normal * bounceForce, ForceMode.Impulse);
        }
    }

    private IEnumerator DamageCooldownRoutine()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDealDamage = true;
    }
}


