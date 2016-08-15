using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
    public sealed class ProxyObject : Variant, IEnumerable<KeyValuePair<string, Variant>>
    {
        internal const string TypeHintName = "@type";
        private Dictionary<string, Variant> m_Value;


        public ProxyObject()
        {
            m_Value = new Dictionary<string, Variant>();
        }

        public override object value
        {
            get
            {
                return m_Value;
            }
        }


        IEnumerator<KeyValuePair<string, Variant>> IEnumerable<KeyValuePair<string, Variant>>.GetEnumerator()
        {
            return m_Value.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Value.GetEnumerator();
        }


        public void Add(string key, Variant item)
        {
            m_Value.Add(key, item);
        }


        public bool TryGetValue(string key, out Variant item)
        {
            return m_Value.TryGetValue(key, out item);
        }

        public bool ContainsKey(string key)
        {
            return m_Value.ContainsKey(key);
        }

        public string TypeHint
        {
            get
            {
                Variant item;
                if (TryGetValue(TypeHintName, out item))
                {
                    return item.ToString();
                }
                return null;
            }
        }

        public override Variant this[string key]
        {
            get { return m_Value[key]; }
            set { m_Value[key] = value; }
        }


        public int Count
        {
            get { return m_Value.Count; }
        }
    }
}

