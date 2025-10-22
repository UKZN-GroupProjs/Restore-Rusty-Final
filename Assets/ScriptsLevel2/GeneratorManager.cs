using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneratorManager : MonoBehaviour
{
    public static GeneratorManager Instance;
    [SerializeField] private Light mainLight;
    [SerializeField] private int totalGenerators;
    private int activatedGenerators = 0;

    // Material change references
    [SerializeField] private Material activeMaterial;
    [SerializeField] private int materialIndex = 0; // Which material slot to change
    [SerializeField] private float shaderTransitionDuration = 1f;

    // Dark settings
    private float darkLightIntensity = 0.05f;
    [SerializeField] private float darkFogDensity;
    private Color darkFogColor = new Color(0.05f, 0.05f, 0.05f);

    // Bright settings
    private float brightLightIntensity = 1.2f;
    private float brightFogDensity = 0f;
    private float transitionDuration = 2f;

    [Header("Game objects that change when lights on")]
    public Animator ForkLiftAnim;
    public GameObject RustyTorch;
    public AudioClip bedroomMusic_gen_on;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = darkFogColor;
        RenderSettings.fogDensity = darkFogDensity;

        if (mainLight != null)
            mainLight.intensity = darkLightIntensity;

        RustyTorch.SetActive(true);
    }

    public void GeneratorActivated(Renderer generatorRenderer)
    {
        activatedGenerators++;

        // Change shader for this specific generator with transition
        StartCoroutine(TransitionGeneratorShader(generatorRenderer));

        if (activatedGenerators >= totalGenerators)
        {
            StartCoroutine(TransitionLighting());
        }
    }

    private IEnumerator TransitionGeneratorShader(Renderer renderer)
    {
        if (activeMaterial == null || renderer == null)
            yield break;

        Material[] materials = renderer.materials;

        if (materialIndex >= materials.Length)
            yield break;

        // Store the original material
        Material originalMat = materials[materialIndex];

        // Create an instance of the new material
        Material newMat = new Material(activeMaterial);
        materials[materialIndex] = newMat;
        renderer.materials = materials;

        // Transition transparency if the shader has it
        if (newMat.HasProperty("_Transparency"))
        {
            float elapsed = 0f;

            while (elapsed < shaderTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / shaderTransitionDuration);

                // Fade in the new material
                newMat.SetFloat("_Transparency", Mathf.Lerp(0f, 0.5f, t));

                yield return null;
            }

            newMat.SetFloat("_Transparency", 0.5f);
        }
    }

    private IEnumerator TransitionLighting()
    {
        float elapsed = 0f;
        float startIntensity = mainLight != null ? mainLight.intensity : 0f;
        float startFog = RenderSettings.fogDensity;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            if (mainLight != null)
                mainLight.intensity = Mathf.Lerp(startIntensity, brightLightIntensity, t);

            RenderSettings.fogDensity = Mathf.Lerp(startFog, brightFogDensity, t);

            ForkLiftAnim?.SetTrigger("GenOn");
            RustyTorch?.SetActive(false);

            BackgroundSoundManager.Instance?.CrossfadeTo(bedroomMusic_gen_on);

            yield return null;
        }

        if (mainLight != null)
            mainLight.intensity = brightLightIntensity;

        RenderSettings.fogDensity = brightFogDensity;
    }
}
