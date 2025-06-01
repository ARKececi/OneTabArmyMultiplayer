using System;
using UnityEngine;

namespace Extentions.GameSystem
{
    public class GameManagerClient : MonoBehaviour
    {
        void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0; // VSync kapalı olmalı ki FPS kontrolü geçerli olsun
        }
    }
}