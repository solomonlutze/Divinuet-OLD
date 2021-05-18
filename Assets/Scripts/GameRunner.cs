using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AK;
//hi
//hello this is megan
public enum GameState
{
  MainMenu,
  ChooseGameMode,
  DefineSpread,
  ViewPreviousReadings,
  ChoosingCards,
  FlippingCard,
  ReadyToFadeInCard,
  FadingCardInDone,
  PreReading,
  ReadingCard,
  RereadingCard,
  ReadingCardDone,
  FadingOutCard,
  FadingOutCardDone,
  BeginGenerativeUI,
  ShowingGenerativeUI,
  ShowingGenerativeUIDone,
  ShowingEndInstructions,
  GenerativePhase,
  GenerativePhaseDone,
  Done
}


public enum GameMode
{
  Random,
  Choose,
  ViewPreviousReadings
}

// sol best comment
public class GameRunner : MonoBehaviour
{
  [Tooltip("Can click to skip through a reading")]
  public bool DEBUG_skipReading;
  [Tooltip("Can click to skip through all readings")]
  public bool DEBUG_skipAllReadings;
  [Tooltip("Save reading when cards fade in, vs after reading")]
  public bool DEBUG_SaveReadingImmediately;
  [Tooltip("Allows you to generate max saved readings from the previous readings screen")]
  public bool DEBUG_EnableReadingGeneration;

  // Card backs. Used for cards in deck and reading cards.
  public GameObject deckCardPrefab;
  // Entire card (front and back) used for readings.
  public GameObject tarotCardPrefab;
  // physical cards used to give the deck some depth and volume.
  // These don't serve any other function; "real" cards, to be read,
  // are instantiated on top of the pile, hopefully unnoticed.
  public DeckCard[] cardsInDeck;
  // how many cards we'll be shuffling
  public int numberOfCardsInDeck;

  public float cardDealSpeed;
  public float cardFlipSpeed;
  public float oddsOfReversedCard = 0f;
  public GameState gameState;
  public GameMode gameMode;
  public int playingClipNumber;
  public UnityEngine.Video.VideoPlayer videoPlayer;
  public Canvas videoCanvas;

  // In-game location to which cards should be dealt. Set in editor.
  public Transform[] dealtCardLocations;
  public CardReadingSpot[] cardReadingSpots;

  public Canvas readingCanvas;
  public Canvas generativeCanvas;
  public Canvas defineSpreadCanvas;
  public Canvas demoInstructionsCanvas;
  public Canvas cardSelectionCanvas;
  public Canvas savedReadingsCanvas;
  public Canvas demoEndCanvas;
  public Canvas spreadCanvas;
  public Canvas pauseCanvas;
  public Canvas settingsCanvas;
  public Canvas mainMenuCanvas;
  public Canvas howToPlayCanvas;
  public List<Canvas> canvases;
  public bool paused;

  public string[] cardMeanings;
  public GameObject card1InputField;
  public GameObject card2InputField;
  public GameObject card3InputField;

  public int majorArcanaTotal = 0;
  public int cupsTotal = 0;
  public int wandsTotal = 0;
  // metal af
  public int swordsTotal = 0;
  public int pentaclesTotal = 0;
  public string suitMajority;

  //Wwise events to be set in editor
  public AK.Wwise.Event readingStart;
  public AK.Wwise.Event makingSongState;
  public AK.Wwise.Event generativeState;
  public AK.Wwise.Event readingState;
  public AK.Wwise.Event accompanimentCups;
  public AK.Wwise.Event accompanimentWands;
  public AK.Wwise.Event accompanimentPentacles;
  public AK.Wwise.Event accompanimentSwords;
  public AK.Wwise.Event accompanimentMajorArcana;
  public AK.Wwise.Event accompanimentNull;
  public AK.Wwise.Event cardSlideSFX;

  //Generative phase Wwise events
  public AK.Wwise.Event[] card1MelodyEvents;
  public AK.Wwise.Event[] card2MelodyEvents;
  public AK.Wwise.Event[] card3MelodyEvents;
  public AK.Wwise.Event[] groupMelodyEvents = new AK.Wwise.Event[3];
  public AK.Wwise.Event[] card1SuitEvents;
  public AK.Wwise.Event[] card2SuitEvents;
  public AK.Wwise.Event[] card3SuitEvents;
  public AK.Wwise.Event[] cardSuitMelodyEvents = new AK.Wwise.Event[3];

  //Generative phase videos
  public UnityEngine.Video.VideoClip[] suitMajorityClips;
  public int setClipNumber;
  public int generativeSection;
  public UnityEngine.Video.VideoClip[] videoClips = new UnityEngine.Video.VideoClip[8];





  public Texture2D waitCursor;

  private CardReadingUI readingUI;
  private GenerativeUI generativeUI;

  private CardSelectionUI cardSelectionUI;
  private SavedReadingsUI savedReadingsUI;
  // list of the card's _order_ property of which cards have been selected
  public List<int> cardsSelectedToDeal;
  // Individual card data for cards chosen to be flipped. Attached to a physical gameObject
  // that looks like a material card.
  private List<TarotCardData> selectedCardData;
  public int numberCardsAlreadyDealt = 0;
  // Keeps track of how many cards have been read so far in this reading
  private int numCardsAlreadyRead = 0;
  public static bool enableButton = true;
  public bool buttonHover = false;
  public Color32[] groupSparkColors;
  public ParticleSystem[] sparks;

  public static bool InReadingGameState(GameState state)
  {
    return (
      state == GameState.ReadingCard
      || state == GameState.RereadingCard
      || state == GameState.ReadingCardDone
      || state == GameState.FadingOutCard
      || state == GameState.FlippingCard
    );
  }
  // Start is called before the first frame update
  void Start()
  {
    readingUI = readingCanvas.GetComponent<CardReadingUI>();
    generativeUI = generativeCanvas.GetComponent<GenerativeUI>();
    cardSelectionUI = cardSelectionCanvas.GetComponent<CardSelectionUI>();
    savedReadingsUI = savedReadingsCanvas.GetComponent<SavedReadingsUI>();
    cardsSelectedToDeal = new List<int>();
    selectedCardData = new List<TarotCardData>();
    cardsInDeck = new DeckCard[numberOfCardsInDeck];
    enableButton = false;
    majorArcanaTotal = 0;
    cupsTotal = 0;
    wandsTotal = 0;
    pentaclesTotal = 0;
    swordsTotal = 0;
    buttonHover = false;
    canvases = new List<Canvas> {
      mainMenuCanvas,
      demoInstructionsCanvas,
      demoEndCanvas,
      generativeCanvas,
      readingCanvas,
      savedReadingsCanvas,
      cardSelectionCanvas,
      defineSpreadCanvas,
      spreadCanvas,
      pauseCanvas,
      settingsCanvas,
      howToPlayCanvas
    // add any new canvases here!
    };
    SetGameState(GameState.MainMenu);
  }

  void ClearGameState()
  {
    StopAllCoroutines();
    foreach (ParticleSystem spark in sparks)
    {
      sparks[numCardsAlreadyRead].Stop();
      sparks[numCardsAlreadyRead].Clear();
    }
    numCardsAlreadyRead = 0;
    readingUI.StopAllCoroutines();
    StartCoroutine(readingUI.FadeOut());
    generativeUI.StopAllCoroutines();
    StartCoroutine(generativeUI.FadeOut());
    generativeUI.Reset();
  }

  public void ResetGameState()
  {
    cardsSelectedToDeal = new List<int>();
    selectedCardData = new List<TarotCardData>();
    cardsInDeck = new DeckCard[numberOfCardsInDeck];
    enableButton = false;
    majorArcanaTotal = 0;
    cupsTotal = 0;
    wandsTotal = 0;
    pentaclesTotal = 0;
    swordsTotal = 0;
    buttonHover = false;
    Unpause();
    generativeSection = 0;
    foreach (CardReadingSpot cardReadingSpot in cardReadingSpots)
    {
      cardReadingSpot.canvasGroup.alpha = 0;
    }
    for (int i = 0; i < numberOfCardsInDeck; i++)
    {
      cardsInDeck[i] = Instantiate(deckCardPrefab).GetComponent<DeckCard>();
      cardsInDeck[i].transform.position = new Vector3(
        transform.position.x,
        transform.position.y,
        transform.position.z - (i * 1f)
      );
    }
  }


  // Runs every frame.
  // Used in this class to handle user input.
  void Update()
  {
    if (Input.GetMouseButtonDown(0) && !buttonHover)
    {
      switch (gameState)
      {
        case GameState.ChoosingCards:
          break;
        case GameState.ReadyToFadeInCard:
          if (DEBUG_skipAllReadings)
          {
            gameState = GameState.ShowingGenerativeUI;
            DoGenerativePhase();
          }
          else
          {
            SetGameState(GameState.FlippingCard);
          }
          break;
        case GameState.FadingOutCardDone:
          SetGameState(GameState.FlippingCard);
          break;
        case GameState.ReadingCard:
          if (DEBUG_skipReading)
          {
            Debug.Log("skipping reading?");
            readingUI.reading = false; // megan don't set variables directly on other objects like this ok, this is bad programming
          }
          break;
        case GameState.FadingCardInDone:
          SetGameState(GameState.PreReading);
          break;
        case GameState.ReadingCardDone:
          SetGameState(GameState.FadingOutCard);
          break;
        case GameState.ShowingGenerativeUIDone:
          SetGameState(GameState.ShowingEndInstructions);
          break;
        default:
          break;
      }
      if (cupsTotal >= 2)
      {
        suitMajority = "cups";
        accompanimentCups.Post(gameObject);
        videoClips[0] = suitMajorityClips[1];
      }
      else if (wandsTotal >= 2)
      {
        suitMajority = "wands";
        accompanimentWands.Post(gameObject);
        videoClips[0] = suitMajorityClips[4];
      }
      else if (swordsTotal >= 2)
      {
        suitMajority = "swords";
        accompanimentSwords.Post(gameObject);
        videoClips[0] = suitMajorityClips[2];
      }
      else if (pentaclesTotal >= 2)
      {
        suitMajority = "pentacles";
        accompanimentPentacles.Post(gameObject);
        videoClips[0] = suitMajorityClips[3];
      }
      else if (majorArcanaTotal >= 2)
      {
        suitMajority = "major arcana";
        accompanimentMajorArcana.Post(gameObject);
        videoClips[0] = suitMajorityClips[0];
      }
      else
      {
        suitMajority = null;
        accompanimentNull.Post(gameObject);
        videoClips[0] = suitMajorityClips[5];
      }
    }
    else if (Input.GetButtonDown("Pause"))
    {
      TogglePause();
    }
  }

  public void Unpause()
  {
    paused = false;
    pauseCanvas.gameObject.SetActive(false);
    Time.timeScale = 1;
    if (gameState == GameState.PreReading)
    {
      Cursor.SetCursor(waitCursor, Vector2.zero, CursorMode.Auto);
    }
  }
  public void TogglePause()
  {
    if (paused)
    {
      Unpause();
    }
    else
    {
      Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
      paused = true;
      pauseCanvas.gameObject.SetActive(true);
      Time.timeScale = 0;
    }
  }

  public void ToggleSettingsMenu()
  {
    if (settingsCanvas.gameObject.activeSelf)
    {
      if (gameState == GameState.MainMenu)
      {
        mainMenuCanvas.gameObject.SetActive(true);
      }
      else if (paused)
      {
        pauseCanvas.gameObject.SetActive(true);
      }
      else
      {
        Debug.LogError("toggling settings menu but neither paused nor in main menu");
      }
      settingsCanvas.gameObject.SetActive(false);
    }
    else
    {
      mainMenuCanvas.gameObject.SetActive(false);
      pauseCanvas.gameObject.SetActive(false);
      settingsCanvas.gameObject.SetActive(true);
    }
  }


  public void ToggleHowToPlayMenu()
  {
    mainMenuCanvas.gameObject.SetActive(!mainMenuCanvas.gameObject.activeSelf);
    pauseCanvas.gameObject.SetActive(!pauseCanvas.gameObject.activeSelf);
    howToPlayCanvas.gameObject.SetActive(!howToPlayCanvas.gameObject.activeSelf);
  }

  void CallbackFunction(object in_cookie, AkCallbackType in_type, object in_info)
  {
    if (gameState == GameState.PreReading)
    {
      StartCoroutine(ReadCard());
    }

    else if (gameState == GameState.GenerativePhase || gameState == GameState.ShowingEndInstructions)
    {
      string[] videoClipConsole = {
                "The cards present... reading meaning (e.g. Past, Present, Future)", "Suit majority video clip",
                "Card 1 title card", "Split screen 1- meaning/card video clip", "Thematic group video clip 1",
                "Card 2 title card", "Split screen 2- meaning/card video clip", "Thematic group video clip 2",
                "Card 3 title card", "Split screen 3- meaning/card video clip", "Thematic group video clip 3",
                };


      if (new int[] { 0, 2, 5, 8 }.Contains(generativeSection))
      {
        videoCanvas.gameObject.SetActive(false);
        videoPlayer.clip = videoClips[playingClipNumber];
        videoPlayer.Stop();
        videoPlayer.targetTexture.Release();
        Debug.Log(videoClipConsole[generativeSection]);
        generativeSection++;
      }
      else if (generativeSection >= 11)
      {
        SetGameState(GameState.GenerativePhaseDone);
      }

      else
      {
        videoPlayer.clip = videoClips[playingClipNumber];
        videoPlayer.Play();
        videoCanvas.gameObject.SetActive(true);
        Debug.Log(videoClipConsole[generativeSection]);
        playingClipNumber++;
        generativeSection++;
      }
    }
  }

  public void ChooseCard(TarotCardData card)
  {
    cardsSelectedToDeal.Add(card.order);
  }

  public void StartChooseCardGame()
  {
    foreach (int cardOrder in cardsSelectedToDeal)
    {
      AddCardData(cardOrder);
    }
    SetCanvasActive(spreadCanvas, true);
    SetGameState(GameState.ReadyToFadeInCard);
  }

  public void RemoveChosenCard(TarotCardData card)
  {
    cardsSelectedToDeal.Remove(card.order);
  }
  public void ChooseAllCardsRandomly()
  {
    while (cardsSelectedToDeal.Count < dealtCardLocations.Length)
    {
      int randomCardOrderNum = GameMaster.Instance.cardsData[Random.Range(0, GameMaster.Instance.cardsData.Length)].order;
      while (cardsSelectedToDeal.Contains(randomCardOrderNum))
      {
        randomCardOrderNum = GameMaster.Instance.cardsData[Random.Range(0, GameMaster.Instance.cardsData.Length)].order;
      }
      cardsSelectedToDeal.Add(randomCardOrderNum);
      AddCardData(randomCardOrderNum);
    }
  }
  // instantiate card
  // populate it with specified card data
  public static TarotCardData GetCardData(int cardOrder)
  {
    return System.Array.Find(GameMaster.Instance.cardsData, delegate (TarotCardData cd) { return cd.order == cardOrder; });
  }
  void AddCardData(int cardOrder)
  {
    TarotCardData cardData = GetCardData(cardOrder);
    selectedCardData.Add(cardData);
    videoClips[setClipNumber] = cardData.cardAndGroupClips[setClipNumber - 1];
    Debug.Log("Card Clip " + setClipNumber + " chosen.");
    setClipNumber++;
    videoClips[setClipNumber] = cardData.cardAndGroupClips[setClipNumber - 1];
    Debug.Log("Group Clip " + setClipNumber + " chosen.");
    setClipNumber++;


    if (numberCardsAlreadyDealt == 0)
    {
      groupMelodyEvents[0] = card1MelodyEvents[cardData.thematicGroup - 1];
      int s = (int)cardData.suit;
      cardSuitMelodyEvents[numberCardsAlreadyDealt] = card1SuitEvents[s];
    }

    else if (numberCardsAlreadyDealt == 1)
    {
      groupMelodyEvents[1] = card2MelodyEvents[cardData.thematicGroup - 1];
      int s = (int)cardData.suit;
      cardSuitMelodyEvents[numberCardsAlreadyDealt] = card2SuitEvents[s];

    }
    else if (numberCardsAlreadyDealt == 2)
    {
      groupMelodyEvents[2] = card3MelodyEvents[cardData.thematicGroup - 1];
      int s = (int)cardData.suit;
      cardSuitMelodyEvents[numberCardsAlreadyDealt] = card3SuitEvents[s];

      foreach (AK.Wwise.Event e in groupMelodyEvents)
      {
        e.Post(gameObject);
      }

      foreach (AK.Wwise.Event e in cardSuitMelodyEvents)
      {
        e.Post(gameObject);
        Debug.Log(cardSuitMelodyEvents[0] + " " + cardSuitMelodyEvents[1] + " " + cardSuitMelodyEvents[2]);
      }

      setClipNumber = 1;

    }
    numberCardsAlreadyDealt++;
    if (cardData.suit == CardSuit.MajorArcana)
    {
      majorArcanaTotal++;
    }
    else if (cardData.suit == CardSuit.Cups)
    {
      cupsTotal++;
    }
    else if (cardData.suit == CardSuit.Wands)
    {
      wandsTotal++;
    }
    else if (cardData.suit == CardSuit.Swords)
    {
      swordsTotal++;
    }

    else if (cardData.suit == CardSuit.Pentacles)
    {
      pentaclesTotal++;
    }

  }

  public void ChooseGameMode()
  {
    SetGameState(GameState.ChooseGameMode);
  }

  void PrepForReading()
  {
    if (gameMode != GameMode.ViewPreviousReadings && DEBUG_SaveReadingImmediately)
    {
      SaveUtils.SaveReading(new SavedReading(
        System.DateTime.Now.Ticks,
        cardsSelectedToDeal.ToArray(),
        cardMeanings,
        false
        ));
    }
    enableButton = true;
    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    setClipNumber = 1;
  }

  void FadeOutDeck()
  {
    foreach (DeckCard card in cardsInDeck)
    {
      StartCoroutine(card.FadeOut());
    }
  }

  IEnumerator FadeInCard()
  {
    float t = 0;
    TarotCardData cardData = selectedCardData[numCardsAlreadyRead];
    cardReadingSpots[numCardsAlreadyRead].Init(cardData, cardMeanings[numCardsAlreadyRead], numCardsAlreadyRead, this);
    int groupNumber = cardData.thematicGroup;
    Color sparkColor = groupSparkColors[groupNumber - 1];
    Debug.Log(groupNumber + " " + sparkColor);
    ParticleSystem.MainModule ma = sparks[numCardsAlreadyRead].main;
    ma.startColor = sparkColor;
    sparks[numCardsAlreadyRead].Play();
    while (t < 1)
    {
      t += Time.deltaTime / cardFlipSpeed;
      cardReadingSpots[numCardsAlreadyRead].canvasGroup.alpha = t;
      yield return null;
    }
    SetGameState(GameState.FadingCardInDone);
  }

  IEnumerator ReadCard()
  {
    SetGameState(GameState.ReadingCard);
    TarotCardData cardData = selectedCardData[numCardsAlreadyRead];
    cardData.readingMusicEvent.Post(gameObject);
    readingUI.Init(selectedCardData[numCardsAlreadyRead]);
    yield return StartCoroutine(readingUI.FadeIn());
    Coroutine readingCoroutine = StartCoroutine(readingUI.ReadCard());
    while (readingUI.reading)
    {
      yield return null;
    }
    if (readingCoroutine != null)
    {
      StopCoroutine(readingCoroutine);
    }
    cardReadingSpots[numCardsAlreadyRead].EnableButton(true);
    numCardsAlreadyRead++;
    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    SetGameState(GameState.ReadingCardDone);
  }


  public bool RereadCard(int cardToReread)
  {
    if (!InReadingGameState(gameState))
    {
      StartCoroutine(DoRereadCard(cardToReread));
      return true;
    }
    return false;
  }

  public IEnumerator DoRereadCard(int cardToReread)
  {
    Debug.Log("rereading card " + cardToReread);
    SetGameState(GameState.RereadingCard);
    TarotCardData cardData = selectedCardData[cardToReread];
    readingUI.Init(selectedCardData[cardToReread], true);
    yield return StartCoroutine(readingUI.FadeIn());
    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    SetGameState(GameState.ReadingCardDone);
  }

  IEnumerator FadeOutReading()
  {
    enableButton = false;
    CardReadingUI readingUI = readingCanvas.GetComponent<CardReadingUI>();
    yield return StartCoroutine(readingUI.FadeOut());
    enableButton = true;
    if (numCardsAlreadyRead < selectedCardData.Count)
    {
      SetGameState(GameState.FadingOutCardDone);
    }
    else
    {
      SetGameState(GameState.BeginGenerativeUI);
      yield return StartCoroutine(BeginGenerativePhase());
    }
  }

  IEnumerator BeginGenerativePhase()
  {
    if (gameMode != GameMode.ViewPreviousReadings && !DEBUG_SaveReadingImmediately)
    {
      SaveUtils.SaveReading(new SavedReading(
        System.DateTime.Now.Ticks,
        cardsSelectedToDeal.ToArray(),
        cardMeanings,
        false
        ));
    }

    Debug.Log("Fade out called");
    float t = 0;
    CanvasGroup readingGroup = readingUI.canvasGroup;
    if (readingGroup != null)
    {

      while (t < 1.0)
      {
        t += Time.deltaTime / cardFlipSpeed;
        readingGroup.alpha = 1 - t;
        yield return null;
      }
    }
    foreach (TarotCardData card in selectedCardData)
    {
      foreach (ParticleSystem ps in sparks)
      {
        float GetColorFromCardOrder(int order)
        {
          return (order < 7 ? ((order + 78) * 3) / 255f : (order * 3) / 255f);
        }
        Color particleColor = new Color32();
        particleColor.r = GetColorFromCardOrder(selectedCardData[0].order);
        particleColor.g = GetColorFromCardOrder(selectedCardData[1].order);
        particleColor.b = GetColorFromCardOrder(selectedCardData[2].order);
        particleColor.a = 255;
        ParticleSystem.MainModule ma = ps.main;
        ma.startColor = particleColor;
        ps.Play();
      }
    }

    makingSongState.Post(gameObject);
    Cursor.SetCursor(waitCursor, Vector2.zero, CursorMode.Auto);
    yield return StartCoroutine(generativeUI.FadeIn());
    yield return StartCoroutine(generativeUI.ReadText());
    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    SetGameState(GameState.ShowingGenerativeUIDone);
  }

  void DoGenerativePhase()
  {
    playingClipNumber = 0;
    enableButton = true;
    GenerativeUI generativeUI = generativeCanvas.GetComponent<GenerativeUI>();
    generativeState.Post(gameObject);
    for (int i = 0; i < 3; i++)
    {
      AK.Wwise.Event keyEvent = (selectedCardData[i].cardKeyArray[i]);
      keyEvent.Post(gameObject);
      Debug.Log(i + " " + keyEvent);
    }
    StartCoroutine(generativeUI.DoGeneration(selectedCardData));
    demoEndCanvas.gameObject.SetActive(true);
  }

  public void AfterInstructions_RandomReading()
  {
    SetGameState(GameState.DefineSpread);
    gameMode = GameMode.Random;
    DisableAllCanvases();
    SetCanvasActive(defineSpreadCanvas, true);
  }

  public void AfterInstructions_ChooseCards()
  {
    gameMode = GameMode.Choose;
    DisableAllCanvases();
    SetCanvasActive(defineSpreadCanvas, true);
  }


  public void AfterInstructions_ViewPreviousReadingss()
  {
    SetGameState(GameState.DefineSpread);
    DisableAllCanvases();
    SetCanvasActive(savedReadingsCanvas, true);
    savedReadingsUI.Init(this);
  }


  public void CloseViewPreviousReadings()
  {
    mainMenuCanvas.gameObject.SetActive(true);
    savedReadingsCanvas.gameObject.SetActive(false);
  }

  public void RevisitSavedReading(SavedReading reading)
  {
    Debug.Log("revisiting saved readings");
    gameMode = GameMode.ViewPreviousReadings;
    ResetGameState();
    savedReadingsCanvas.gameObject.SetActive(false);
    cardMeanings = reading.cardMeanings;
    cardsSelectedToDeal = new List<int>(reading.cards);
    StartGame();
  }

  public void SpreadConfirm()
  {
    cardMeanings = new string[] {
          card1InputField.GetComponent<Text>().text,
          card2InputField.GetComponent<Text>().text,
          card3InputField.GetComponent<Text>().text
      };
    StartGame();
  }

  public void StartGame()
  {
    switch (gameMode)
    {
      case GameMode.Random:
        SetGameState(GameState.ReadyToFadeInCard);
        AkSoundEngine.PostEvent("MenuAmbienceStop", this.gameObject);
        readingStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncUserCue, CallbackFunction);
        DisableAllCanvases();
        SetCanvasActive(generativeCanvas, true);
        SetCanvasActive(readingCanvas, true);
        SetCanvasActive(spreadCanvas, true);
        ResetGameState();
        ChooseAllCardsRandomly();
        PrepForReading();
        break;
      case GameMode.ViewPreviousReadings:
        SetGameState(GameState.ReadyToFadeInCard);
        AkSoundEngine.PostEvent("MenuAmbienceStop", this.gameObject);
        readingStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncUserCue, CallbackFunction);
        SetCanvasActive(generativeCanvas, true);
        SetCanvasActive(readingCanvas, true);
        PrepForReading();
        break;
      case GameMode.Choose:
      default:
        SetGameState(GameState.ChoosingCards);
        DisableAllCanvases();
        SetCanvasActive(cardSelectionCanvas, true);
        cardSelectionUI.Init(GameMaster.Instance.cardsData, this);
        ResetGameState();
        break;
    }
  }

  public void DisableEndCanvas()
  {
    SetGameState(GameState.GenerativePhase);
    demoEndCanvas.gameObject.SetActive(false);
  }

  public void QuitToMainMenu()
  {
    SetGameState(GameState.MainMenu);
  }

  public void QuitGame()
  {
    Application.Quit();
  }


  public void StartOver()
  {
    readingState.Post(gameObject);
    ClearGameState();
    ResetGameState();
    SetGameState(GameState.ReadyToFadeInCard);
  }

  public void ButtonHoverOn()
  {
    buttonHover = true;
  }

  public void ButtonHoverOff()
  {
    buttonHover = false;
  }


  public void DisableAllCanvases()
  {
    foreach (Canvas c in canvases)
    {
      SetCanvasActive(c, false);
    }
  }
  public void SetCanvasActive(Canvas c, bool active)
  {
    c.gameObject.SetActive(active);
  }

  public void SetGameState(GameState newState)
  {
    gameState = newState;
    foreach (CardReadingSpot spot in cardReadingSpots)
    {
      spot.OnGameStateChange(gameState);
    }
    switch (newState)
    {
      case (GameState.MainMenu):
        DisableAllCanvases();
        AkSoundEngine.PostEvent("MenuAmbienceStart", this.gameObject);
        SetCanvasActive(mainMenuCanvas, true);
        break;
      case (GameState.FlippingCard):
        if (cardReadingSpots[numCardsAlreadyRead].canvasGroup.alpha < 1)
        {
          StartCoroutine(FadeInCard());
        }
        else
        {
          SetGameState(GameState.PreReading);
        }
        break;
      case (GameState.PreReading):
        enableButton = false;
        Cursor.SetCursor(waitCursor, Vector2.zero, CursorMode.Auto);
        break;
      case (GameState.FadingOutCard):
        StartCoroutine(FadeOutReading());
        break;
      case (GameState.ShowingEndInstructions):
        DoGenerativePhase();
        break;
      case (GameState.GenerativePhaseDone):
        playingClipNumber = 0;
        videoPlayer.clip = videoClips[playingClipNumber];
        videoPlayer.Play();
        videoCanvas.gameObject.SetActive(true);
        break;
      case (GameState.ReadyToFadeInCard):
        PrepForReading();
        AkSoundEngine.PostEvent("MenuAmbienceStop", this.gameObject);
        readingStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncUserCue, CallbackFunction);
        DisableAllCanvases();
        SetCanvasActive(generativeCanvas, true);
        SetCanvasActive(readingCanvas, true);
        SetCanvasActive(spreadCanvas, true);
        break;
      case (GameState.ChooseGameMode):
        readingUI.canvasGroup.alpha = 0;
        ClearGameState();
        ResetGameState();
        DisableAllCanvases();
        demoInstructionsCanvas.gameObject.SetActive(true);
        break;
      default:
        break;
    }
  }
}
