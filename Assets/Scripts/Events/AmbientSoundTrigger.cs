using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class AmbientSoundTrigger : MonoBehaviour
{
    [Tooltip("Звук, который будет проигрываться внутри триггера")]
    [SerializeField] private AudioClip ambientSound;
    [Tooltip("Громкость звука")]
    [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.clip = ambientSound;
        audioSource.volume = volume;

        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}