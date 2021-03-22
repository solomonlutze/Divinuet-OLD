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
  public Button rereadButton;
  int readingOrder;
  GameRunner gameRunner;
  public float fadeSpeed = 1.0f;
  // public bool isReversed;
  // Start is called before the first frame update
  public void Init(TarotCardData cd, string cardMeaning, int order, GameRunner gr)
  {
    readingOrder = order;
    gameRunner = gr;
    cardMeaningText.text = cardMeaning;
    cardData = cd;
    cardFront.sprite = cardData.cardPicture2x;
  }

  public void OnClickReread()
  {
    Debug.Log("OnClickReread");
    StartCoroutine(gameRunner.RereadCard(readingOrder));
  }

  public void EnableButton(bool enable)
  {
    rereadButton.gameObject.SetActive(enable);
  }

  public void OnHover(bool hover)
  {
    Debug.Log("OnHover");
    gameRunner.buttonHover = hover;
  }
}
