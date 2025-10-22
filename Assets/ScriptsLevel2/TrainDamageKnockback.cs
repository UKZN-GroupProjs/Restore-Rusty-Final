// TrainDamageKnockback.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrainDamageKnockback : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Only hit objects with this tag.")]
    public string targetTag = "Player";

    [Header("Damage & Knockback")]
    [Tooltip("Passed to PlayerStats.Instance.TakeDamage(value).")]
    public float damage = 0.2f;
    [Tooltip("Horizontal knockback force passed to PlayerMovementFreeLook.ApplyKnockback.")]
    public float knockbackForce = 10f;

    [Header("Hit Control")]
    [Tooltip("Cooldown so the same player isn't hit every frame.")]
    public float hitCooldown = 0.4f;

    // per-target cooldown memory
    private readonly Dictionary<int, float> lastHitTime = new Dictionary<int, float>();

    void Reset()
    {
        // For moving trains, a non-trigger MeshCollider is typical.
        // If using a trigger collider, OnTriggerEnter will handle it.
        var col = GetComponent<Collider>();
        if (col != null && col is MeshCollider meshCol)
        {
            if (!meshCol.convex && GetComponent<Rigidbody>() != null)
            {
                // Non-convex MeshCollider cannot be used with Rigidbody; usually trains are kinematic or animated without RB.
                GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    void OnCollisionEnter(Collision c)
    {
        TryHit(c.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        // Tag gate
        if (!string.IsNullOrEmpty(targetTag) &&
            !other.CompareTag(targetTag) &&
            !(other.attachedRigidbody && other.attachedRigidbody.CompareTag(targetTag)) &&
            !(other.transform.root && other.transform.root.CompareTag(targetTag)))
        {
            return;
        }

        // Get player controller
        var playerRoot = other.GetComponentInParent<PlayerMovementFreeLook>();
        if (playerRoot == null) return;

        int id = playerRoot.GetInstanceID();
        if (lastHitTime.TryGetValue(id, out float t) && Time.time - t < hitCooldown)
            return;

        lastHitTime[id] = Time.time;

        // Damage (same logic you had)
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.TakeDamage(damage);
        }

        // Knockback: from train to player (horizontal only)
        Vector3 dir = (playerRoot.transform.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward; // fallback if overlapping

        playerRoot.ApplyKnockback(dir.normalized, knockbackForce);
    }
}
