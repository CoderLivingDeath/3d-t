using System.Collections.Generic;
using UnityEngine;

namespace AFPC
{
    [System.Serializable]
    public class AFPCObjectInfo
    {
        public string ID = "Unnamed";
        public Sprite Icon;
        public string Description = "";
        public List<StringEntry> StringData = new List<StringEntry>();
        public List<IntEntry> IntData = new List<IntEntry>();
        public List<FloatEntry> FloatData = new List<FloatEntry>();

        public bool Has (string key) {
            for (int i = 0; i < StringData.Count; i++) if (StringData[i].Key == key) return true;
            for (int i = 0; i < IntData.Count; i++) if (IntData[i].Key == key) return true;
            for (int i = 0; i < FloatData.Count; i++) if (FloatData[i].Key == key) return true;
            return false;
        }

        public string GetString (string key, string fallback = "") {
            for (int i = 0; i < StringData.Count; i++) if (StringData[i].Key == key) return StringData[i].Value;
            return fallback;
        }

        public int GetInt (string key, int fallback = 0) {
            for (int i = 0; i < IntData.Count; i++) if (IntData[i].Key == key) return IntData[i].Value;
            return fallback;
        }

        public float GetFloat (string key, float fallback = 0f) {
            for (int i = 0; i < FloatData.Count; i++) if (FloatData[i].Key == key) return FloatData[i].Value;
            return fallback;
        }
    }

    [System.Serializable]
    public class StringEntry
    {
        public string Key = "";
        public string Value = "";
    }

    [System.Serializable]
    public class IntEntry
    {
        public string Key = "";
        public int Value;
    }

    [System.Serializable]
    public class FloatEntry
    {
        public string Key = "";
        public float Value;
    }
}
