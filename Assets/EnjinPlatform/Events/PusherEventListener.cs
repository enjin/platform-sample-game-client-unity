namespace EnjinPlatform.Events
{
    public abstract class PusherEventListener
    {
        public abstract void OnEvent(string data);
    }

    [System.Serializable]
    public class WebSocketPayload
    {
        public string @event;
        public string data;
        public string channel;
    }
}