using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using DG.Tweening;
using PlayerSystem;
using Signals;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem.WhoSpawnController
{
    public class CardMoveController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        
        [SerializeField] private Transform cardHolder; // Kartların yerleşeceği UI alanı

        #endregion

        #region Private Variables
        
        private SerializedDictionary<CardType, CardData> cardDictionary; // Her kart türü için farklı seviyelerde kartları tutan sözlük
        [SerializeField]private SerializedDictionary<CardType, MoveCardData> selectedCards = new SerializedDictionary<CardType, MoveCardData>(); // seçilen kartlar

        private bool firstPlay;

        #endregion

        #endregion

        private void Awake()
        {
            foreach (CardType card in Enum.GetValues(typeof(CardType)))
            {
                selectedCards.Add(card, new MoveCardData());
            }

            OnGetData();
        }
        
        private void OnGetData()
        {
            cardDictionary = Resources.Load<CD_Card>("Data/CD_Card").CardData; 
        }
        
        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            UISignals.Instance.onSpawnCard += ShowCardSelection;
        }

        private void UnsubscribeEvents()
        {
            UISignals.Instance.onSpawnCard -= ShowCardSelection;
        }
    
        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void ShowCardSelection(CardType cardType, int lwl)
        {
            if(cardType == CardType.Castle) return;
            // Önceki kartları temizle
            if(selectedCards == null) return;
            Debug.Log(cardType);
            // Kartların düşüş animasyonu için başlangıç değerleri
            float startY = 2000f; // Kartlar başlangıçta yukarıdan gelecek
            float delayBetweenCards = 0.5f; // Kartların sırayla düşmesi için gecikme süresi
            
            // Yeni kartı oluştur ve holder içine yerleştir
            GameObject cardPrefabInstance = Instantiate(cardDictionary[cardType].List[lwl - 1], cardHolder);
            
            if(selectedCards[cardType].Card == null)
                selectedCards[cardType].Card = cardPrefabInstance;
            else
            {
                var vec3 = selectedCards[cardType].Card.transform.position;
                Destroy(selectedCards[cardType].Card);
                selectedCards[cardType].Card = cardPrefabInstance;
                selectedCards[cardType].Card.transform.position = vec3;
            }
            selectedCards[cardType].Lwl = lwl;
            foreach (var VARIABLE in selectedCards.Keys)
            {
                if (selectedCards[VARIABLE].Active)
                {
                    var transform = selectedCards[VARIABLE].Card.transform;
                    transform.DOMoveY(transform.position.y, 0.5f).SetEase(Ease.OutQuad);
                    PlayerSignals.Instance.onSpawnEnum?.Invoke(cardType,selectedCards[cardType].Lwl);
                }
            }
            
            // Kartın butonuna tıklanınca çağrılacak fonksiyonu ata
            
            Button button = cardPrefabInstance.GetComponent<Button>();
            button.onClick.AddListener(() => SpawnCard(cardType));
            if(firstPlay) return;
            SpawnCard(cardType);
            firstPlay = true;
        }

        private void SpawnCard(CardType cardType)
        {
            if(selectedCards[cardType].Active) return;
            foreach (var VARIABLE in selectedCards.Keys)
            {
                if(!selectedCards[VARIABLE].Active) continue;
                Transform downcard = selectedCards[VARIABLE].Card.transform;
                selectedCards[VARIABLE].Active = false;
                downcard.DOMoveY(downcard.position.y - 40f, 0.5f).SetEase(Ease.OutQuad);
            }
            
            selectedCards[cardType].Active = true;
            Transform upcard = selectedCards[cardType].Card.transform;
            upcard.DOMoveY(upcard.position.y + 40f, 0.5f).SetEase(Ease.OutQuad);
            PlayerSignals.Instance.onMoveEnum?.Invoke(cardType);
        }
    }

    public class MoveCardData
    {
        public GameObject Card;
        public int Lwl;
        public bool Active;
    }
}