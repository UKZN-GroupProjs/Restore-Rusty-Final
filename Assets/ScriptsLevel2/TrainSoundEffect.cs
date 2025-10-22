using UnityEngine;

public class TrainSoundControl : MonoBehaviour
{
    public Transform listener; // usually the Main Camera
    public AudioSource trainAudio;
    public float maxDistance = 50f;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, listener.position);
        float volume = Mathf.Clamp01(1 - (distance / maxDistance));
        trainAudio.volume = volume;
    }
}
