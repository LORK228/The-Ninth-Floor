using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float timeBetweenSteps = 0.5f;

    private AudioSource audioSource;
    private FirstPersonController fpc;
    private bool isPlayingFootsteps = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        fpc = GetComponent<FirstPersonController>();
    }

    void Update()
    {
        bool isMoving = fpc != null && fpc.playerCanMove && (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f);

        if (isMoving && !isPlayingFootsteps)
        {
            StartCoroutine(PlayFootsteps());
        }
    }

    private IEnumerator PlayFootsteps()
    {
        isPlayingFootsteps = true;

        while (fpc != null && fpc.playerCanMove && (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f))
        {
            if (footstepClips.Length > 0)
            {
                AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
                audioSource.PlayOneShot(clip);
            }
            yield return new WaitForSeconds(timeBetweenSteps);
        }

        isPlayingFootsteps = false;
    }
}