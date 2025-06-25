using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]          // ← forces Unity to add one
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }            // ★ NEW

    [Tooltip("0 = intro, 1 = elder, 2 = merchant …")]
    [SerializeField] private AudioClip[] tracks;

    [SerializeField] private float fadeDuration = 2f;

    private AudioSource source;
    private Coroutine fadeRoutine;

    void Awake()
    {
        // ------------- singleton boiler-plate -------------
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);                                     // optional
        // ---------------------------------------------------

        source = GetComponent<AudioSource>();
        if (tracks.Length > 0) PlayTrack(0);                               // intro
    }

    public void PlayTrack(int index, bool loop = true)                     // ★ loop optional
    {
        if (index < 0 || index >= tracks.Length) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToClip(tracks[index], loop));
    }

    private IEnumerator FadeToClip(AudioClip next, bool loop)
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {         // out-fade
            source.volume = 1f - t / fadeDuration;
            yield return null;
        }

        source.clip = next;
        source.loop = loop;                                                // ★
        source.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {         // in-fade
            source.volume = t / fadeDuration;
            yield return null;
        }
        source.volume = 1f;
    }
}
