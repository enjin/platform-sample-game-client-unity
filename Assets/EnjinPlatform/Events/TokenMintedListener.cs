using UnityEngine;

namespace EnjinPlatform.Events
{
    public class TokenMintedListener : PusherEventListener
    {
        public override void OnEvent(dynamic data)
        {
            Debug.Log("TokenMinted received");
        }
    }
}