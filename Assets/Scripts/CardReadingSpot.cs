using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Takes a TarotCardData object and applies its properties
// to the card/board as required.

public class CardReadingSpot : MonoBehaviour
{
  public TextMeshProUGUI cardMeaningText;
  public Image cardFront;
  public TarotCardData cardData;
  public CanvasGroup canvasGroup;
  public float fadeSpeed = 1.0f;
  // public bool isReversed;
  // Start is called before the first frame update
  public void Init(TarotCardData cd, string cardMeaning)
  {
    cardMeaningText.text = cardMeaning;
    cardData = cd;
    cardFront.sprite = cardData.cardPicture2x;
  }

}
