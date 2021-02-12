using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SavedReadingsUI : BaseUICanvas
{
    // Start is called before the first frame update

    public GameObject savedReadingPrefab;
    public Transform savedReadingsContainer;
    public List<SavedReading> savedReadings;
    public Transform confirmDeleteReadingUI;
    public SavedReading readingToDelete;
    public Deck deck;
    public UnityEngine.UI.Button DEBUG_generateReadingsButton;

    public void Init(Deck d)
    {
        Debug.Log("inside init");
        deck = d;
        foreach (Transform child in savedReadingsContainer.transform)
        {
            Destroy(child.gameObject); // macabre
        }
        if (d.DEBUG_EnableReadingGeneration)
        {
            DEBUG_generateReadingsButton.gameObject.SetActive(true);
        }
        else
        {
            DEBUG_generateReadingsButton.gameObject.SetActive(false);
        }
        SaveData data = SaveUtils.LoadSaveData();
        savedReadings = data.savedReadings;
        confirmDeleteReadingUI.gameObject.SetActive(false);
        List<SavedReading> favoriteReadings = new List<SavedReading>();
        List<SavedReading> regularReadings = new List<SavedReading>(); // little old bullshit readings, fuck em
        foreach (SavedReading reading in data.savedReadings)
        {
            if (reading.isFavorite)
            {
                favoriteReadings.Add(reading);
            }
            else
            {
                regularReadings.Add(reading);
            }
        }
        foreach (SavedReading reading in favoriteReadings.Concat(regularReadings))
        {
            SavedReadingCard card = Instantiate(savedReadingPrefab, savedReadingsContainer).GetComponent<SavedReadingCard>();
            card.Init(reading, this, favoriteReadings.Count < 20);
        }
    }

    public void DoReading(SavedReading reading)
    {
        deck.RevisitSavedReading(reading);
    }

    public void FavoriteOrUnfavoriteReading(SavedReading readingToFavoriteOrUnfavorite)
    {
        readingToFavoriteOrUnfavorite.isFavorite = !readingToFavoriteOrUnfavorite.isFavorite; // toggle isFavorite by setting it to whatever it isn't
        SaveUtils.SaveReadings(savedReadings);
        Init(deck);
    }

    public void ConfirmDeleteSavedReading(SavedReading reading)
    {
        confirmDeleteReadingUI.gameObject.SetActive(true);
        readingToDelete = reading;
    }

    public void DeleteSavedReading()
    {
        if (readingToDelete != null)
        {
            savedReadings.Remove(readingToDelete);
            SaveUtils.SaveReadings(savedReadings);
            Init(deck);
        }
        else
        {
            Debug.LogError("tried to delete a saved reading when one wasn't set");
        }
    }

    public void CancelDeleteSavedReading()
    {
        confirmDeleteReadingUI.gameObject.SetActive(false);
        readingToDelete = null;
    }

    public void DEBUG_GenerateReadings()
    {
        SaveUtils.DEBUG_GenerateAndSave100Readings();
        Init(deck);
    }
}
