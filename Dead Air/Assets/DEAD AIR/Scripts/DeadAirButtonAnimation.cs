
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeadAirButtonAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float hoverScale = 1.05f;
    public float slideDistance = 14f;
    public float animationSpeed = 12f;

    [Header("Accent Color")]
    public Color accentColor = new Color(
        0.75f, 0.25f, 0.3f, 1f);

    private class AnimatedButton
    {
        public RectTransform rect;
        public Vector2 originalPosition;
        public Vector3 originalScale;
        public Image accent;
        public bool hovered;
    }

    private List<AnimatedButton> buttons =
        new List<AnimatedButton>();

    void Awake()
    {
        Button[] menuButtons =
            GetComponentsInChildren<Button>(true);

        foreach (Button button in menuButtons)
        {
            RectTransform rect =
                button.GetComponent<RectTransform>();

            AnimatedButton item = new AnimatedButton();

            item.rect = rect;
            item.originalPosition = rect.anchoredPosition;
            item.originalScale = rect.localScale;

            // Create a red indicator automatically.
            GameObject indicator =
                new GameObject("HoverAccent");

            indicator.transform.SetParent(
                button.transform, false);

            RectTransform bar =
                indicator.AddComponent<RectTransform>();

            bar.anchorMin = new Vector2(0f, 0.5f);
            bar.anchorMax = new Vector2(0f, 0.5f);
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.sizeDelta = new Vector2(4f, 38f);
            bar.anchoredPosition = new Vector2(-18f, 0f);

            item.accent = indicator.AddComponent<Image>();
            item.accent.raycastTarget = false;

            Color hidden = accentColor;
            hidden.a = 0f;
            item.accent.color = hidden;

            // Register hover events automatically.
            EventTrigger trigger =
                button.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = button.gameObject
                    .AddComponent<EventTrigger>();

            AddEvent(
                trigger,
                EventTriggerType.PointerEnter,
                () => item.hovered = true);

            AddEvent(
                trigger,
                EventTriggerType.PointerExit,
                () => item.hovered = false);

            buttons.Add(item);
        }
    }

    void AddEvent(
        EventTrigger trigger,
        EventTriggerType type,
        System.Action action)
    {
        EventTrigger.Entry entry =
            new EventTrigger.Entry();

        entry.eventID = type;

        entry.callback.AddListener(
            (data) => action());

        trigger.triggers.Add(entry);
    }

    void Update()
    {
        float smooth = 1f - Mathf.Exp(
            -animationSpeed * Time.unscaledDeltaTime);

        foreach (AnimatedButton item in buttons)
        {
            float scale = item.hovered
                ? hoverScale
                : 1f;

            Vector3 targetScale =
                item.originalScale * scale;

            Vector2 targetPosition =
                item.originalPosition +
                Vector2.right *
                (item.hovered ? slideDistance : 0f);

            item.rect.localScale = Vector3.Lerp(
                item.rect.localScale,
                targetScale,
                smooth);

            item.rect.anchoredPosition = Vector2.Lerp(
                item.rect.anchoredPosition,
                targetPosition,
                smooth);

            Color color = item.accent.color;

            color.a = Mathf.Lerp(
                color.a,
                item.hovered ? 1f : 0f,
                smooth);

            item.accent.color = color;
        }
    }
}
