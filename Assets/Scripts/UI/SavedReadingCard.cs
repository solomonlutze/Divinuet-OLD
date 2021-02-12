using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "card" as in UI component. I realize it's confusing but I'm not about to change it now lol
public class SavedReadingCard : MonoBehaviour
{
    public TMPro.TextMeshProUGUI dateText;
    public TMPro.TextMeshProUGUI card1Name;
    public TMPro.TextMeshProUGUI card1Meaning;
    public TMPro.TextMeshProUGUI card2Name;
    public TMPro.TextMeshProUGUI card2Meaning;
    public TMPro.TextMeshProUGUI card3Name;
    public TMPro.TextMeshProUGUI card3Meaning;
    public TMPro.TextMeshProUGUI favoriteText;
    public UnityEngine.UI.Button favoriteButton;

    public SavedReading sourceReading;
    public SavedReadingsUI savedReadingsUI;

    public void Init(SavedReading reading, SavedReadingsUI ui, bool favoriteAvailable)
    {
        savedReadingsUI = ui;
        sourceReading = reading;
        dateText.text = new System.DateTime(reading.dateTime).ToString();
        card1Name.text = Deck.GetCardData(reading.cards[0]).cardName;
        card2Name.text = Deck.GetCardData(reading.cards[1]).cardName;
        card3Name.text = Deck.GetCardData(reading.cards[2]).cardName;
        card1Meaning.text = reading.cardMeanings[0];
        card2Meaning.text = reading.cardMeanings[1];
        card3Meaning.text = reading.cardMeanings[2];
        favoriteText.text = reading.isFavorite ? "Unfavorite" : "Favorite";
        if (!favoriteAvailable && !reading.isFavorite)
        {
            favoriteButton.interactable = false;
        }
    }


    public void DoReading()
    {
        savedReadingsUI.DoReading(sourceReading);
    }

    public void FavoriteOrUnfavoriteReading()
    {
        savedReadingsUI.FavoriteOrUnfavoriteReading(sourceReading);
    }

    public void DeleteReading()
    {
        savedReadingsUI.ConfirmDeleteSavedReading(sourceReading);
    }
}
