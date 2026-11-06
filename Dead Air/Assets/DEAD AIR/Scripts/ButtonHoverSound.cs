
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioSource buttonAudio;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonAudio != null && buttonAudio.clip != null)
        {
            buttonAudio.PlayOneShot(buttonAudio.clip);
        }
    }
}

