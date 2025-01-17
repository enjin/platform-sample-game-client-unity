using System;
using System.Reflection;
using UnityEngine;

namespace EnjinPlatform.Events
{
    public abstract class GameEventData
    {
        public string GetEncodedData()
        {
            if (!ValidateFields())
            {
                Debug.LogError("Validation failed: Not all fields are filled.");
                
                return null;
            }

            string jsonString = JsonUtility.ToJson(this);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            
            return System.Convert.ToBase64String(dataBytes);
        }
        
        private bool ValidateFields()
        {
            Type type = this.GetType();
            while (type != null && type != typeof(object))
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    object value = field.GetValue(this);
                    if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                    {
                        Debug.LogError($"Field {field.Name} is not filled.");
                        return false;
                    }
                }
                type = type.BaseType;
            }
            return true;
        }
    }
}