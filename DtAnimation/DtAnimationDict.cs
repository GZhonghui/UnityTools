// Checked

using System.Collections.Generic;
using UnityEngine;

namespace DtAnimation
{
    [System.Serializable]
    public class DtSerializationDict<tKey, tValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<tKey> _keys = new List<tKey>();

        [SerializeField]
        private List<tValue> _values = new List<tValue>();

        public SortedDictionary<tKey, tValue> m_Dict = new SortedDictionary<tKey, tValue>();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in m_Dict)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            m_Dict = new SortedDictionary<tKey, tValue>();

            for (int i = 0; i != System.Math.Min(_keys.Count, _values.Count); i++)
                m_Dict.Add(_keys[i], _values[i]);
        }
    }
} // namespace DtAnimation
