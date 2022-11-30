using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

namespace UI.InGame
{
    public class ArenaUIManager : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _winnerCaption;
        [SerializeField] private TextMeshProUGUI _nextMatchTimeCounter;
        [SerializeField] private TextMeshProUGUI _nextMatchPreparationCounter;
    
        private readonly string _winnerString = "Победитель - ";
        private readonly string _nextMatchString = "Новый матч начнётся через ";

        [ClientRpc]
        public void RpcShowBetweenMatchesCountDown(float time)
        {
            StartCoroutine(BetweenMatchesCountDown(time));
        }
    
        [ClientRpc]
        public void RpcShowPreparationCountDown(float time)
        {
            StartCoroutine(PreparationCountDown(time));
        }
    
        [ClientRpc]
        public void RpcShowWinner(string winnerName)
        {
            _winnerCaption.gameObject.SetActive(true);
            _winnerCaption.text = _winnerString + winnerName;
        }
    
        private IEnumerator PreparationCountDown(float time)
        {
            float currentTime = time;
            _nextMatchPreparationCounter.gameObject.SetActive(true);
            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                _nextMatchPreparationCounter.text = ((int) currentTime + 1).ToString();
                yield return new WaitForEndOfFrame();
            }
            _nextMatchPreparationCounter.gameObject.SetActive(false);
        }
    
        private IEnumerator BetweenMatchesCountDown(float time)
        {
            float currentTime = time;
            _nextMatchTimeCounter.gameObject.SetActive(true);
            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                _nextMatchTimeCounter.text = _nextMatchString + ((int)currentTime + 1);
                yield return new WaitForEndOfFrame();
            }
            _winnerCaption.gameObject.SetActive(false);
            _nextMatchTimeCounter.gameObject.SetActive(false);
        }
    }
}
