using System;
using System.Numerics;
using UnityEngine;

namespace EnjinPlatform.Data
{
// TODO: DOCS - Explain: We need this function as the blockchain uses uint128 numbers and Unity can't serialize them.
    [Serializable]
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
    }
}