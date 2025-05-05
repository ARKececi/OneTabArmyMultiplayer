using System.Collections.Generic;
using BotSystem;
using Fusion;
using SpawnSystem;
using UnityEngine;

namespace PlayerSystem.Controller
{
    public class MoveAndAligmentController : NetworkBehaviour
    {
        #region Self Variables

        #region SerializeField Variables

        [SerializeField]private GameObject _aligment;
        [SerializeField] private float Timer;
        
        #endregion

        #region Private Variables

        [SerializeField] private List<BotManager> moveNpcList = new List<BotManager>();
        [SerializeField] private List<BotManager> spawnNpcList = new List<BotManager>();
        
        private List<NetworkObject> spawnNpc = new();
        private float _timer;

        private SpawnController SpawnController;
        
        #endregion

        #endregion
        
        private void TimerClass()
        {
            _timer -= Runner.DeltaTime;
            if (_timer <= 0)
            {
                RPC_SpawnObject(NPCPrefabEnum.Soldier);
                _timer = Timer;
            }
        }

        public override void Spawned()
        {
            
        }

        public void Awake()
        {
            SpawnController = GetComponent<SpawnController>(); // veya doğrudan referansla bağla
            _timer = Timer;
        }
        
        public override void FixedUpdateNetwork()        
        {
            if (!HasStateAuthority) return; // sadece server timer kontrolü yapar
            TimerClass();
        }  
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_MoveObject(Vector3 move)
        {
            if (Runner == null)
            {
                Debug.LogError("Runner is null, cannot spawn.");
                return;
            }

            foreach (var VARIABLE in spawnNpcList)
            {
                moveNpcList.Add(VARIABLE);
            }
            if(spawnNpcList.Count > 0) spawnNpcList.Clear();
            
            moveNpcList.TrimExcess();
            spawnNpcList.TrimExcess();

            AlignHitdBots(move, moveNpcList, 7, 1f, 1f);
        }
        
        
        public void RPC_SpawnObject(NPCPrefabEnum npcPrefabEnum)
        {
            // Server'da çalışacak, Runner.Spawn burada çağrılmalı
            var npc = SpawnController.OnSpawn(transform.position, npcPrefabEnum);
            Debug.Log(npc);
            if (npc == null) return;
            var botManager = npc.GetComponent<BotManager>();
            spawnNpc.Add(npc);
            AlignSpawnedBots(botManager, 7, 1f,1f);
        }
        
        #region Move Positioning
        
        /// <summary>
        /// Spawn edilen botları belirli bir düzen içinde hizalar.
        /// </summary>
        public void AlignHitdBots(Vector3 spawnPoint, List<BotManager> bots, int rowCount, float xSpacing, float zSpacing)
        {
            if (bots == null || bots.Count == 0) return;

            int columnTotal = Mathf.CeilToInt((float)bots.Count / rowCount); // Kaç sütun olacağını belirle

            // Botların kapladığı toplam alanı hesapla
            float totalWidth = (Mathf.Min(columnTotal, bots.Count) - 1) * xSpacing;
            float totalDepth = (Mathf.Min(rowCount, Mathf.CeilToInt(bots.Count / (float)columnTotal)) - 1) * zSpacing;

            // **ORTA NOKTAYI BELİRLE**
            int centerIndex = bots.Count / 2;
            int centerRow = centerIndex / rowCount;
            int centerCol = centerIndex % rowCount;
    
            Vector3 centerOffset = new Vector3(centerCol * xSpacing, 0, centerRow * zSpacing);
    
            // Yeni başlangıç pozisyonu, ortanca botun tam tıklanan noktaya denk gelmesi için kaydırılmış olacak.
            Vector3 startPos = spawnPoint - centerOffset;

            for (int i = 0; i < bots.Count; i++)
            {
                int row = i / rowCount; // Önce Z ekseninde sırala
                int col = i % rowCount; // Sonra X eksenine göre kaydır

                // Botun yeni pozisyonu
                Vector3 newPos = startPos + new Vector3(col * xSpacing, 0, row * zSpacing);
                bots[i].OnHitTarget(newPos);
            }
        }

        #endregion
        
        /// <summary>
        /// Spawn edilen botları _alignment objesi etrafında hizalar.
        /// </summary>
        public void AlignSpawnedBots(BotManager bot, int rowCount, float xSpacing, float zSpacing)
        {
            if (_aligment == null)
            {
                Debug.LogError("Alignment GameObject is not assigned!");
                return;
            }

            spawnNpcList.Add(bot); // Yeni botu listeye ekle

            int botIndex = spawnNpcList.Count - 1; // Yeni eklenen botun indexi
            int row = botIndex / rowCount; // Kaçıncı satırda olduğunu bul
            int col = botIndex % rowCount; // Kaçıncı sütunda olduğunu bul

            Vector3 basePosition = _aligment.transform.position; // Hizalama merkez noktası
    
            // X ekseni pozisyonu (merkezden yayılma)
            float xOffset = (col - (rowCount - 1) / 2.0f) * xSpacing;
    
            // Z ekseni pozisyonu (satır sayısına göre kaydırma)
            float zOffset = -row * zSpacing;

            // Yeni pozisyonu hesapla
            Vector3 newPos = basePosition + new Vector3(xOffset, 0, zOffset);

            // Botu belirlenen noktaya gönder
            bot.OnHitTarget(newPos);
        }
        
        public void Reset()
        {
            foreach (var VARIABLE in spawnNpc)
            {
                Runner.Despawn(VARIABLE);
            }
        }
    }
}