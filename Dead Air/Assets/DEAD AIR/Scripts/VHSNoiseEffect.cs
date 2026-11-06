
using UnityEngine;
using UnityEngine.UI;

public class VHSNoiseEffect : MonoBehaviour
{
    [Header("VHS Appearance")]
    [Range(0f, 1f)]
    public float opacity = 0.16f;

    [Range(5f, 30f)]
    public float refreshRate = 15f;

    [Range(0f, 1f)]
    public float glitchChance = 0.10f;

    private RawImage noiseImage;
    private Texture2D noiseTexture;
    private Color32[] pixels;

    private System.Random random = new System.Random();

    private float timer = 0f;

    private const int width = 256;
    private const int height = 144;

    void Awake()
    {
        noiseImage = GetComponent<RawImage>();

        if (noiseImage == null)
        {
            Debug.LogError(
                "DEAD AIR: VHS effect needs a Raw Image!");

            enabled = false;
            return;
        }

        // Generate our television static texture.
        noiseTexture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        noiseTexture.filterMode = FilterMode.Point;
        noiseTexture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color32[width * height];

        noiseImage.texture = noiseTexture;
        noiseImage.raycastTarget = false;

        noiseImage.color = new Color(
            1f, 1f, 1f, opacity);

        GenerateNoise();
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer < 1f / refreshRate)
            return;

        timer = 0f;

        GenerateNoise();

        // Occasional flicker in the signal.
        float flicker = random.NextDouble() < 0.06
            ? 1.6f
            : 1f;

        noiseImage.color = new Color(
            1f,
            1f,
            1f,
            Mathf.Clamp01(opacity * flicker)
        );
    }

    void GenerateNoise()
    {
        // Sometimes create a horizontal glitch band.
        bool glitch =
            random.NextDouble() < glitchChance;

        int bandStart = random.Next(10, height - 10);
        int bandHeight = random.Next(3, 12);

        for (int y = 0; y < height; y++)
        {
            bool scanline = y % 3 == 0;

            bool glitchBand = glitch &&
                y >= bandStart &&
                y < bandStart + bandHeight;

            for (int x = 0; x < width; x++)
            {
                int brightness = random.Next(35, 230);

                // Dark horizontal scanlines.
                if (scanline)
                {
                    brightness = (int)(brightness * 0.35f);
                }

                // Occasional brighter interference bands.
                if (glitchBand)
                {
                    brightness = random.Next(150, 255);
                }

                byte value = (byte)brightness;

                pixels[y * width + x] = new Color32(
                    value,
                    value,
                    value,
                    255
                );
            }
        }

        noiseTexture.SetPixels32(pixels);
        noiseTexture.Apply(false);
    }

    void OnDestroy()
    {
        if (noiseTexture != null)
        {
            Destroy(noiseTexture);
        }
    }
}
