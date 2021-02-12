using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenerativeUI : BaseUICanvas
{
    public CanvasGroup textCanvasGroup;
    public TextMeshProUGUI text;
    public bool reading = false;
    public float totalReadingTime = 5.0f;
    public float backgroundFadeSpeed = 1.0f;
    

    public void Start() {
        ReadingUtils.HideAllCharacters(text);
    }
    public IEnumerator ReadText() {
        reading = true;
        yield return ReadingUtils.ReadText(text, totalReadingTime);
        reading = false;
    }

    public IEnumerator DoGeneration(List<TarotCard> cards) {
        yield return StartCoroutine(FadeOut());
    }

    public void Reset() {
        // textCanvasGroup.alpha = 0;
        text.maxVisibleCharacters = 0;
    }
}