
using System.Linq;
using UnityEngine;

// Singleton class used to store game-wide info

public class GameMaster : Singleton<GameMaster>
{
    TarotCardData[] _cardsData;
    public TarotCardData[] cardsData
    {
        get
        {
            if (_cardsData == null)
            {
                _cardsData = Resources.LoadAll("CardData", typeof(TarotCardData)).Cast<TarotCardData>().ToArray();
                System.Array.Sort(_cardsData, delegate (TarotCardData t1, TarotCardData t2) { return (t1.order - t2.order); });
            }
            return _cardsData;
        }
    }
}