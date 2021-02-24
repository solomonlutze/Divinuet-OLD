using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AK;
//hi
public enum GameState
{
    ChooseGameMode,
    DefineSpread,
    ViewPreviousReadings,
    ChoosingCards,
    FadingInDeck,
    ReadyToDeal,
    Dealing,
    DealingDone,
    FlippingCard,
    FlippingCardDone,
    PreReading,
    ReadingCard,
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

public class Deck : MonoBehaviour
{
    public bool DEBUG_skipReading;
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

    public Canvas readingCanvas;
    public Canvas generativeCanvas;
    public Canvas defineSpreadCanvas;
    public Canvas demoInstructionsCanvas;
    public Canvas cardSelectionCanvas;
    public Canvas savedReadingsCanvas;
    public Canvas gameUICanvas;
    public Canvas demoEndCanvas;

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
    private List<TarotCard> dealtCards;
    public int numberCardsAlreadyDealt = 0;
    // Keeps track of how many cards have been read so far in this reading
    private int numCardsAlreadyRead = 0;
    public static bool enableButton = true;
    public bool buttonHover = false;
    public Color32[] groupSparkColors;
    public ParticleSystem[] sparks;

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.ChooseGameMode;
        AkSoundEngine.PostEvent("MenuAmbienceStart", this.gameObject);
        readingUI = readingCanvas.GetComponent<CardReadingUI>();
        generativeUI = generativeCanvas.GetComponent<GenerativeUI>();
        cardSelectionUI = cardSelectionCanvas.GetComponent<CardSelectionUI>();
        savedReadingsUI = savedReadingsCanvas.GetComponent<SavedReadingsUI>();
        cardsSelectedToDeal = new List<int>();
        dealtCards = new List<TarotCard>();
        cardsInDeck = new DeckCard[numberOfCardsInDeck];
        enableButton = false;
        majorArcanaTotal = 0;
        cupsTotal = 0;
        wandsTotal = 0;
        pentaclesTotal = 0;
        swordsTotal = 0;
        buttonHover = false;
        demoEndCanvas.enabled = false;
        generativeCanvas.enabled = false;
        readingCanvas.enabled = false;
        savedReadingsCanvas.enabled = false;
        gameUICanvas.enabled = false;
        cardSelectionCanvas.enabled = false;
        defineSpreadCanvas.enabled = false;
    }

    void ClearGameState()
    {
        StopAllCoroutines();
        foreach (TarotCard tcard in dealtCards)
        {
            tcard.StopAllCoroutines();
            Destroy(tcard.gameObject);
        }
        foreach (DeckCard dcard in cardsInDeck)
        {
            dcard.StopAllCoroutines();
            Destroy(dcard.gameObject);
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
        dealtCards = new List<TarotCard>();
        cardsInDeck = new DeckCard[numberOfCardsInDeck];
        enableButton = false;
        majorArcanaTotal = 0;
        cupsTotal = 0;
        wandsTotal = 0;
        pentaclesTotal = 0;
        swordsTotal = 0;
        buttonHover = false;
        generativeSection = 0;
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
                case GameState.ReadyToDeal:

                    gameState = GameState.Dealing; // start dealing
                    DealCard();

                    break;
                case GameState.DealingDone:
                    if (DEBUG_skipReading)
                    {
                        gameState = GameState.ShowingGenerativeUI;
                        DoGenerativePhase();
                    }
                    else
                    {
                        gameState = GameState.FlippingCard;
                        StartCoroutine(FlipCard());
                    }
                    break;
                case GameState.FadingOutCardDone:
                    gameState = GameState.FlippingCard;
                    StartCoroutine(FlipCard());
                    break;
                case GameState.FlippingCardDone:
                    gameState = GameState.PreReading;
                    enableButton = false;
                    Cursor.SetCursor(waitCursor, Vector2.zero, CursorMode.Auto);
                    break;
                case GameState.ReadingCardDone:
                    gameState = GameState.FadingOutCard;
                    StartCoroutine(FadeOutReading());
                    break;
                case GameState.ShowingGenerativeUIDone:
                    gameState = GameState.ShowingEndInstructions;
                    gameUICanvas.enabled = false;
                    DoGenerativePhase();
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
                
            
                if (new int[] {0, 2, 5, 8}.Contains(generativeSection))
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
                    gameState = GameState.GenerativePhaseDone;
                    playingClipNumber = 0;
                    videoPlayer.clip = videoClips[playingClipNumber];
                    videoPlayer.Play();
                    videoCanvas.gameObject.SetActive(true);
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
        gameState = GameState.FadingInDeck;
        StartCoroutine(FadeInDeck());
        AkSoundEngine.PostEvent("MenuAmbienceStop", this.gameObject);
        readingStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncUserCue, CallbackFunction);
        cardSelectionCanvas.enabled = false;
        defineSpreadCanvas.enabled = false;
        generativeCanvas.enabled = true;
        readingCanvas.enabled = true;
        gameUICanvas.enabled = true;
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
        }
    }
    // instantiate card
    // populate it with specified card data
    public static TarotCardData GetCardData(int cardOrder)
    {
        return System.Array.Find(GameMaster.Instance.cardsData, delegate (TarotCardData cd) { return cd.order == cardOrder; });
    }
    void DealCard()
    {
        TarotCard card = Instantiate(tarotCardPrefab).GetComponent<TarotCard>();
        dealtCards.Add(card);
        if (numberCardsAlreadyDealt < cardsSelectedToDeal.Count)
        {
            int cardOrder = cardsSelectedToDeal[numberCardsAlreadyDealt];
            TarotCardData cardData = GetCardData(cardOrder);
            card.Init(cardData, false);
            videoClips[setClipNumber] = cardData.cardAndGroupClips[setClipNumber - 1];
            videoClips[setClipNumber] = cardData.cardAndGroupClips[setClipNumber - 1];
            Debug.Log("Card Clip " + setClipNumber + " chosen.");
            setClipNumber++;
            videoClips[setClipNumber] = cardData.cardAndGroupClips[setClipNumber - 1];
            Debug.Log("Group Clip " + setClipNumber + " chosen.");
            setClipNumber++;


            if (numberCardsAlreadyDealt == 0)
            {
                groupMelodyEvents[0] = card1MelodyEvents[card.cardData.thematicGroup - 1];
                int s = (int)card.cardData.suit;
                cardSuitMelodyEvents[numberCardsAlreadyDealt] = card1SuitEvents[s];
            }

            else if(numberCardsAlreadyDealt == 1)
            {
                groupMelodyEvents[1] = card2MelodyEvents[card.cardData.thematicGroup - 1];
                int s = (int)card.cardData.suit;
                cardSuitMelodyEvents[numberCardsAlreadyDealt] = card2SuitEvents[s];

            }
            else if (numberCardsAlreadyDealt == 2)
            {
                groupMelodyEvents[2] = card3MelodyEvents[card.cardData.thematicGroup - 1];
                int s = (int)card.cardData.suit;
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
            StartCoroutine(DoCardDealingMovement(card));
            cardSlideSFX.Post(gameObject);
        }
        else
        {
            Debug.LogError("More cards dealt than exist in data set. This shouldn't ever happen.");
        }
        if (card.cardData.suit == CardSuit.MajorArcana)
        {
            majorArcanaTotal++;
        }
        else if (card.cardData.suit == CardSuit.Cups)
        {
            cupsTotal++;
        }
        else if (card.cardData.suit == CardSuit.Wands)
        {
            wandsTotal++;
        }
        else if (card.cardData.suit == CardSuit.Swords)
        {
            swordsTotal++;
        }

        else if (card.cardData.suit == CardSuit.Pentacles)
        {
            pentaclesTotal++;
        }

    }

    public void ChooseGameMode()
    {
        gameState = GameState.ChooseGameMode;
        demoInstructionsCanvas.enabled = true;
    }

    IEnumerator FadeInDeck()
    //this shouldn't work but it does
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
        Cursor.SetCursor(waitCursor, Vector2.zero, CursorMode.Auto);
        Coroutine lastCardFadeInCoroutine = null; // this is stupid
        foreach (DeckCard card in cardsInDeck)
        {
            lastCardFadeInCoroutine = StartCoroutine(card.FadeIn());
            gameState = GameState.ReadyToDeal;
        }
        yield return lastCardFadeInCoroutine; // thiS ISS TUPID
        enableButton = true;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        gameUICanvas.enabled = true;
        setClipNumber = 1;
    }

    void FadeOutDeck()
    {
        foreach (DeckCard card in cardsInDeck)
        {
            StartCoroutine(card.FadeOut());
        }
    }

    IEnumerator DoCardDealingMovement(TarotCard dealtCard)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / cardDealSpeed;
            dealtCard.transform.position = Vector3.Slerp(
                transform.position,         // location of deck
                dealtCardLocations[dealtCards.Count - 1].position,  // next card deal location
                t
            );
            yield return null;
        }
        gameState = GameState.ReadyToDeal;
        if (dealtCards.Count == dealtCardLocations.Length)
        {
            gameState = GameState.DealingDone;
        }
    }

    IEnumerator FlipCard()
    {
        if (numCardsAlreadyRead == 0)
        {
            FadeOutDeck();
        }
        float t = 0;
        float yRot = 0;
        TarotCard card = dealtCards[numCardsAlreadyRead];
        int groupNumber = card.cardData.thematicGroup;
        Color sparkColor = groupSparkColors[groupNumber - 1];
        Debug.Log(groupNumber + " " + sparkColor);
        ParticleSystem.MainModule ma = sparks[numCardsAlreadyRead].main;
        ma.startColor = sparkColor;
        sparks[numCardsAlreadyRead].gameObject.SetActive(true);



        while (t < 1)
        {
            t += Time.deltaTime / cardFlipSpeed;
            yRot = Mathf.Lerp(
                0f,
                -180f,
                t
            );
            card.transform.eulerAngles = new Vector3(
                card.transform.eulerAngles.x,
                yRot,
                card.transform.eulerAngles.z
            );
            yield return null;
        }

        gameState = GameState.FlippingCardDone;
    }

    IEnumerator ReadCard()
    {
        gameState = GameState.ReadingCard;
        TarotCard card = dealtCards[numCardsAlreadyRead];
        card.cardData.readingMusicEvent.Post(gameObject);
        readingUI.Init(dealtCards[numCardsAlreadyRead]);
        yield return StartCoroutine(readingUI.FadeIn());
        yield return StartCoroutine(readingUI.ReadCard());
        while (readingUI.reading)
        {
            yield return null;
        }
        numCardsAlreadyRead++;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        gameState = GameState.ReadingCardDone;
    }

    IEnumerator FadeOutReading()
    {
        enableButton = false;
        CardReadingUI readingUI = readingCanvas.GetComponent<CardReadingUI>();
        yield return StartCoroutine(readingUI.FadeOut());
        enableButton = true;
        if (numCardsAlreadyRead < dealtCards.Count)
        {
            gameState = GameState.FadingOutCardDone;
        }
        else
        {
            gameState = GameState.BeginGenerativeUI;
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

        foreach (TarotCard card in dealtCards)
        {
            StartCoroutine(card.FadeOut());
            
            foreach (ParticleSystem ps in sparks)
            {
                float GetColorFromCardOrder(int order)
                {
                    return (order < 7 ? ((order + 78) * 3) / 255f : (order * 3) / 255f);
                }
                Color particleColor = new Color32();
                particleColor.r = GetColorFromCardOrder(dealtCards[0].cardData.order);
                particleColor.g = GetColorFromCardOrder(dealtCards[1].cardData.order);
                particleColor.b = GetColorFromCardOrder(dealtCards[2].cardData.order);
                particleColor.a = 255;
                ParticleSystem.MainModule ma = ps.main;
                ma.startColor = particleColor;
                ps.Play();
            }
        }

       
        {
            
        }
        makingSongState.Post(gameObject);
        Cursor.SetCursor(waitCursor, Vector2.zero, CursorMode.Auto);
        yield return StartCoroutine(generativeUI.FadeIn());
        yield return StartCoroutine(generativeUI.ReadText());
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        gameState = GameState.ShowingGenerativeUIDone;
    }

    void DoGenerativePhase()
    {
        playingClipNumber = 0;
        enableButton = true;
        GenerativeUI generativeUI = generativeCanvas.GetComponent<GenerativeUI>();
        generativeState.Post(gameObject);
        for (int i = 0; i < 3; i++)
        {
            AK.Wwise.Event keyEvent = (dealtCards[i].cardData.cardKeyArray[i]);
            keyEvent.Post(gameObject);
            Debug.Log(i + " " + keyEvent);
        }
        StartCoroutine(generativeUI.DoGeneration(dealtCards));
        demoEndCanvas.enabled = true;
    }

    public void AfterInstructions_RandomReading()
    {
        gameMode = GameMode.Random;
        gameState = GameState.DefineSpread;
        demoInstructionsCanvas.enabled = false;
        defineSpreadCanvas.enabled = true;
    }

    public void AfterInstructions_ChooseCards()
    {
        gameMode = GameMode.Choose;
        gameState = GameState.DefineSpread;
        demoInstructionsCanvas.enabled = false;
        defineSpreadCanvas.enabled = true;
    }


    public void AfterInstructions_ViewPreviousReadingss()
    {
        gameState = GameState.DefineSpread;
        demoInstructionsCanvas.enabled = false;
        savedReadingsCanvas.enabled = true;
        Debug.Log("initing");
        savedReadingsUI.Init(this);
    }


    public void CloseViewPreviousReadings()
    {
        demoInstructionsCanvas.enabled = true;
        savedReadingsCanvas.enabled = false;
    }

    public void RevisitSavedReading(SavedReading reading)
    {
        Debug.Log("revisiting saved readings");
        gameMode = GameMode.ViewPreviousReadings;
        ResetGameState();
        savedReadingsCanvas.enabled = false;
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
                gameState = GameState.FadingInDeck;
                AkSoundEngine.PostEvent("MenuAmbienceStop", this.gameObject);
                readingStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncUserCue, CallbackFunction);
                generativeCanvas.enabled = true;
                readingCanvas.enabled = true;
                gameUICanvas.enabled = true;
                defineSpreadCanvas.enabled = false;
                ResetGameState();
                ChooseAllCardsRandomly();
                StartCoroutine(FadeInDeck());
                break;
            case GameMode.ViewPreviousReadings:
                gameState = GameState.FadingInDeck;
                AkSoundEngine.PostEvent("MenuAmbienceStop", this.gameObject);
                readingStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncUserCue, CallbackFunction);
                generativeCanvas.enabled = true;
                readingCanvas.enabled = true;
                gameUICanvas.enabled = true;
                defineSpreadCanvas.enabled = false;
                StartCoroutine(FadeInDeck());
                break;
            case GameMode.Choose:
            default:
                gameState = GameState.ChoosingCards;
                cardSelectionCanvas.enabled = true;
                cardSelectionUI.Init(GameMaster.Instance.cardsData, this);
                demoInstructionsCanvas.enabled = false;
                defineSpreadCanvas.enabled = false;
                ResetGameState();
                break;
        }
    }
    public void DisableEndCanvas()
    {
        gameState = GameState.GenerativePhase;
        gameUICanvas.enabled = true;
        demoEndCanvas.enabled = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    public void StartOver()
    {
        readingState.Post(gameObject);
        gameState = GameState.FadingInDeck;
        ClearGameState();
        ResetGameState();
    }

    public void ButtonHoverOn()
    {
        buttonHover = true;
    }

    public void ButtonHoverOff()
    {
        buttonHover = false;
    }


}
