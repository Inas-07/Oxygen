using TMPro;
using System;
using UnityEngine;
using Oxygen.Utils;

namespace Oxygen.Components
{
    public class AirBar : MonoBehaviour
    {
        public static AirBar Current;
        public TextMeshPro m_airText;
        public RectTransform m_air1;
        public RectTransform m_air2;
        public SpriteRenderer m_airBar1;
        public SpriteRenderer m_airBar2;
        
        private float m_airWidth = 100f;
        private float m_barHeightMin = 3f;
        private float m_barHeightMax = 9f;

        private Color m_airLow = new Color(0, 0.5f, 0.5f);
        private Color m_airHigh = new Color(0.1f, 0.1f, 0.7f);

        public AirBar(IntPtr value) : base(value) { }

        public static void Setup()
        {
            AirBar.Current = GuiManager.Current.m_playerLayer.m_playerStatus.gameObject.AddComponent<AirBar>();
            Current.OnExpeditionStarted();
        }
        
        void OnExpeditionStarted()
        {
            // Instantiate air bar and text
            m_airText = GuiManager.Current.m_playerLayer.m_playerStatus.m_healthText.gameObject.Instantiate<TextMeshPro>("AirText");
            m_air1 =
                GuiManager.Current.m_playerLayer.m_playerStatus.m_health1.gameObject.transform.parent.gameObject
                    .Instantiate<RectTransform>("AirFill Right");
            m_air2 =
                GuiManager.Current.m_playerLayer.m_playerStatus.m_health2.gameObject.transform.parent.gameObject
                    .Instantiate<RectTransform>("AirFill Left");

            // Move air bar down
            m_airText.fontSize /= 2.0f;
            m_airText.transform.Translate(0, 30f, 0);
            m_air1.transform.Translate(0, 30f, 0);
            m_air2.transform.Translate(0, 90f, 0);
            
            // Get left & right air bars
            m_airBar1 = m_air1.GetChild(1).GetComponent<SpriteRenderer>();
            m_airBar2 = m_air2.GetChild(1).GetComponent<SpriteRenderer>();

            
            // Remove yellow damage bars
            //m_air1.GetChild(2).GetComponent<SpriteRenderer>().enabled = false;
            //m_air2.GetChild(2).GetComponent<SpriteRenderer>().enabled = false;

            // Initialize Bar
            UpdateAirBar(1f);
            
            // Hide air bar
            SetVisible(false);
        }

        public void UpdateAirBar(float air)
        {
            SetAirText(m_airText, air);
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
        private void SetAirText(TextMeshPro text, float val)
        {
            text.text = (val * 100f).ToString("N0") + "%";
            text.color = Color.Lerp(this.m_airLow, this.m_airHigh, val);
            text.ForceMeshUpdate(true);
        }
        
        // Set visibility of air bar
        public void SetVisible(bool vis)
        {
            m_airText.gameObject.SetActive(vis);
            m_air1.gameObject.SetActive(vis);
            m_air2.gameObject.SetActive(vis);
        }
    }
}