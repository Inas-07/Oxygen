using TMPro;
using System;
using UnityEngine;
using Oxygen.Utils;
using Oxygen.Config;
using Microsoft.Extensions.Logging.Abstractions;
using GTFO.API;
using Il2CppInterop.Runtime.Injection;

namespace Oxygen.Components
{
    public class AirBar : MonoBehaviour
    {
        public static AirBar Current = null;

        private TextMeshPro m_airText = null; // air percentage %
        private TextMeshPro m_airTextLocalization = null;
        private float m_airTextX = 0f;
        private float m_airTextY = 0f;
        private float m_airTextZ = 0f;

        private RectTransform m_air1 = null;
        private RectTransform m_air2 = null;
        private SpriteRenderer m_airBar1 = null;
        private SpriteRenderer m_airBar2 = null;
        
        private float m_airWidth = 100.0f;
        private float m_barHeightMin = 3f;
        private float m_barHeightMax = 9f;
        //private bool m_airWarningLoop = false;

        private Color m_airLow = new Color(0, 0.5f, 0.5f);
        private Color m_airHigh = new Color(0.0f, 0.3f, 0.8f);

        public AirBar(IntPtr value) : base(value) { }

        public static void Setup()
        {
            var bar = Current;
            if (bar != null) return;

            bar = GuiManager.Current?.m_playerLayer?.m_playerStatus?.gameObject.AddComponent<AirBar>();
            if(bar != null)
            {
                LevelAPI.OnEnterLevel += bar.SetupAirBar;
                LevelAPI.OnLevelCleanup += bar.OnLevelCleanup;

                if(GameStateManager.CurrentStateName == eGameStateName.ExpeditionFail) // checkpoint restore
                {
                    bar.SetupAirBar();
                }
            }
        }

        internal void SetupAirBar()
        {
            // Instantiate air bar and text. 
            // Only need to instanitiate once.
            if (m_airText == null)
            {
                m_airText = GuiManager.Current.m_playerLayer.m_playerStatus.m_healthText.gameObject.Instantiate<TextMeshPro>("AirText");
                m_airText.fontSize /= 1.25f;
                m_airText.transform.Translate(0, -30f, 0);
            }

            if (m_airTextLocalization == null)
            {
                m_airTextLocalization = GuiManager.Current.m_playerLayer.m_playerStatus.m_pulseText.gameObject.Instantiate<TextMeshPro>("AirText Localization");
                m_airTextLocalization.enabled = true;
                //m_airTextLocalization.fontSize /= 1.5f;
                m_airTextLocalization.transform.Translate(300.0f - m_airWidth, -45.0f, 0);
                m_airTextX = m_airTextLocalization.transform.position.x;
                m_airTextY = m_airTextLocalization.transform.position.y;
                m_airTextZ = m_airTextLocalization.transform.position.z;

                for (int i = 0; i < m_airTextLocalization.gameObject.transform.childCount; i++)
                {
                    m_airTextLocalization.gameObject.transform.GetChild(i).gameObject.SetActive(false);
                }
            }

            // right air bars
            if (m_air1 == null)
            {
                m_air1 = GuiManager.Current.m_playerLayer.m_playerStatus.m_health1.gameObject.transform.parent.gameObject
                    .Instantiate<RectTransform>("AirFill Right");

                m_air1.transform.Translate(0, -30f, 0);

                SpriteRenderer b1 = m_air1.GetChild(0).GetComponent<SpriteRenderer>();
                b1.size = new Vector2(m_airWidth, b1.size.y);

                // Remove yellow damage bars
                m_airBar1 = m_air1.GetChild(1).GetComponent<SpriteRenderer>();
                m_air1.GetChild(2).GetComponent<SpriteRenderer>().enabled = false;
            }

            // left air bar
            if (m_air2 == null)
            {
                m_air2 = GuiManager.Current.m_playerLayer.m_playerStatus.m_health2.gameObject.transform.parent.gameObject
                    .Instantiate<RectTransform>("AirFill Left");

                // Move air bar down
                m_air2.transform.Translate(0, 30f, 0);

                SpriteRenderer b2 = m_air2.GetChild(0).GetComponent<SpriteRenderer>();
                b2.size = new Vector2(m_airWidth, b2.size.y);

                // Remove yellow damage bars
                m_airBar2 = m_air2.GetChild(1).GetComponent<SpriteRenderer>();
                m_air2.GetChild(2).GetComponent<SpriteRenderer>().enabled = false;
            }

            // Initialize Bar
            UpdateAirBar(1f);
            UpdateAirText(AirManager.Current?.Config ?? null);
            SetVisible(false);
        }

        public void UpdateAirBar(float air)
        {
            SetAirPercentageText(air);
            SetAirBar(m_airBar1, air); 
            SetAirBar(m_airBar2, air);
        }
        
        // Set bar length and color
        private void SetAirBar(SpriteRenderer bar, float val)
        {
            if (bar == null) return;
            bar.size = new Vector2(val * m_airWidth, Mathf.Lerp(this.m_barHeightMin, this.m_barHeightMax, val));
            bar.color = Color.Lerp(m_airLow, m_airHigh, val);
        }

        // Set air text and color
        private void SetAirPercentageText(float val)
        {
            Color color = Color.Lerp(m_airLow, m_airHigh, val);
            if (m_airText != null)
            {
                m_airText.text = "O<size=75%>2</size>";
                m_airText.color = color;
                m_airText.ForceMeshUpdate(true);
            }

            if(m_airTextLocalization != null)
            {
                m_airTextLocalization.color = color;
                m_airTextLocalization.ForceMeshUpdate(true);
            }
        }

        public void UpdateAirText(OxygenBlock config)
        {
            if (config == null || m_airTextLocalization == null) return;
            
            string text = config.AirText?.Text ?? string.Empty;
            float x = config.AirText.x;
            float y = config.AirText.y;

            m_airTextLocalization.text = text; 
            m_airTextLocalization.ForceMeshUpdate(true);
            CoroutineManager.BlinkIn(m_airTextLocalization.gameObject);
            m_airTextLocalization.transform.SetPositionAndRotation(new(m_airTextX + x, m_airTextY + y, m_airTextZ), m_airTextLocalization.transform.rotation);
        }

        // Set visibility of air bar
        public void SetVisible(bool vis)
        {
            m_airText?.gameObject.SetActive(vis);
            m_airTextLocalization?.gameObject.SetActive(vis);
            m_air1?.gameObject.SetActive(vis);
            m_air2?.gameObject.SetActive(vis);
        }

        private void OnLevelCleanup()
        {
            SetVisible(false);
        }

        private void OnDestroy()
        {
            LevelAPI.OnEnterLevel -= SetupAirBar;
            LevelAPI.OnLevelCleanup -= OnLevelCleanup;

            if (m_airText != null)
            {
                GameObject.Destroy(m_airText.gameObject);
                m_airText = null;
            }
            if (m_airTextLocalization != null)
            {
                GameObject.Destroy(m_airTextLocalization.gameObject);
                m_airTextLocalization = null;
            }
            if (m_air1 != null)
            {
                GameObject.Destroy(m_air1.gameObject);
                m_air1 = null;
                m_airBar1 = null;
            }
            if (m_air2 != null)
            {
                GameObject.Destroy(m_air2.gameObject);
                m_air2 = null;
                m_airBar2 = null;
            }
        }

        public static AirBar? Current => GuiManager.Current?.m_playerLayer?.m_playerStatus?.gameObject.GetComponent<AirBar>() ?? null;
    
        static AirBar()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AirBar>();
        }
    }
}