using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenerativeUI : BaseUICanvas
{
  public CanvasGroup transitionTextCanvasGroup;
  public CanvasGroup titleTextCanvasGroup;
  public TextMeshProUGUI text;
  public TextMeshProUGUI[] titleMeaningTexts; // probably 3
  public bool reading = false;
  public float totalReadingTime = 5.0f;
  public float backgroundFadeSpeed = 1.0f;
  public float titleCardFadeSpeed = 1.0f;

  public void Start()
  {
    titleTextCanvasGroup.alpha = 0;
    // ReadingUtils.HideAllCharacters(text);
  }

  public void SetMeanings(string[] meanings)
  {
    if (meanings.Length != titleMeaningTexts.Length)
    {
      Debug.LogError("mismatch between number of cards read and number of title texts in generative phase. This won't end well");
    }
    for (int i = 0; i < meanings.Length; i++)
    {
      titleMeaningTexts[i].text = meanings[i];
    }
  }
  public IEnumerator ReadText()
  {
    reading = true;
    Debug.Log("total reading time " + totalReadingTime);
    yield return new WaitForSeconds(1);
    reading = false;
  }

  public IEnumerator ShowTitleCard()
  {

    while (transitionTextCanvasGroup.alpha > 0)
    {
      transitionTextCanvasGroup.alpha -= titleCardFadeSpeed * Time.deltaTime;
      yield return null;
    }
    while (titleTextCanvasGroup.alpha < 1)
    {
      titleTextCanvasGroup.alpha += titleCardFadeSpeed * Time.deltaTime;
      yield return null;
    }
  }

  public IEnumerator DoGeneration(List<TarotCardData> cardDatas)
  {
    yield return StartCoroutine(FadeOut());
  }

  public void Reset()
  {
    titleTextCanvasGroup.alpha = 0;
    // textCanvasGroup.alpha = 0;
    // text.maxVisibleCharacters = 0;\
    // ReadingUtils.HideAllCharacters(text);
  }
}