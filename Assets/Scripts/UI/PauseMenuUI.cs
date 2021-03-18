using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PauseMenuUI : BaseUICanvas
{
  // Start is called before the first frame update

  public TextMeshProUGUI confirmLeaveReadingText;
  public Transform confirmLeaveReadingModal;
  ConfirmLeaveReadingOption currentLeaveReadingOption;

  public enum ConfirmLeaveReadingOption { NewReading, MainMenu, QuitGame }
  public GameRunner gameRunner;

  void OnEnable()
  {
    confirmLeaveReadingModal.gameObject.SetActive(false);
  }

  void OnDisable()
  {
    confirmLeaveReadingModal.gameObject.SetActive(false);
  }

  public void HideConfirmNewReadingModal()
  {
    confirmLeaveReadingModal.gameObject.SetActive(false);
  }

  public void ShowConfirmNewReadingModal()
  {
    confirmLeaveReadingModal.gameObject.SetActive(true);
    currentLeaveReadingOption = ConfirmLeaveReadingOption.NewReading;
    confirmLeaveReadingText.text = "Begin a new reading?";
  }

  public void ShowConfirmMainMenuModal()
  {
    confirmLeaveReadingModal.gameObject.SetActive(true);
    currentLeaveReadingOption = ConfirmLeaveReadingOption.MainMenu;
    confirmLeaveReadingText.text = "Return to the main menu?";
  }


  public void ShowConfirmQuitModal()
  {
    confirmLeaveReadingModal.gameObject.SetActive(true);
    currentLeaveReadingOption = ConfirmLeaveReadingOption.QuitGame;
    confirmLeaveReadingText.text = "Quit?";
  }

  public void LeaveReading()
  {
    switch (currentLeaveReadingOption)
    {
      case ConfirmLeaveReadingOption.NewReading:
        NewReading();
        break;
      case ConfirmLeaveReadingOption.MainMenu:
        MainMenu();
        break;
      case ConfirmLeaveReadingOption.QuitGame:
      default:
        Quit();
        break;
    }
  }

  void NewReading()
  {
    gameRunner.ToggleHowToPlayMenu();
  }

  void MainMenu()
  {
    gameRunner.QuitToMainMenu();
  }

  void Quit()
  {
    gameRunner.QuitGame();
  }
}