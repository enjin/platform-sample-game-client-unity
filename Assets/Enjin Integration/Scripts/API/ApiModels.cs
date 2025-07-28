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

    public class MintRequest
    {
        public string tokenId;
        public int amount;
    }

    public class MeltRequest
    {
        public string tokenId;
        public int amount;
    }

    public class TransferRequest
    {
        public string tokenId;
        public int amount;
        public string recipient;
    }

    [Serializable]
    public class BoolResponse
    {
        public bool success;
    }

}