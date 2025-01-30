using System;
using System.Linq;
using System.Text;
using EnjinPlatform.Data;
using UnityEngine;

namespace EnjinPlatform.Events
{
    public class TokenBurnedListener : PusherEventListener
    {
        public delegate void TokenBurnedHandler(TokenBurnedData tokenBurnedData);
        public event TokenBurnedHandler OnTokenBurned;
        
        public override void OnEvent(string data)
        {
            Debug.Log("TokenBurned received: "  + data);
            WebSocketPayload payload = JsonUtility.FromJson<WebSocketPayload>(data);
            TokenBurnedData tokenBurnedData = JsonUtility.FromJson<TokenBurnedData>(payload.data);
            
            Debug.Log("Collection ID: " + tokenBurnedData.GetCollectionId());
            string hexString = tokenBurnedData.GetTokenId().Value.ToString("X");
            byte[] bytes = Enumerable.Range(0, hexString.Length / 2)
                .Select(x => Convert.ToByte(hexString.Substring(x * 2, 2), 16))
                .ToArray();
            Debug.Log("Token Name: " + Encoding.ASCII.GetString(bytes) + " (" + tokenBurnedData.GetTokenId().Value + ")");
            Debug.Log("Amount: " + tokenBurnedData.amount);
            
            OnTokenBurned?.Invoke(tokenBurnedData);
        }
        
        [Serializable]
        public class TokenBurnedData
        {
            public string extrinsicIndex;
            public string module;
            public string name;
            public string collectionId;
            public string tokenId;
            public string account;
            public int amount;
            public string idempotencyKey;
            public string uuid;
            
            public SerializableBigInteger GetCollectionId()
            {
                return new SerializableBigInteger(collectionId);
            }
            
            public SerializableBigInteger GetTokenId()
            {
                return new SerializableBigInteger(tokenId);
            }
        }
    }
}