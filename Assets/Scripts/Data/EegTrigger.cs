public class EegTrigger
{
    public EegEvent Trigger { get; set; }
    public EegEvent Response { get; set; }
    public int ReactionTimeInSample
    {
        get
        {
            return m_rtSample = Response.Sample - Trigger.Sample;
        }
        set
        {
            m_rtSample = value;
        }
    }

    int m_rtSample = -1;
    int m_rtMs = -1;

    public EegTrigger(EegTrigger trigger)
    {
        Trigger = new EegEvent(trigger.Trigger);
        Response = new EegEvent(trigger.Response);
    }
    public EegTrigger(EegEvent trigger)
    {
        Trigger = new EegEvent(trigger);
    }
    public EegTrigger(EegEvent trigger, EegEvent response)
    {
        Trigger = new EegEvent(trigger);
        Response = new EegEvent(response);
    }
    ~EegTrigger()
    {

    }

    public int ReactionTimeInMs(int samplingFreq)
    {
        return m_rtMs = (int)(((float)ReactionTimeInSample / samplingFreq) * 1000);
    }
    public int ReactionTimeInMs()
    {
        return m_rtMs;
    }

    public static bool operator !=(EegTrigger c1, EegTrigger c2)
    {
        return !(c1 == c2);
    }
    public static bool operator ==(EegTrigger c1, EegTrigger c2)
    {
        if (c1.Trigger.Code == c2.Trigger.Code)
            return true;
        else
            return false;
    }
    public override bool Equals(object obj)
    {
        EegTrigger triggObj = obj as EegTrigger;
        if (triggObj == null)
            return false;
        else
            return Trigger.Code.Equals(triggObj.Trigger.Code);
    }
    public override int GetHashCode()
    {
        return Trigger.GetHashCode();
    }
}

public class EegTrigger2
{
    public float ReactionTimeInMs { get { return m_Response.TimeInMilliSeconds - m_MainEvent.TimeInMilliSeconds; } }

    private EegEvent2 m_MainEvent = null;
    private EegEvent2 m_Response = null;

    public EegTrigger2(EegTrigger2 trigger)
    {
        m_MainEvent = new EegEvent2(trigger.m_MainEvent);
        m_Response = new EegEvent2(trigger.m_Response);
    }
    public EegTrigger2(EegEvent2 trigger)
    {
        m_MainEvent = new EegEvent2(trigger);
    }
    public EegTrigger2(EegEvent2 trigger, EegEvent2 response)
    {
        m_MainEvent = new EegEvent2(trigger.Code, trigger.TimeInMilliSeconds);
        m_Response = new EegEvent2(response.Code, response.TimeInMilliSeconds);
    }

    public static bool operator !=(EegTrigger2 c1, EegTrigger2 c2)
    {
        return !(c1 == c2);
    }
    public static bool operator ==(EegTrigger2 c1, EegTrigger2 c2)
    {
        if (c1.m_MainEvent.Code == c2.m_MainEvent.Code)
            return true;
        else
            return false;
    }
    public override bool Equals(object obj)
    {
        EegTrigger2 triggObj = obj as EegTrigger2;
        if (triggObj == null)
            return false;
        else
            return m_MainEvent.Code.Equals(triggObj.m_MainEvent.Code);
    }
    public override int GetHashCode()
    {
        return m_MainEvent.GetHashCode();
    }
}