using System;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace EnjinPlatform.Events
{
    public class TokenMintedListener : PusherEventListener
    {
        public override void OnEvent(string data)
        {
            Debug.Log("TokenMinted received: "  + data);
            WebSocketPayload payload = JsonUtility.FromJson<WebSocketPayload>(data);
            TokenMintedData tokenMintedData = JsonUtility.FromJson<TokenMintedData>(payload.data);
            
            Debug.Log("Collection ID: " + tokenMintedData.GetCollectionId());
            string hexString = tokenMintedData.GetTokenId().Value.ToString("X");
            byte[] bytes = Enumerable.Range(0, hexString.Length / 2)
                .Select(x => Convert.ToByte(hexString.Substring(x * 2, 2), 16))
                .ToArray();
            Debug.Log("Token Name: " + Encoding.ASCII.GetString(bytes) + " (" + tokenMintedData.GetTokenId().Value + ")");
            Debug.Log("Amount: " + tokenMintedData.amount);
        }
        
        [Serializable]
        public class TokenMintedData
        {
            public string extrinsicIndex;
            public string module;
            public string name;
            public string collectionId;
            public string tokenId;
            public string issuer;
            public string recipient;
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