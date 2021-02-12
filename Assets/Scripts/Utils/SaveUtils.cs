using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveUtils
{
    public static void SaveReading(SavedReading savedReading)
    {
        SaveData saveData = LoadSaveData();
        if (saveData.savedReadings.Count > 0)
        {
            SavedReading last = saveData.savedReadings[saveData.savedReadings.Count - 1];
            int[] cards = last.cards;
        }
        saveData.savedReadings.Insert(0, savedReading);
        SaveReadings(saveData.savedReadings);
    }

    public static void DeleteReading(SavedReading savedReading)
    {
        SaveData saveData = LoadSaveData();
        if (saveData.savedReadings.Count > 0)
        {
            saveData.savedReadings.Remove(savedReading);
        }
        SaveReadings(saveData.savedReadings);
    }

    public static void SaveReadings(List<SavedReading> savedReadings)
    {
        SaveData saveData = new SaveData();
        if (savedReadings.Count > 100)
        {
            savedReadings = savedReadings.GetRange(0, 100); // limit to 100
        }
        saveData.savedReadings = savedReadings;

        Save(saveData);
    }

    public static void Save(SaveData saveData)
    {
        File.WriteAllText(Application.persistentDataPath + "/savedreadings.json", JsonUtility.ToJson(saveData));
    }

    public static SaveData LoadSaveData()
    {
        if (File.Exists(Application.persistentDataPath + "/savedreadings.json"))
        {
            SaveData savedReadings = JsonUtility.FromJson<SaveData>(File.ReadAllText(Application.persistentDataPath + "/savedreadings.json"));
            Debug.Log("loaded " + savedReadings.savedReadings.Count + "saved readings");
            return savedReadings;
        }
        else
        {
            return new SaveData();
        }
    }

    public static void DEBUG_GenerateAndSave100Readings()
    {
        List<SavedReading> savedReadings = new List<SavedReading>();
        long startingDateTime = System.DateTime.Now.Ticks;
        for (int i = 0; i < 100; i++)
        {
            startingDateTime -= Random.Range(5000000, 10000000);
            savedReadings.Add(new SavedReading(
              startingDateTime,
              ChooseThreeRandomCards(),
              new string[] { "one", "two", "three" },
              i % 5 == 0
            ));
        }
        SaveReadings(savedReadings);
    }

    public static int[] ChooseThreeRandomCards()
    {
        List<int> selectedCards = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            int randomCardOrderNum = GameMaster.Instance.cardsData[Random.Range(0, GameMaster.Instance.cardsData.Length)].order;
            while (selectedCards.Contains(randomCardOrderNum))
            {
                randomCardOrderNum = GameMaster.Instance.cardsData[Random.Range(0, GameMaster.Instance.cardsData.Length)].order;
            }
            selectedCards.Add(randomCardOrderNum);
        }
        return selectedCards.ToArray();
    }
}


[System.Serializable]
public class SaveData
{
    public List<SavedReading> savedReadings;
    public int testInt;
    public SaveData()
    {
        savedReadings = new List<SavedReading>();
    }
}

[System.Serializable]
public class SavedReading
{
    public long dateTime;
    public int[] cards;
    public string[] cardMeanings;
    public bool isFavorite;

    public SavedReading(long dt, int[] c, string[] cm, bool f)
    {
        dateTime = dt;
        cards = c;
        cardMeanings = cm;
        isFavorite = f;
    }
}