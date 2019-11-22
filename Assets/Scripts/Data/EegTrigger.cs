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
