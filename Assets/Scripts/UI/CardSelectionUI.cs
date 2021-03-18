using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// First pass:
// Populate list of all card names and allow user to pick them.
// Choosing a card adds it to Deck.cs's dealtCards.
// Once 3 are chosen, press a "start" button to hide this canvas and begin game normally.

public class CardSelectionUI : MonoBehaviour
{
  // Start is called before the first frame update

  public Button cardChooseButtonPrefab;
  public Button beginGameButton;
  public Transform cardsList;
  public Transform chosenCardsList;
  public Color cardButtonDefaultColor;
  public Color cardButtonSelectedColor;
  GameRunner gameRunner;
  TarotCardData[] cardsData;
  // mapping of the selected card's ordering to the selected cards themselves
  Dictionary<int, GameObject> deckOrderToDeckCardButtonMap;

  // mapping of the selected card's ordering to the selected cards themselves
  Dictionary<int, GameObject> deckOrderToSelectedCardButtonMap;
  public void Init(TarotCardData[] cd, GameRunner gr)
  {
    gameRunner = gr;
    deckOrderToDeckCardButtonMap = new Dictionary<int, GameObject>();
    deckOrderToSelectedCardButtonMap = new Dictionary<int, GameObject>();
    beginGameButton.interactable = false;
    cardsData = cd;
    DisplayAllCardButtons();
  }

  void DisplayButtonsForSuit(CardSuit? suit)
  {
    foreach (Transform child in cardsList.transform)
    {
      GameObject.Destroy(child.gameObject);
    }
    foreach (TarotCardData card in cardsData)
    {
      if (suit == null || card.suit == suit)
      {
        CreateChooseCardButton(card);
      }
    }
  }
  public void DisplayAllCardButtons()
  {
    DisplayButtonsForSuit(null);
  }
  public void DisplayMajorArcanaCardButtons()
  {
    DisplayButtonsForSuit(CardSuit.MajorArcana);
  }
  public void DisplayCupsCardButtons()
  {
    DisplayButtonsForSuit(CardSuit.Cups);
  }
  public void DisplayWandsCardButtons()
  {
    DisplayButtonsForSuit(CardSuit.Wands);
  }
  public void DisplaySwordsCardButtons()
  {
    DisplayButtonsForSuit(CardSuit.Swords);
  }
  public void DisplayPentaclesCardButtons()
  {
    DisplayButtonsForSuit(CardSuit.Pentacles);
  }
  void CreateChooseCardButton(TarotCardData card)
  {
    TextMeshProUGUI textComp;
    Button button = Instantiate(cardChooseButtonPrefab).GetComponent<Button>();
    button.transform.parent = cardsList;
    button.image.color = cardButtonDefaultColor;
    textComp = null;
    textComp = button.GetComponentInChildren<TextMeshProUGUI>();
    button.onClick.AddListener(() => ChooseCard(card, button));
    if (textComp)
    {
      textComp.text = card.cardName;
    }
    if (gameRunner.cardsSelectedToDeal.Contains(card.order))
    {
      button.interactable = false;
    }
  }

  void ChooseCard(TarotCardData card, Button button)
  {
    if (gameRunner.cardsSelectedToDeal.Contains(card.order))
    {
      // this card is already selected; remove it!
      Debug.Log("removing card " + card.name);
      button.image.color = cardButtonDefaultColor;
      gameRunner.RemoveChosenCard(card);
      Destroy(deckOrderToSelectedCardButtonMap[card.order]);
      deckOrderToSelectedCardButtonMap.Remove(card.order);
      deckOrderToDeckCardButtonMap.Remove(card.order);
      MaybeEnableAllButtons();
    }
    else
    {
      Debug.Log("choosing card " + card.name);
      button.interactable = false;
      gameRunner.ChooseCard(card);
      deckOrderToDeckCardButtonMap.Add(card.order, button.gameObject);
      Button selectedCardButton = Instantiate(cardChooseButtonPrefab);
      selectedCardButton.transform.SetParent(chosenCardsList);
      selectedCardButton.image.color = cardButtonSelectedColor;
      TextMeshProUGUI textComp = null;
      textComp = selectedCardButton.GetComponentInChildren<TextMeshProUGUI>();
      selectedCardButton.onClick.AddListener(() => ChooseCard(card, selectedCardButton));
      if (textComp)
      {
        textComp.text = card.cardName;
      }
      deckOrderToSelectedCardButtonMap.Add(card.order, selectedCardButton.gameObject);
      MaybeDisableAllButtons();
    }
  }
  void MaybeDisableAllButtons()
  {
    if (gameRunner.cardsSelectedToDeal.Count == gameRunner.dealtCardLocations.Length)
    {
      foreach (Button btn in cardsList.GetComponentsInChildren<Button>())
      {
        btn.interactable = false;
      }
      beginGameButton.interactable = true;
    }
  }
  void MaybeEnableAllButtons()
  {
    if (gameRunner.cardsSelectedToDeal.Count < gameRunner.dealtCardLocations.Length)
    {
      foreach (Button btn in cardsList.GetComponentsInChildren<Button>())
      {
        if (!deckOrderToDeckCardButtonMap.ContainsValue(btn.gameObject))
        {
          btn.interactable = true;
        }
      }
      beginGameButton.interactable = false;
    }
  }
}
