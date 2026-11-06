
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class IntroTextGlitch : MonoBehaviour
{
    [Header("Glitch Settings")]
    public float glitchDuration = 0.3f;
    public float shakeAmount = 9f;
    public float glitchSpeed = 0.04f;
    public float spacingDistortion = 5f;

    private TextMeshProUGUI introText;
    private RectTransform textRect;

    private string previousText;
    private Vector2 originalPosition;
    private float originalSpacing;

    private float glitchTimer;
    private float updateTimer;

    void Awake()
    {
        introText = GetComponent<TextMeshProUGUI>();
        textRect = GetComponent<RectTransform>();

        originalPosition = textRect.anchoredPosition;
        originalSpacing = introText.characterSpacing;
        previousText = introText.text;
    }

    void OnEnable()
    {
        glitchTimer = glitchDuration;
    }

    void LateUpdate()
    {
        // Detect when the intro controller changes the words.
        if (introText.text != previousText)
        {
            previousText = introText.text;
            glitchTimer = glitchDuration;
            updateTimer = 0f;
        }

        if (glitchTimer > 0f)
        {
            glitchTimer -= Time.unscaledDeltaTime;
            updateTimer -= Time.unscaledDeltaTime;

            if (updateTimer <= 0f)
            {
                updateTimer = Mathf.Max(0.01f, glitchSpeed);

                float x = Random.Range(
                    -shakeAmount, shakeAmount);

                float y = Random.Range(-2f, 2f);

                textRect.anchoredPosition =
                    originalPosition + new Vector2(x, y);

                introText.characterSpacing =
                    originalSpacing +
                    Random.Range(
                        -spacingDistortion,
                        spacingDistortion);
            }
        }
        else
        {
            textRect.anchoredPosition = originalPosition;
            introText.characterSpacing = originalSpacing;
        }
    }

    void OnDisable()
    {
        if (textRect != null)
            textRect.anchoredPosition = originalPosition;

        if (introText != null)
            introText.characterSpacing = originalSpacing;
    }
}

