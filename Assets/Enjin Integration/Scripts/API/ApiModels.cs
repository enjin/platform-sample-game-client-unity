using UnityEngine;
using System;

namespace HappyHarvest.EnjinIntegration.API
{

    public enum RequestType { GET, POST, PUT }

    [Serializable]
    public class HealthCheckResponse
    {
        public string status;
    }

    [Serializable]
    public class RegisterResponse
    {
        public string email;
        public string wallet;
        public string token;
    }

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    // amount is a decimal string: token amounts are BigIntegers on-chain, so the
    // wire format must carry values above int/long range.
    public class MintRequest
    {
        public string tokenId;
        public string amount;
    }

    public class MeltRequest
    {
        public string tokenId;
        public string amount;
    }

    public class TransferRequest
    {
        public string tokenId;
        public string amount;
        public string recipient;
    }

    [Serializable]
    public class BoolResponse
    {
        public bool success;
    }

}