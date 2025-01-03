using System.Runtime.Serialization;

namespace EnjinPlatform.Data
{ 
    public enum GameEventType
    {
        [EnumMember(Value = "ITEM_COLLECTED")]
        ITEM_COLLECTED,
    }
}