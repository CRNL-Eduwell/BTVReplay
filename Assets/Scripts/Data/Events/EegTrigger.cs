
public class EegTrigger
{
    public float ReactionTimeInMs { get { return m_Response.TimeInMilliSeconds - m_MainEvent.TimeInMilliSeconds; } }

    private EegEvent m_MainEvent = null;
    private EegEvent m_Response = null;

    public EegTrigger(EegTrigger trigger)
    {
        m_MainEvent = new EegEvent(trigger.m_MainEvent);
        m_Response = new EegEvent(trigger.m_Response);
    }
    public EegTrigger(EegEvent trigger)
    {
        m_MainEvent = new EegEvent(trigger);
    }
    public EegTrigger(EegEvent trigger, EegEvent response)
    {
        m_MainEvent = new EegEvent(trigger.Code, trigger.TimeInMilliSeconds);
        m_Response = new EegEvent(response.Code, response.TimeInMilliSeconds);
    }

    public static bool operator !=(EegTrigger c1, EegTrigger c2)
    {
        return !(c1 == c2);
    }
    public static bool operator ==(EegTrigger c1, EegTrigger c2)
    {
        if (c1.m_MainEvent.Code == c2.m_MainEvent.Code)
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
            return m_MainEvent.Code.Equals(triggObj.m_MainEvent.Code);
    }
    public override int GetHashCode()
    {
        return m_MainEvent.GetHashCode();
    }
}