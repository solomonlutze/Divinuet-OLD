using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PredefinedSpread
{
  public string name = "Meaning";
  public string[] cardMeanings = new string[3];
}
public class PredefinedSpreadUI : MonoBehaviour
{
  // Start is called before the first frame update
  public GameRunner runner;
  public Transform buttonsContainer;
  public List<PredefinedSpread> spreads;
  public GameObject spreadButtonPrefab;
  void Start()
  {
    for (int i = buttonsContainer.childCount - 1; i >= 0; i--)
    {
      Destroy(buttonsContainer.GetChild(i).gameObject);
    }
    foreach (PredefinedSpread spread in spreads)
    {
      GameObject button = Instantiate(spreadButtonPrefab, buttonsContainer);
      button.GetComponentInChildren<TextMeshProUGUI>().text = spread.name;
      button.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(spread));
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void OnButtonClick(PredefinedSpread spread)
  {
    Debug.Log("button click " + spread.name);
    runner.SetCardMeaningTexts(spread.cardMeanings);
  }
}
