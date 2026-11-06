
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeadAirSettingsMenu : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button backButton;

    [Header("Volume Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Animation")]
    public float fadeDuration = 0.3f;

    private CanvasGroup settingsGroup;
    private Coroutine fadeRoutine;

    private AudioSource introMusic;
    private AudioSource menuMusic;
    private AudioSource buttonAudio;
    private AudioSource clickAudio;
    private AudioSource radioStatic;
    private AudioSource introEffects;

    private DeadAirIntroController introController;

    private float introBase;
    private float menuBase;
    private float hoverBase;
    private float clickBase;
    private float staticBase;
    private float effectsBase;

    void Awake()
    {
        if (settingsPanel == null ||
            settingsButton == null ||
            backButton == null ||
            masterVolumeSlider == null ||
            musicVolumeSlider == null ||
            sfxVolumeSlider == null)
        {
            Debug.LogError(
                "DEAD AIR: Settings references missing!");
            enabled = false;
            return;
        }

        // Find the existing audio objects automatically.
        introMusic = FindAudio("IntroMusic");
        menuMusic = FindAudio("MenuMusic");
        buttonAudio = FindAudio("ButtonAudio");
        clickAudio = FindAudio("ClickAudio");
        radioStatic = FindAudio("IntroRadioStatic");
        introEffects = FindAudio("IntroDirector");

        introController =
            FindObjectOfType<DeadAirIntroController>();

        // Remember the original volume levels.
        introBase = GetVolume(introMusic);
        menuBase = introController != null
            ? introController.menuMusicVolume
            : GetVolume(menuMusic);

        hoverBase = GetVolume(buttonAudio);
        clickBase = GetVolume(clickAudio);
        staticBase = GetVolume(radioStatic);
        effectsBase = GetVolume(introEffects);

        // Prepare the settings panel.
        settingsGroup =
            settingsPanel.GetComponent<CanvasGroup>();

        if (settingsGroup == null)
        {
            settingsGroup =
                settingsPanel.AddComponent<CanvasGroup>();
        }

        settingsGroup.alpha = 0f;
        settingsGroup.interactable = false;
        settingsGroup.blocksRaycasts = false;

        settingsPanel.SetActive(false);

        // Configure sliders.
        ConfigureSlider(masterVolumeSlider);
        ConfigureSlider(musicVolumeSlider);
        ConfigureSlider(sfxVolumeSlider);

        // Load previously saved settings.
        float master = PlayerPrefs.GetFloat(
            "DeadAirVolume", 0.8f);

        float music = PlayerPrefs.GetFloat(
            "DeadAirMusic", 1f);

        float sfx = PlayerPrefs.GetFloat(
            "DeadAirSFX", 1f);

        masterVolumeSlider.SetValueWithoutNotify(master);
        musicVolumeSlider.SetValueWithoutNotify(music);
        sfxVolumeSlider.SetValueWithoutNotify(sfx);

        SetMasterVolume(master);
        SetMusicVolume(music);
        SetSFXVolume(sfx);

        // Connect buttons and sliders automatically.
        settingsButton.onClick.AddListener(OpenSettings);
        backButton.onClick.AddListener(CloseSettings);

        masterVolumeSlider.onValueChanged.AddListener(
            SetMasterVolume);

        musicVolumeSlider.onValueChanged.AddListener(
            SetMusicVolume);

        sfxVolumeSlider.onValueChanged.AddListener(
            SetSFXVolume);
    }

    AudioSource FindAudio(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);

        if (obj == null)
            return null;

        return obj.GetComponent<AudioSource>();
    }

    float GetVolume(AudioSource source)
    {
        return source != null ? source.volume : 0f;
    }

    void ConfigureSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    // VOLUME CONTROLS

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("DeadAirVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (introMusic != null)
            introMusic.volume = introBase * volume;

        if (menuMusic != null)
            menuMusic.volume = menuBase * volume;

        // Keep the cinematic transition in sync.
        if (introController != null)
        {
            introController.menuMusicVolume =
                menuBase * volume;
        }

        PlayerPrefs.SetFloat("DeadAirMusic", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (buttonAudio != null)
            buttonAudio.volume = hoverBase * volume;

        if (clickAudio != null)
            clickAudio.volume = clickBase * volume;

        if (radioStatic != null)
            radioStatic.volume = staticBase * volume;

        if (introEffects != null)
            introEffects.volume = effectsBase * volume;

        PlayerPrefs.SetFloat("DeadAirSFX", volume);
    }

    // SETTINGS SCREEN

    public void OpenSettings()
    {
        StartFade(true);
    }

    public void CloseSettings()
    {
        PlayerPrefs.Save();
        StartFade(false);
    }

    void StartFade(bool opening)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (opening)
        {
            settingsPanel.SetActive(true);
            settingsGroup.interactable = true;
            settingsGroup.blocksRaycasts = true;
        }
        else
        {
            settingsGroup.interactable = false;
            settingsGroup.blocksRaycasts = false;
        }

        fadeRoutine =
            StartCoroutine(FadePanel(opening));
    }

    IEnumerator FadePanel(bool opening)
    {
        float startAlpha = settingsGroup.alpha;
        float targetAlpha = opening ? 1f : 0f;

        float timer = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = t * t * (3f - 2f * t);

            settingsGroup.alpha = Mathf.Lerp(
                startAlpha, targetAlpha, t);

            yield return null;
        }

        settingsGroup.alpha = targetAlpha;

        if (!opening)
            settingsPanel.SetActive(false);

        fadeRoutine = null;
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
