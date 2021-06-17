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


  public void Start()
  {
    // ReadingUtils.HideAllCharacters(text);
  }
  public IEnumerator ReadText()
  {
    reading = true;
    Debug.Log("total reading time " + totalReadingTime);
    yield return new WaitForSeconds(1);
    reading = false;
  }

  public IEnumerator DoGeneration(List<TarotCardData> cardDatas)
  {
    yield return StartCoroutine(FadeOut());
  }

  public void Reset()
  {
    // textCanvasGroup.alpha = 0;
    // text.maxVisibleCharacters = 0;\
    // ReadingUtils.HideAllCharacters(text);
  }
}