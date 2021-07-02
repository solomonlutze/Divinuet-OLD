using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoCardUI : BaseUICanvas
{
  public LayoutElement cardDefinitionText;
  public TextMeshProUGUI cardTitleText;
  public TextMeshProUGUI cardDescriptionShortText;
  private GenerativeUI generativeUI;

  public void Start()
  {
  }


  public void SetTextFromCardData(TarotCardData cardData)
  {
    cardTitleText.text = cardData.name;
    cardDescriptionShortText.text = cardData.cardShortDescription;
  }

  public void Reset()
  {
    // textCanvasGroup.alpha = 0;
    // text.maxVisibleCharacters = 0;\
    // ReadingUtils.HideAllCharacters(text);
  }
}