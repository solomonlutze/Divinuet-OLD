﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardReadingUI : BaseUICanvas
{
  // Start is called before the first frame update
  public Image cardImage;
  public Image backgroundColor;
  public TextMeshProUGUI cardText;
  public bool reading;
  public float totalReadingTime = 45.0f;
  public Color32 cupsBGColor;
  public Color32 swordsBGColor;
  public Color32 wandsBGColor;
  public Color32 pentaclesBGColor;
  public Color32 majorArcanaBGColor;
  public bool DEBUG_gottaGoFast;

  public void Start()
  {
    canvasGroup = GetComponent<CanvasGroup>();
    canvasGroup.alpha = 0;
  }


  public void Init(TarotCardData cardData)
  {
    cardImage.sprite = cardData.cardPicture2x;
    ReadingUtils.HideAllCharacters(cardText);
    totalReadingTime = DEBUG_gottaGoFast ? 2f : (cardData.clipDuration - 2);
    cardText.text = cardData.cardTextMain.ToString() + ReadingUtils.readingBreakCharacter + "\n\n";
    if (cardData.suit == CardSuit.Cups)
    {
      backgroundColor.color = cupsBGColor;
    }
    else if (cardData.suit == CardSuit.Wands)
    {
      backgroundColor.color = wandsBGColor;
    }
    else if (cardData.suit == CardSuit.Swords)
    {
      backgroundColor.color = swordsBGColor;
    }
    else if (cardData.suit == CardSuit.Pentacles)
    {
      backgroundColor.color = pentaclesBGColor;
    }
    else
    {
      backgroundColor.color = majorArcanaBGColor;
    }
    cardText.text += cardData.cardTextUpright.ToString();

  }

  public IEnumerator ReadCard()
  {
    GameRunner.enableButton = false;
    reading = true;
    yield return ReadingUtils.ReadText(cardText, totalReadingTime);
    reading = false;
  }

  // public void RereadCard() {

  // }

}
