using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Grab many SkinnedMeshRenderers
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material hologramMaterial;

    [SerializeField] private ParticleSystem starsEffect; 
    [SerializeField] private float starsDuration = 2f;

    private void Start()
    {
        PlayerStats.Instance.onDamageTaken += HandleDamage;
        PlayerStats.Instance.onDeath += HandleDeath;
    }

    private void HandleDamage()
    {
        if (animator != null)
            animator.SetTrigger("TakeDamage");

        if (starsEffect != null)
        {
            starsEffect.Play();
            Invoke(nameof(StopStars), starsDuration);
        }
        SoundManager.Instance.PlaySFX(SoundManager.Instance.hitTakenClip);
    }

    private void StopStars()
    {
        if (starsEffect != null)
            starsEffect.Stop();
    }

    private void HandleDeath()
    {
        if (animator != null)
            animator.SetTrigger("Death");
        SoundManager.Instance.PlaySFX(SoundManager.Instance.deathClip);
        // Apply hologram material to all meshes
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null && hologramMaterial != null)
                renderer.material = hologramMaterial;
        }
    }

    // Call this when respawning/resetting
    public void ResetAppearance()
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null && defaultMaterial != null)
                renderer.material = defaultMaterial;
        }
    }
}





