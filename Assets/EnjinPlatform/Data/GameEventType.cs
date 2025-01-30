using System.Runtime.Serialization;

namespace EnjinPlatform.Data
{ 
    public enum GameEventType
    {
        [EnumMember(Value = "ITEM_COLLECTED")]
        ITEM_COLLECTED,
        [EnumMember(Value = "ITEM_TRANSFERRED")]
        ITEM_TRANSFERRED,
        [EnumMember(Value = "ITEM_MELTED")]
        ITEM_MELTED,
    }
}