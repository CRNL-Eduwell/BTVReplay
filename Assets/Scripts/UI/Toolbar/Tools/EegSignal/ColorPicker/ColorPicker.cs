using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void ColorChanged(Color c);

    public class ColorPicker : Tool
    {
        public event ColorChanged UpdateColor;

        #region Ui Members
        [SerializeField]
        private Image m_PreviewColor = null;
        [SerializeField]
        private ColorSlider m_RedSlider = null;
        [SerializeField]
        private ColorSlider m_GreenSlider = null;
        [SerializeField]
        private ColorSlider m_BlueSlider = null;
        [SerializeField]
        private ColorSlider m_AlphaSlider = null;
        [SerializeField]
        private RawImage m_TextureRawImage = null;
        [SerializeField]
        private Button[] m_ColorPresets = new Button[5];
        #endregion

        private Texture2D m_TexturePicker = null;
        private Color[] m_DataColorPicker;

        public override void Initialize()
        {
            //Init Sliders
            m_RedSlider.Init();
            m_GreenSlider.Init();
            m_BlueSlider.Init();
            m_AlphaSlider.Init();

            m_RedSlider.sliderColorChange += new colorSliderChanged(UpdateColorFromSliders);
            m_GreenSlider.sliderColorChange += new colorSliderChanged(UpdateColorFromSliders);
            m_BlueSlider.sliderColorChange += new colorSliderChanged(UpdateColorFromSliders);
            m_AlphaSlider.sliderColorChange += new colorSliderChanged(UpdateColorFromSliders);

            //Init TexturePicker
            m_TexturePicker = ((Texture2D)m_TextureRawImage.texture);
            m_DataColorPicker = m_TexturePicker.GetPixels();

            //Init PresetButtons
            for (int i = 0; i < 5; i++)
                ConnectPresetsColor(i);

            //Get Initial Color
            m_PreviewColor.color = GetColorFromSliders();
        }

        public void SetColorWithoutNotify(Color color)
        {
            m_PreviewColor.color = color;

            m_RedSlider.ColorValue = color.r;
            m_GreenSlider.ColorValue = color.g;
            m_BlueSlider.ColorValue = color.b;
            m_AlphaSlider.ColorValue = color.a;
        }

        //1--2
        //|  |
        //0--3
        public void UpdateColorOnClick()
        {
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //Mouse Position to world coordinates
            Vector3[] colorWorldPos = new Vector3[4];
            transform.GetChild(4).transform.GetComponent<RectTransform>().GetWorldCorners(colorWorldPos);

            int pixelIDX = (int)(((screenPos.x - colorWorldPos[1].x) / (colorWorldPos[2].x - colorWorldPos[1].x)) * m_TexturePicker.width);
            int pixelIDY = (int)(((screenPos.y - colorWorldPos[1].y) / (colorWorldPos[3].y - colorWorldPos[2].y)) * m_TexturePicker.height);
            int pixelID = (m_TexturePicker.height - pixelIDY) * m_TexturePicker.width + pixelIDX;

            m_PreviewColor.color = GetColorFromPickerClick(pixelID);
            UpdateSlidersColor(m_PreviewColor.color);
        }

        private void OnDestroy()
        {
            m_RedSlider.sliderColorChange -= new colorSliderChanged(UpdateColorFromSliders);
            m_GreenSlider.sliderColorChange -= new colorSliderChanged(UpdateColorFromSliders);
            m_BlueSlider.sliderColorChange -= new colorSliderChanged(UpdateColorFromSliders);
            m_AlphaSlider.sliderColorChange -= new colorSliderChanged(UpdateColorFromSliders);
            for (int i = 0; i < 5; i++)
                m_ColorPresets[i].onClick.RemoveAllListeners();
        }

        private void UpdateColorFromSliders()
        {
            m_PreviewColor.color = GetColorFromSliders();
            UpdateColor(m_PreviewColor.color);
        }

        private void UpdateSlidersColor(Color NewColor)
        {
            m_RedSlider.ColorValue = NewColor.r;
            m_GreenSlider.ColorValue = NewColor.g;
            m_BlueSlider.ColorValue = NewColor.b;
            m_AlphaSlider.ColorValue = NewColor.a;

            UpdateColor(NewColor);
        }

        private Color GetColorFromPickerClick(int pixelID)
        {
            Color colorToChange;
            colorToChange.r = m_DataColorPicker[pixelID].r;
            colorToChange.g = m_DataColorPicker[pixelID].g;
            colorToChange.b = m_DataColorPicker[pixelID].b;
            colorToChange.a = m_DataColorPicker[pixelID].a;

            return colorToChange;
        }

        private Color GetColorFromSliders()
        {
            Color NewColor;

            NewColor.r = m_RedSlider.ColorValue;
            NewColor.g = m_GreenSlider.ColorValue;
            NewColor.b = m_BlueSlider.ColorValue;
            NewColor.a = m_AlphaSlider.ColorValue;
            return NewColor;
        }

        private void ConnectPresetsColor(int ID)
        {
            m_ColorPresets[ID].onClick.AddListener(() =>
            {
                Color currentColor = m_ColorPresets[ID].GetComponent<Image>().color;
                UpdateSlidersColor(currentColor);
            });
        }
    }
}
