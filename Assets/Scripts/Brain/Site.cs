using BTV.Data;
using BTV.Services.EegFileService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.CSharp.EEG;
using UnityEngine;

public class Site : MonoBehaviour
{
    public float EegValue { get; private set; } = 0;
    #region Pubic Members
    public int ID
    {
        get;
        private set;
    }

    public bool IsFrozen
    {
        get
        {
            return m_IsFrozen;
        }
        set
        {
            if (value == true)
            {
                gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            m_IsFrozen = value;
        }
    }

    public string MarsAtlasName
    {
        get { return m_Plot != null ? m_Plot.MarsAtlas : ""; }
    }

    public string BroadmanName
    {
        get { return m_Plot != null ? m_Plot.Broadmann : ""; }
    }

    /// <summary>
    /// Coordinates in the Unity Referential
    /// </summary>
    public Vector3 Coordinates
    {
        get
        {
            return gameObject.transform.localPosition;
        }
    }

    /// <summary>
    /// 3d coordinates representation on 2d texture
    /// </summary>
    public Vector2 Texture2dCoordinates_Normal
    {
        get
        {
            float x = 0.0f, y = 0.0f;
            Vector3 coordinates = Coordinates;

            //Carthesien to spherical
            float r = Mathf.Sqrt((coordinates.x * coordinates.x) + (coordinates.y * coordinates.y) + (coordinates.z * coordinates.z));

            //Check values if we find the start values (debug)
            //float theta_latt_rad = Mathf.Acos(m_coordinates_Z[count] / r);
            //float phi_long_rad = (float)Mathf.Atan2(m_coordinates_Y[count], m_coordinates_X[count]);
            //float theta_latt_degree = (theta_latt_rad * (180f / Mathf.PI));
            //float phi_long_degree = (phi_long_rad * (180f / Mathf.PI));
            //if (phi_long_degree >= 360) { phi_long_degree -= 360; }
            //else if (phi_long_degree < 0) { phi_long_degree += 360; }

            float theta = (float)Math.Acos(coordinates.z / r);
            if (Math.Abs(theta) < 1.0e-16)
            {
                y = 0.0f;
                x = 0.0f;
            }
            else
            {
                y = (theta / Mathf.Sin(theta)) * (coordinates.y / r);
                x = (theta / Mathf.Sin(theta)) * (coordinates.x / r);
            }

            x = r + (r * (2.0f / Mathf.PI) * x);
            y = r - (r * (2.0f / Mathf.PI) * y);

            return new Vector2(x, y); 
        }
    }

    public Color Color
    {
        get
        {
            return m_Color;
        }
        set
        {
            m_Color = value;
            // Use the cached instanced material: m_MeshRenderer.materials[0] allocated a new
            // array and instantiated materials on every call (this runs per electrode per tick).
            if (m_Material != null) m_Material.color = m_Color;
        }
    }
    #endregion

    #region Private Members
    private AnatomicalSite m_Plot = null; //Reference to the corresponding data element , either a Eeg_Plot or Intra_Plot
    private bool m_IsFrozen = false;
    private MeshRenderer m_MeshRenderer = null;
    private Material m_Material = null;
    private Color m_Color = Color.white;
    private float m_Gain = 1;

    private TraceOption m_MasterTraceOption = null;
    private BtvChannel m_Channel = null;
    private Frequency m_Frequency = null;
    #endregion

    public void Init(AnatomicalSite site)
    {
        m_Plot = site;
        m_MasterTraceOption = TracesService.GetOptionsFor(0);

        Messenger.Default.Register<UiToBrainMessage>(this, OnBrainParametersMessage, MessageContext.UiToBrain);
        Messenger.Default.Register<VideoToModulesMessage>(this, OnVideoToModulesMessage, MessageContext.VideoToModulesMessage);

        m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
        m_Material = m_MeshRenderer.material; // instanced once; reused by the Color setter
        UpdateElectrodeID(m_MasterTraceOption.FileHandle);
        IsFrozen = false;
    }

    /// <summary>
    /// If we don't find the corresponding name beetween this object and one
    /// electrode in an eeg file, we don't show the site on the 3D brain
    /// </summary>
    /// <param name="container"></param>
    private void UpdateElectrodeID(BtvProgram container)
    {
        ID = container.GetElectrodeIDFromElectrodeName(gameObject.name.ToLower(), true);
        if (ID == -1)
        {
            gameObject.SetActive(false);
            m_Channel = null;
            m_Frequency = null;
        }
        else
        {
            m_Channel = container.Channels[ID];
            m_Frequency = m_Channel.Frequency;
        }
    }

    /// <summary>
    /// Used to unsubscribe from events whent he site is deactivated
    /// otherwise it tries to update values that are note here.
    /// Maybe see to destroy the go totally ? 
    /// </summary>
    private void OnDisable()
    {
        //m_MasterTraceOption.PropertyChanged -= OnMasterTraceOptionPropertyChanged;

        Messenger.Default.Unregister(this, MessageContext.UiToBrain);
        Messenger.Default.Unregister(this, MessageContext.VideoToModulesMessage);
    }

    /// <summary>
    /// Re-subscribe when a site is re-activated after OnDisable (Messenger.Register is
    /// idempotent, so this is safe alongside the registration in Init). Guarded on m_Plot so
    /// the pre-Init OnEnable fired during Instantiate is a no-op.
    /// </summary>
    private void OnEnable()
    {
        if (m_Plot == null) return;
        Messenger.Default.Register<UiToBrainMessage>(this, OnBrainParametersMessage, MessageContext.UiToBrain);
        Messenger.Default.Register<VideoToModulesMessage>(this, OnVideoToModulesMessage, MessageContext.VideoToModulesMessage);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.UiToBrain);
        Messenger.Default.Unregister(this, MessageContext.VideoToModulesMessage);
    }

    private void OnBrainParametersMessage(UiToBrainMessage message)
    {
        if (message.TaskToExecute == 2) //=> gain update
            m_Gain = message.Gain;
    }

    private void OnVideoToModulesMessage(VideoToModulesMessage message)
    {
        UpdateSize((int)message.TimeMilliseconds);
    }

    private void UpdateSize(int milliSecToLook)
    {
        if (m_Channel == null || m_Frequency == null) return; // no matching EEG channel (e.g. re-enabled invalid site)
        if (!IsFrozen)
        {
            int MostRecentSample = m_Frequency.ConvertToRoundedNumberOfSamples(milliSecToLook);
            EegValue = m_Channel.GetSample(MostRecentSample) / 100;
            float currentValue = 2 + (m_Gain * EegValue);

            if (currentValue >= 7)
                currentValue = 7;
            else if (currentValue <= 0)
                currentValue = 0.1f;

            gameObject.transform.localScale = new Vector3(currentValue, currentValue, currentValue);
        }
    }
}

