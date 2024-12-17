using System;
using System.Numerics;
using UnityEngine;

// TODO: DOCS - Explain: We need this function as the blockchain uses uint128 numbers and Unity can't serialize them.
[Serializable]
public class SerializableBigInteger
{
    [SerializeField]
    private string value;

    public BigInteger Value
    {
        get => BigInteger.Parse(value);
        set => this.value = value.ToString();
    }

    public SerializableBigInteger(BigInteger value)
    {
        Value = value;
    }

    public static implicit operator BigInteger(SerializableBigInteger sbi) => sbi.Value;
    public static implicit operator SerializableBigInteger(BigInteger bi) => new SerializableBigInteger(bi);
}