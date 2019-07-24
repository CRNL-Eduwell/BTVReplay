using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.ButtonsUI
{
    class ButtonUI : MonoBehaviour, IPointerClickHandler
    {
        public GameObject optionsPanel
        {
            get
            {
                return m_options;
            }
        }
        public GameObject optionsPanel2
        {
            get
            {
                return m_options2Panel;
            }
        }
        //==
        protected GameObject m_buttonsPanel = null;
        protected GameObject m_optionsPanel = null;
        protected Image m_showPic = null;
        protected GameObject m_options = null;
        protected int m_idOpt = -2;
        //==
        protected GameObject m_curve = null;
        protected GameObject m_options2Panel = null;
        protected WindowOpt m_optionsWindow = null;
        protected int m_positionCounter = 1;
        //==
        protected bool m_isVisible = false;
        protected Color yellow = new Color(0.9058f, 0.8784f, 0.0f);
        protected Color orange = new Color(0.9058f, 0.5254f, 0.1921f);
        protected Color blue = new Color(0.6117f, 0.7058f, 0.7960f);
        protected Color blueHide = new Color(0.6117f, 0.7058f, 0.7960f, 0.3921f);

        public void init(GameObject p_buttonsPanel, GameObject p_optionsPanel, int p_idOpt)
        {
            m_buttonsPanel = p_buttonsPanel;
            m_optionsPanel = p_optionsPanel;
            m_idOpt = p_idOpt;

            m_showPic = p_buttonsPanel.transform.GetChild(m_idOpt).GetComponent<Image>();
            m_options = m_optionsPanel.transform.GetChild(m_idOpt).gameObject;
        }

        public void initExtraData(GameObject p_options2Panel)
        {
            m_options2Panel = p_options2Panel;
            m_optionsWindow = p_options2Panel.transform.parent.GetComponent<WindowOpt>();
        }

        public void initUIElement(string name)
        {
            m_curve = GameObject.Find(name);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
                {
                    if (i != m_idOpt)
                    {
                        m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                        m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                        m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                changeColorOptions(m_options.activeSelf);
                m_options.SetActive(m_isVisible);
                showExtraPanels(m_isVisible);
            }
        }

        protected void changeColorOptions(bool currentStatus)
        {
            m_isVisible = !currentStatus;

            if (m_isVisible)
                m_showPic.color = blue;
            else
                m_showPic.color = Color.black;
        }

        public virtual void showExtraPanels(bool show)
        {
            if (m_options2Panel != null)
            {
                m_optionsWindow.HideOptionPanel(show); //Full Option Panel
                m_options2Panel.SetActive(show); //Child to show or hide
                if (show == false)
                    m_positionCounter = 1;
                if (m_curve != null)
                {
                    if (m_curve.transform.GetComponent<Trace>() != null)
                    {
                        if (m_curve.transform.GetComponent<Trace>().hasFocus)
                            m_curve.transform.GetComponent<Trace>().manageFocusClick();
                    }
                }
            }
        }
    }
}
