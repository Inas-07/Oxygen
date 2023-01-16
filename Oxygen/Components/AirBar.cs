using TMPro;
using System;
using UnityEngine;
using Oxygen.Utils;
using GameData;
using Localization;

namespace Oxygen.Components
{
    public class AirBar : MonoBehaviour
    {
        public static AirBar Current = null;
        //private static TextDataBlock airLocalization = null;
        private static readonly uint airLocalizationID = 2999;

        public TextMeshPro m_airText = null; // air percentage %
        //public TextMeshPro m_airTextLocalization = null; // TODO: add localization. ref monocode: PUI_LocalPlayerStatus.UpdateInfection
        
        public RectTransform m_air1 = null;
        public RectTransform m_air2 = null;
        public SpriteRenderer m_airBar1 = null;
        public SpriteRenderer m_airBar2 = null;
        
        private float m_airWidth = 100.0f;
        private float m_barHeightMin = 3f;
        private float m_barHeightMax = 9f;
        //private bool m_airWarningLoop = false;

        private Color m_airLow = new Color(0, 0.5f, 0.5f);
        private Color m_airHigh = new Color(0.0f, 0.1f, 0.8f);

        public AirBar(IntPtr value) : base(value) { }

        public static void Setup()
        {
            if(Current == null)
            {
                Current = GuiManager.Current.m_playerLayer.m_playerStatus.gameObject.AddComponent<AirBar>();
            }
            Current.OnExpeditionStarted();
        }
        
        void OnExpeditionStarted()
        {
            // Instantiate air bar and text
            if (m_airText == null)
            {
                m_airText = GuiManager.Current.m_playerLayer.m_playerStatus.m_healthText.gameObject.Instantiate<TextMeshPro>("AirText");
                m_airText.fontSize /= 1.25f;
                m_airText.transform.Translate(0, -30f, 0);
            }

            //if(m_airTextLocalization == null)
            //{
            //    m_airTextLocalization = GuiManager.Current.m_playerLayer.m_playerStatus.m_infectionText.gameObject.Instantiate<TextMeshPro>("AirText Localization");
            //    m_airTextLocalization.enabled = true;
            //    m_airTextLocalization.fontSize /= 1.5f;
            //    m_airTextLocalization.transform.Translate(300.0f - m_airWidth, -120f, 0);
            //}

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

            // Hide air bar
            if (AirManager.Current != null)
                SetVisible(AirManager.Current.AlwaysDisplayAirBar());
            else
                SetVisible(false);
        }

        public void UpdateAirBar(float air)
        {
            SetAirText(air);
            SetAirBar(m_airBar1, air); 
            SetAirBar(m_airBar2, air);
        }
        
        // Set bar length and color
        private void SetAirBar(SpriteRenderer bar, float val)
        {
            bar.size = new Vector2(val * m_airWidth, Mathf.Lerp(this.m_barHeightMin, this.m_barHeightMax, val));
            bar.color = Color.Lerp(m_airLow, m_airHigh, val);
        }

        // Set air text and color
        private void SetAirText(float val)
        {
            Color color = Color.Lerp(m_airLow, m_airHigh, val);

            m_airText.text = (val * 100f).ToString("N0") + "%";
            m_airText.color = color;
            m_airText.ForceMeshUpdate(true);

            //m_airTextLocalization.color = color;
            //m_airTextLocalization.text = Text.Get(airLocalizationID);
            //m_airTextLocalization.ForceMeshUpdate(true);
        }
        
        // Set visibility of air bar
        public void SetVisible(bool vis)
        {
            m_airText.gameObject.SetActive(vis);
            //m_airTextLocalization.gameObject.SetActive(vis);
            m_air1.gameObject.SetActive(vis);
            m_air2.gameObject.SetActive(vis);
        }
    }
}