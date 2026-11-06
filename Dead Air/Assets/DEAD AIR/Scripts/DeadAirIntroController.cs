
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeadAirIntroController : MonoBehaviour
{
    [Header("Intro UI")]
    public CanvasGroup introPanel;
    public CanvasGroup mainMenuPanel;
    public TextMeshProUGUI introText;
    public Button skipButton;

    [Header("Background Music")]
    public AudioSource introMusic;
    public AudioSource menuMusic;

    [Header("Intro Sound Effects")]
    public AudioSource introRadioStatic;
    public AudioClip textGlitch;
    public AudioClip titleImpact;

    [Header("Audio Volumes")]
    [Range(0f, 1f)] public float glitchVolume = 0.5f;
    [Range(0f, 1f)] public float impactVolume = 0.85f;
    [Range(0f, 1f)] public float menuMusicVolume = 0.35f;

    [Header("Visual Effects")]
    public Image redFlashOverlay;

    [Range(0f, 1f)]
    public float glitchFlashStrength = 0.18f;

    [Range(0f, 1f)]
    public float titleFlashStrength = 0.4f;

    public float flashDuration = 0.3f;

    private AudioSource effectsSource;
    private Coroutine introRoutine;
    private Coroutine flashRoutine;

    private bool transitioning = false;
    private Color originalTextColor;

    void Start()
    {
        if (introPanel == null || mainMenuPanel == null ||
            introText == null || skipButton == null)
        {
            Debug.LogError("DEAD AIR: Missing intro UI references!");
            return;
        }

        effectsSource = GetComponent<AudioSource>();
        originalTextColor = introText.color;

        introPanel.gameObject.SetActive(true);
        mainMenuPanel.gameObject.SetActive(true);

        introPanel.alpha = 1;
        introPanel.interactable = true;
        introPanel.blocksRaycasts = true;

        mainMenuPanel.alpha = 1;
        mainMenuPanel.interactable = false;
        mainMenuPanel.blocksRaycasts = false;

        // Hide the flash effect when the scene starts.
        SetFlashAlpha(0f);

        skipButton.onClick.AddListener(SkipIntro);

        if (menuMusic != null)
        {
            menuMusic.Stop();
            menuMusic.loop = true;
        }

        if (introMusic != null && introMusic.clip != null)
        {
            introMusic.Play();
        }

        if (introRadioStatic != null &&
            introRadioStatic.clip != null)
        {
            introRadioStatic.Stop();
            introRadioStatic.Play();
        }

        introRoutine = StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        yield return AnimateText(
            "INITIALIZING TRANSMISSION...",
            2f, false, null);

        yield return AnimateText(
            "THE SIGNAL IS STILL ACTIVE.",
            2f, false, textGlitch);

        yield return AnimateText(
            "SOMETHING IS LISTENING.",
            2f, true, textGlitch);

        yield return AnimateText(
            "DEAD AIR",
            3f, false, titleImpact);

        introRoutine = null;
        BeginTransition();
    }

    IEnumerator AnimateText(
        string message,
        float duration,
        bool redText,
        AudioClip effect)
    {
        introText.text = message;

        // Play the sound effect.
        if (effectsSource != null && effect != null)
        {
            float volume = effect == titleImpact
                ? impactVolume
                : glitchVolume;

            effectsSource.PlayOneShot(effect, volume);
        }

        // Trigger the visual flash.
        if (effect != null && redFlashOverlay != null)
        {
            float strength = effect == titleImpact
                ? titleFlashStrength
                : glitchFlashStrength;

            TriggerRedFlash(strength);
        }

        // Choose text color.
        Color color = redText
            ? new Color(0.85f, 0.2f, 0.25f, 1f)
            : originalTextColor;

        float timer = 0f;

        while (timer < duration)
        {
            float fadeIn = Mathf.Clamp01(timer / 0.2f);

            float fadeOut = Mathf.Clamp01(
                (duration - timer) / 0.3f);

            introText.color = new Color(
                color.r,
                color.g,
                color.b,
                color.a * fadeIn * fadeOut);

            // Text appears slightly enlarged,
            // then settles into its normal size.
            float scale = Mathf.Lerp(
                1.12f,
                1f,
                Mathf.Clamp01(timer / 0.4f));

            introText.rectTransform.localScale =
                Vector3.one * scale;

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        introText.color = new Color(
            color.r, color.g, color.b, 0f);
    }

    // RED FLASH EFFECT

    void TriggerRedFlash(float strength)
    {
        if (redFlashOverlay == null)
            return;

        // Prevent overlapping flashes.
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        flashRoutine = StartCoroutine(
            FlashRed(strength));
    }

    IEnumerator FlashRed(float strength)
    {
        // Show a quick red flash.
        SetFlashAlpha(strength);

        yield return new WaitForSecondsRealtime(0.07f);

        // Smoothly fade the red flash away.
        float timer = 0f;
        float duration = Mathf.Max(0.01f, flashDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                timer / duration);

            float alpha = Mathf.Lerp(
                strength, 0f, progress);

            SetFlashAlpha(alpha);

            yield return null;
        }

        SetFlashAlpha(0f);
        flashRoutine = null;
    }

    void SetFlashAlpha(float alpha)
    {
        if (redFlashOverlay == null)
            return;

        Color color = redFlashOverlay.color;
        color.a = alpha;
        redFlashOverlay.color = color;
    }

    // SKIP INTRO

    public void SkipIntro()
    {
        BeginTransition();
    }

    void BeginTransition()
    {
        if (transitioning)
            return;

        transitioning = true;

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        // Stop any active red flash.
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        SetFlashAlpha(0f);

        if (introRadioStatic != null)
            introRadioStatic.Stop();

        if (effectsSource != null)
            effectsSource.Stop();

        StartCoroutine(FadeToMenu());
    }

    // TRANSITION TO MAIN MENU

    IEnumerator FadeToMenu()
    {
        float duration = 0.75f;
        float timer = 0f;

        float introVolume = introMusic != null
            ? introMusic.volume
            : 0f;

        bool hasMenuMusic = menuMusic != null &&
                            menuMusic.clip != null;

        if (hasMenuMusic)
        {
            menuMusic.volume = 0f;
            menuMusic.Play();
        }

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / duration);

            introPanel.alpha = 1f - t;

            if (introMusic != null)
            {
                introMusic.volume =
                    introVolume * (1f - t);
            }

            if (hasMenuMusic)
            {
                menuMusic.volume =
                    menuMusicVolume * t;
            }

            yield return null;
        }

        if (introMusic != null)
            introMusic.Stop();

        introPanel.alpha = 0f;
        introPanel.interactable = false;
        introPanel.blocksRaycasts = false;
        introPanel.gameObject.SetActive(false);

        mainMenuPanel.interactable = true;
        mainMenuPanel.blocksRaycasts = true;
    }
}
