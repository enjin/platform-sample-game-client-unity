using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace HappyHarvest.EnjinIntegration.Data
{
    public class PlatformModels
    {
        [System.Serializable]
        public class SerializableBigInteger
        {
            [SerializeField] private string value;

            public BigInteger Value
            {
                get => BigInteger.Parse(value);
                set => this.value = value.ToString();
            }

            public SerializableBigInteger(BigInteger value)
            {
                Value = value;
            }

            public SerializableBigInteger(int value)
            {
                Value = new BigInteger(value);
            }

            public SerializableBigInteger(string value)
            {
                Value = BigInteger.Parse(value);
            }

            public static implicit operator BigInteger(SerializableBigInteger sbi) => sbi.Value;
            public static implicit operator string(SerializableBigInteger sbi) => sbi.value;
            public static implicit operator SerializableBigInteger(BigInteger bi) => new SerializableBigInteger(bi);
            public static implicit operator SerializableBigInteger(int i) => new SerializableBigInteger(i);
            public static implicit operator SerializableBigInteger(string s) => new SerializableBigInteger(s);

            public override bool Equals(object obj)
            {
                if (obj is SerializableBigInteger other)
                    return this.Value == other.Value;
                return false;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public override string ToString()
            {
                return value;
            }
        }

        [System.Serializable]
        public class ManagedWalletAccount
        {
            public Account account;
            public TokenAccount[] tokenAccounts;

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine("---- Managed Wallet ----");
                sb.AppendLine(account?.ToString() ?? "Account: null");

                if (tokenAccounts != null && tokenAccounts.Length > 0)
                {
                    sb.AppendLine("Token Accounts:");
                    // Join the string from each TokenAccount
                    sb.Append(string.Join("\n", tokenAccounts.Select(ta => ta.ToString())));
                }
                else
                {
                    sb.AppendLine("Token Accounts: None");
                }
                return sb.ToString();
            }
        }

        [System.Serializable]
        public class Account
        {
            public string publicKey;
            public string address;

            public override string ToString() => $"Account | Address: {address}, PublicKey: {publicKey}";
        }

        [System.Serializable]
        public class TokenAccount
        {
            public string balance;
            public Token token;

            public override string ToString() => $"  - Balance: {balance}\n{token?.ToString() ?? "    Token: null"}";
        }

        [System.Serializable]
        public class Token
        {
            public Collection collection;
            public string tokenId;
            public Attribute[] attributes;

            public override string ToString()
            {
                var attrs = attributes != null && attributes.Length > 0
                    ? string.Join(", ", attributes.Select(a => a.ToString()))
                    : "None";
                
                return $"    Token ID: {tokenId}\n    {collection?.ToString() ?? "Collection: null"}\n    Attributes: [{attrs}]";
            }
        }

        [System.Serializable]
        public class Collection
        {
            public string collectionId;

            public override string ToString() => $"Collection ID: {collectionId}";
        }

        [System.Serializable]
        public class Attribute
        {
            public string key;
            public string value;
            public override string ToString() => $"{key}: {value}";
        }
    }
}