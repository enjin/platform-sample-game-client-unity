using System;
using System.Linq;
using System.Text;
using EnjinPlatform.Data;
using UnityEngine;

namespace EnjinPlatform.Events
{
    public class TokenTransferredListener : PusherEventListener
    {
        public delegate void TokenTransferredHandler(TokenTransferredData tokenTransferredData);
        public event TokenTransferredHandler OnTokenTransferred;
        
        public override void OnEvent(string data)
        {
            Debug.Log("TokenTransferred received: "  + data);
            WebSocketPayload payload = JsonUtility.FromJson<WebSocketPayload>(data);
            TokenTransferredData tokenTransferredData = JsonUtility.FromJson<TokenTransferredData>(payload.data);
            
            Debug.Log("Collection ID: " + tokenTransferredData.GetCollectionId());
            string hexString = tokenTransferredData.GetTokenId().Value.ToString("X");
            byte[] bytes = Enumerable.Range(0, hexString.Length / 2)
                .Select(x => Convert.ToByte(hexString.Substring(x * 2, 2), 16))
                .ToArray();
            Debug.Log("Token Name: " + Encoding.ASCII.GetString(bytes) + " (" + tokenTransferredData.GetTokenId().Value + ")");
            Debug.Log("Amount: " + tokenTransferredData.amount);
            
            OnTokenTransferred?.Invoke(tokenTransferredData);
        }
        
        [Serializable]
        public class TokenTransferredData
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