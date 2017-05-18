using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void ColorElecChangedEventHandler(Color newColor);

public class ColorPickerManager : MonoBehaviour
{
    public GameObject rawImage = null;
    public Scrollbar redScrollbar = null;
    public Scrollbar greenScrollbar = null;
    public Scrollbar blueScrollbar = null;
    public Scrollbar alphaScrollbar = null;
    public Image colorPreview;
    public Button okButon = null;

    public event ColorElecChangedEventHandler colorElecChanged;

    //===
    BoxCollider RawImageColider;
    RectTransform RawImageRectTransform;

    float coliderWidth, coliderHeight;
    Vector3 centerTextureFromParent, centerColiderFromParent;
    float lowX, HighX, lowY, HighY;

    Color[] Data;

    // Use this for initialization
    void Start ()
    {
        RawImageColider = rawImage.AddComponent<BoxCollider>();
        RawImageRectTransform = rawImage.GetComponent<RectTransform>();
        addColiderToColorPicture();

        Texture2D tex = (Texture2D)rawImage.GetComponent<RawImage>().texture;
        Data =  tex.GetPixels();

        okButon.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            colorElecChanged(colorPreview.color);
        });
    }

    void OnDestroy()
    {
        okButon.onClick.RemoveAllListeners();
    }

    // Update is called once per frame
    void Update ()
    {
        if ((coliderWidth != RawImageRectTransform.rect.width) || (coliderHeight != RawImageRectTransform.rect.height))
        {
            coliderWidth = RawImageRectTransform.rect.width;
            coliderHeight = RawImageRectTransform.rect.height;
            RawImageColider.size = new Vector3(coliderWidth, coliderHeight, 0);
        }

        if (Input.GetMouseButtonUp(0))
        {
            checkColorClicked();
        }
    }

    void addColiderToColorPicture()
    {
        //== def colider size
        coliderWidth = RawImageRectTransform.rect.width;
        coliderHeight = RawImageRectTransform.rect.height;
        RawImageColider.size = new Vector3(coliderWidth, coliderHeight, 0);

        //== def colider offset 
        centerTextureFromParent = (RawImageRectTransform.transform.position - gameObject.transform.position); //Offset centre texture
        centerColiderFromParent = (RawImageColider.bounds.center - centerTextureFromParent);                 //center colider

        lowX = centerColiderFromParent.x - RawImageColider.bounds.extents.x;
        HighX = centerColiderFromParent.x + RawImageColider.bounds.extents.x;
        lowY = centerColiderFromParent.y - RawImageColider.bounds.extents.y;
        HighY = centerColiderFromParent.y + RawImageColider.bounds.extents.y;
    }

    void checkColorClicked()
    {
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //Mouse Position to world coordinates
        screenPos = screenPos - centerTextureFromParent;
        Vector2 screenPosOk = new Vector2(screenPos.x, screenPos.y);
        screenPosOk.y = screenPosOk.y - gameObject.transform.localScale.y;

        chooseColor(screenPosOk);
    }

    void chooseColor(Vector3 mousePosInPicker)
    {
        Color chosenColor;
        if ((mousePosInPicker.x >= lowX) && (mousePosInPicker.x <= HighX) && (mousePosInPicker.y >= lowY) && (mousePosInPicker.y <= HighY))
        {
            int pixelIDX = (int)(((mousePosInPicker.x - lowX) / (HighX - lowX)) * 128);
            int pixelIDY = (int)(((mousePosInPicker.y - lowY) / (HighY - lowY)) * 128);
            int pixelID = pixelIDY * 128 + pixelIDX;

            chosenColor.r = Data[pixelID].r;
            chosenColor.g = Data[pixelID].g;
            chosenColor.b = Data[pixelID].b;
            chosenColor.a = 255;
            colorPreview.color = chosenColor;

            redScrollbar.value = chosenColor.r;
            greenScrollbar.value = chosenColor.g;
            blueScrollbar.value = chosenColor.b;
            alphaScrollbar.value = chosenColor.a;
        }
    }

    public void changeColorPreview()
    {
        Color colorToChange;

        colorToChange.r = redScrollbar.value;
        colorToChange.g = greenScrollbar.value;
        colorToChange.b = blueScrollbar.value;
        colorToChange.a = alphaScrollbar.value;
        colorPreview.color = colorToChange;
    }
}
