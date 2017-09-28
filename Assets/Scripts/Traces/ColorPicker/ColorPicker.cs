using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void colorChanged(Color c);

public class ColorPicker : MonoBehaviour
{
    public event colorChanged changeColor;

    ColorSlider redSlider = null;
    ColorSlider greenSlider = null;
    ColorSlider blueSlider = null;
    ColorSlider alphaSlider = null;
    Texture2D colorPicker = null;
    Image colorPreview = null;
    Color[] dataColorPicker;

    void Awake()
    {
        redSlider = transform.GetChild(0).GetComponent<ColorSlider>();
        greenSlider = transform.GetChild(1).GetComponent<ColorSlider>();
        blueSlider = transform.GetChild(2).GetComponent<ColorSlider>();
        alphaSlider = transform.GetChild(3).GetComponent<ColorSlider>();
        colorPicker = (Texture2D)transform.GetChild(4).GetComponent<RawImage>().texture;
        colorPreview = transform.GetChild(5).GetComponent<Image>();

        redSlider.sliderColorChange += new colorSliderChanged(changeColorPreview);
        greenSlider.sliderColorChange += new colorSliderChanged(changeColorPreview);
        blueSlider.sliderColorChange += new colorSliderChanged(changeColorPreview);
        alphaSlider.sliderColorChange += new colorSliderChanged(changeColorPreview);

        changeColorPreview();
        dataColorPicker = colorPicker.GetPixels(); 
    }

    void OnDestroy()
    {
        redSlider.sliderColorChange -= new colorSliderChanged(changeColorPreview);
        greenSlider.sliderColorChange -= new colorSliderChanged(changeColorPreview);
        blueSlider.sliderColorChange -= new colorSliderChanged(changeColorPreview);
        alphaSlider.sliderColorChange -= new colorSliderChanged(changeColorPreview);
    }

    void changeColorPreview()
    {
        Color colorToChange = getSliderColor();

        colorPreview.color = colorToChange;
        changeColor(colorToChange);
    }

    Color changeColorPreview(int pixelID)
    {
        Color colorToChange;
        colorToChange.r = dataColorPicker[pixelID].r;
        colorToChange.g = dataColorPicker[pixelID].g;
        colorToChange.b = dataColorPicker[pixelID].b;
        colorToChange.a = dataColorPicker[pixelID].a;

        colorPreview.color = colorToChange;
        changeColor(colorToChange);
        return colorToChange;
    }

    Color getSliderColor()
    {
        Color colorToChange;

        colorToChange.r = redSlider.scrollbar.value;
        colorToChange.g = greenSlider.scrollbar.value;
        colorToChange.b = blueSlider.scrollbar.value;
        colorToChange.a = alphaSlider.scrollbar.value;

        return colorToChange;
    }

    void setSliderColor(Color colorToChange)
    {
        redSlider.scrollbar.value = colorToChange.r;
        greenSlider.scrollbar.value = colorToChange.g;
        blueSlider.scrollbar.value = colorToChange.b;
        alphaSlider.scrollbar.value = colorToChange.a;
    }

    //1--2
    //|  |
    //0--3
    public void changeColorOnClick()
    {
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //Mouse Position to world coordinates
        Vector3[] colorWorldPos = new Vector3[4];
        transform.GetChild(4).transform.GetComponent<RectTransform>().GetWorldCorners(colorWorldPos);

        int pixelIDX = (int)(((screenPos.x - colorWorldPos[1].x) / (colorWorldPos[2].x - colorWorldPos[1].x)) * colorPicker.width);
        int pixelIDY = (int)(((screenPos.y - colorWorldPos[1].y) / (colorWorldPos[3].y - colorWorldPos[2].y)) * colorPicker.height);
        int pixelID = (colorPicker.height - pixelIDY) * colorPicker.width + pixelIDX;

        Color colorToChange = changeColorPreview(pixelID);
        setSliderColor(colorToChange);
    }
}
