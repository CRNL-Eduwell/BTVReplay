using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.ButtonsUI
{
    class ButtonUI_hide : ButtonUI
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Right:
                    if (m_positionCounter - 1 >= 0)
                        m_positionCounter -= 1;
                    break;
                case PointerEventData.InputButton.Left:
                    if (m_positionCounter + 1 <= 2)
                        m_positionCounter += 1;
                    break;
            }

            if (m_curve != null)
            {
                switch (m_positionCounter)
                {
                    case 0:
                        m_curve.SetActive(false); //ni courbe ni option
                        for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
                        {
                            if (i != m_idOpt)
                            {
                                m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                                m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                                m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                            }
                        }
                        changeColorOptions(true);
                        m_options.SetActive(m_isVisible);
                        break;
                    case 1:
                        m_curve.SetActive(true); //courbe et option

                        changeColorOptions(true);
                        m_options.SetActive(m_isVisible);
                        showExtraPanels(m_isVisible);
                        break;
                    case 2:
                        for (int i = 0; i < m_optionsPanel.transform.childCount; i++)
                        {
                            if (i != m_idOpt)
                            {
                                m_buttonsPanel.transform.GetChild(i).GetComponent<Image>().color = Color.black;
                                m_buttonsPanel.transform.GetChild(i).GetComponent<ButtonUI>().showExtraPanels(false);
                                m_optionsPanel.transform.GetChild(i).gameObject.SetActive(false);
                            }
                        }
                        changeColorOptions(false);
                        m_options.SetActive(m_isVisible);
                        showExtraPanels(m_isVisible);
                        break;
                }
            }
        }
    }
}
