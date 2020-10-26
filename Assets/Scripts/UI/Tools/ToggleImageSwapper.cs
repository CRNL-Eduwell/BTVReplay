using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleImageSwapper : MonoBehaviour
{
    //First, we get the target Toggle Component. 
    //Even if you don't select the target Toggle component here, the script get the one one the same object.
    [SerializeField] private Toggle targetToggle = null;

    //This means that we want to actually "Swap" the target image which is set in Toggle component. 
    //By default, it is the "Background".
    //You may want to use this while "Checkmark" image set to "none" for traditional effect.
    [SerializeField] private bool swapTogglesTargetGraphic = true;
    [SerializeField] private Sprite swapSprite = null;

    //This means that we want to "Enable" a different image instead of "Checkmark" image when the toggle is unchecked.
    //You need to create a new image to use this, you can just duplicate Checkmark image and change it.
    [SerializeField] private bool enableUncheckedGraphic = false;
    [SerializeField] private Graphic uncheckedGraphic = null;


    private void Start()
    {
        OnTargetToggleValueChanged(targetToggle.isOn);
        targetToggle.onValueChanged.AddListener(OnTargetToggleValueChanged);
        targetToggle.toggleTransition = Toggle.ToggleTransition.None;
        if (uncheckedGraphic != null)
            uncheckedGraphic.CrossFadeAlpha(targetToggle.isOn ? 0f : 1f, 0f, true);
    }

    private void OnTargetToggleValueChanged(bool toggleValue)
    {
        if (swapTogglesTargetGraphic)
        {
            Image targetImage = targetToggle.targetGraphic as Image;
            if (targetImage != null)
            {
                targetImage.overrideSprite = toggleValue ? swapSprite : null;
            }
        }
        if (enableUncheckedGraphic)
        {
            uncheckedGraphic.CrossFadeAlpha(toggleValue ? 0f : 1f, 0f, true);
        }
    }
}