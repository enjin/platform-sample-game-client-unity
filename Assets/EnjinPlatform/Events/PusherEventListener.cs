namespace EnjinPlatform.Events
{
    public abstract class PusherEventListener
    {
        public abstract void OnEvent(dynamic data);
    }
}