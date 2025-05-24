using System;
using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Extentions.GameSystem;
using PlayerSystem;
using Signals;

public class CardManager : MonoBehaviour
{
    [Header("Kart Verileri")]
    [SerializeField] private SerializedDictionary<CardType, CardData> cardDictionary; // Her kart türü için farklı seviyelerde kartları tutan sözlük
    [SerializeField] private SerializedDictionary<CardType, int> cardLevels = new SerializedDictionary<CardType, int>(); // Kartların seviyelerini saklayan sözlük

    [Header("UI Elemanları")]
    [SerializeField] private GameObject cardPanel; // Kart seçim ekranını temsil eden panel
    [SerializeField] private Transform cardHolder; // Kartların yerleşeceği UI alanı

    [Header("Durum Kontrolleri")]
    [SerializeField] private bool isCardSelectionActive = false; // Eğer true olursa kart seçim ekranı açılır
    
    [SerializeField] private Image fadeImageLight; // Hafif karartma için
    [SerializeField] private Image fadeImageDark;  // Tam karartma için

    private bool firsPlay;
    
    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        UISignals.Instance.onNextLevel += OnNextLevel;
    }

    private void UnsubscribeEvents()
    {
        UISignals.Instance.onNextLevel -= OnNextLevel;
    }
    
    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        OnNextLevel();
        // Başlangıçta tamamen görünmez yap
        fadeImageLight.color = new Color(0, 0, 0, 0);
        fadeImageDark.color = new Color(0, 0, 0, 0);
        
    }

    private void Awake()
    {
        firsPlay = true;
        foreach (CardType card in Enum.GetValues(typeof(CardType)))
        {
            cardLevels.Add(card,0);
        }
        OnGetData();
    }

    private void OnGetData()
    {
        cardDictionary = Resources.Load<CD_Card>("Data/CD_Card").CardData; 
    }
    
    private void OnNextLevel()
    {

        // Eğer kart seçimi aktifse, oyunu durdur ve seçim ekranını aç
        if (isCardSelectionActive)
        {
            ShowCardSelection(); // Kart seçim ekranını göster
            isCardSelectionActive = false; // Tekrar çağırmamak için flag'i kapat
        }
    }

    /// <summary>
    /// Kart seçim ekranını açar ve 3 rastgele kart gösterir
    /// </summary>
    public void ShowCardSelection()
    {
        cardPanel.SetActive(true); // Kart panelini aç
        List<GameObject> selectedCards = GetRandomUniqueCards(3); // 3 farklı türden kart seç
        // Önceki kartları temizle
        foreach (Transform child in cardHolder)
        {
            Destroy(child.gameObject);
        }

        // Kartların düşüş animasyonu için başlangıç değerleri
        float startY = 2000f; // Kartlar başlangıçta yukarıdan gelecek
        float delayBetweenCards = 0.5f; // Kartların sırayla düşmesi için gecikme süresi

        for (int i = 0; i < selectedCards.Count; i++)
        {
            // Yeni kartı oluştur ve holder içine yerleştir
            GameObject cardPrefabInstance = Instantiate(selectedCards[i], cardHolder);
            RectTransform rect = cardPrefabInstance.GetComponent<RectTransform>();
            
            // Kartın başlangıç pozisyonunu yukarıya ayarla
            Vector3 startPos = rect.anchoredPosition;
            rect.anchoredPosition = new Vector3(startPos.x, startY);

            // Kartı yumuşak bir şekilde aşağı indir
            rect.DOAnchorPosY(startPos.y, 0.5f).SetUpdate(true)
                .SetEase(Ease.OutBounce)
                .SetDelay(i * delayBetweenCards);
            
            // Kartın butonuna tıklanınca çağrılacak fonksiyonu ata
            
            Button button = cardPrefabInstance.GetComponent<Button>();
            CardType cardType = GetCardTypeFromPrefab(cardPrefabInstance);
            button.onClick.AddListener(() => SelectCard(cardType));
        }
        FadeToDarkBlack();
        FadeToLightBlack();
    }

    /// <summary>
    /// Belirtilen sayıda benzersiz kart seçer (her biri farklı türde olur)
    /// </summary>
    private List<GameObject> GetRandomUniqueCards(int count)
    {
        List<CardType> availableTypes = new List<CardType>(cardDictionary.Keys); // Mevcut kart türlerini al
        List<GameObject> selectedCards = new List<GameObject>();
        while (selectedCards.Count < count && availableTypes.Count > 0)
        {
            int randcount = availableTypes.Count;
            if (firsPlay)
            {
                randcount--;
                firsPlay = false;
            }
            int randomIndex = UnityEngine.Random.Range(0, randcount);
            CardType selectedType = availableTypes[randomIndex];

            availableTypes.RemoveAt(randomIndex); // Aynı türden bir daha seçmemesi için listeden çıkar
            int level = cardLevels[selectedType]; // Kartın mevcut seviyesini al
            int index = Mathf.Min(level, cardDictionary[selectedType].List.Count - 1);
            GameObject card = cardDictionary[selectedType].List[index]; // Seviye sınırını aşmaz
            selectedCards.Add(card);
        }

        return selectedCards;
    }

    /// <summary>
    /// Kullanıcı bir kart seçtiğinde çağrılır, kartın seviyesini artırır ve seçim ekranını kapatır
    /// </summary>
    private void SelectCard(CardType selectedType)
    {
        cardLevels[selectedType]++; // Kartın seviyesini artır
        cardPanel.SetActive(false); // Kart seçim ekranını kapat
        PlayerSignals.Instance.onSpawnEnum?.Invoke(selectedType,cardLevels[selectedType]);
        FadeFromLightBlack();
        FadeFromDarkBlack();
        isCardSelectionActive = true;
    }

    /// <summary>
    /// Kart nesnesinin hangi türe ait olduğunu belirler
    /// </summary>
    private CardType GetCardTypeFromPrefab(GameObject cardPrefab)
    {
        foreach (var Keys in cardDictionary.Keys)
        {
            foreach (var entry in cardDictionary[Keys].List)
            {
                if ($"{entry.name}(Clone)" == cardPrefab.name)
                {
                    return Keys;
                }
            }
        }
        return default;
    }
    
    /// <summary>
    /// Hafif karartma (alpha: 0.3)
    /// </summary>
    public void FadeToLightBlack(float duration = .5f)
    {
        fadeImageLight.DOFade(0.5f, duration).SetUpdate(true);
    }

    /// <summary>
    /// Hafif açılma
    /// </summary>
    public void FadeFromLightBlack(float duration = .5f)
    {
        fadeImageLight.DOFade(0, duration).SetUpdate(true);
    }

    /// <summary>
    /// Tam karartma (alpha: 1)
    /// </summary>
    public void FadeToDarkBlack(float duration = 1.5f)
    {
        fadeImageDark.DOFade(.7f, duration).SetUpdate(true);
    }

    /// <summary>
    /// Tam açılma
    /// </summary>
    public void FadeFromDarkBlack(float duration = 1.5f)
    {
        fadeImageDark.DOFade(0, duration).SetUpdate(true);
    }
}
