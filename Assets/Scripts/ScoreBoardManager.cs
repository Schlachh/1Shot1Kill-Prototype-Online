using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardManager : MonoBehaviour
{
    public List<PlayerScoreCard> scoreCards = new List<PlayerScoreCard>(10);
    private void OnEnable()
    {
        GameMaster.instance.tempPlayers = GameMaster.instance.SortTempList(GameMaster.instance.tempPlayers);
        for (int i = 0; i < scoreCards.Count; i++)
        {
            scoreCards[i].playerName.text = GameMaster.instance.tempPlayers[i].playerName;
            scoreCards[i].ammoCount.text = "Score | " + GameMaster.instance.tempPlayers[i].ammoCount.ToString();
        }
    }
}
