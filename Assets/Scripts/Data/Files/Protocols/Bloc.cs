public class Bloc
{
    public Bloc()
    {
        mainEvent = new MainEventBloc();
        secondaryEvents = new SecondaryEventsBloc();
        dispBloc = new DisplayBloc();
    }

    ~Bloc()
    {

    }

    public MainEventBloc mainEvent;
    public SecondaryEventsBloc secondaryEvents;
    public DisplayBloc dispBloc;
}