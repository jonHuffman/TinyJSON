using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
    public sealed class ProxyArray : Variant, IEnumerable<Variant>
    {
        public const string CombineHintName = "@index";

        private List<Variant> m_Value;


        public ProxyArray()
        {
            m_Value = new List<Variant>();
        }

        public override object value
        {
            get
            {
                return m_Value;
            }
        }

        IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
        {
            return m_Value.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Value.GetEnumerator();
        }


        public void Add( Variant item )
        {
            m_Value.Add( item );
        }


        public override Variant this[ int index ]
        {
            get { return m_Value[index]; }
            set { m_Value[index] = value; }
        }


        public int Count
        {
            get { return m_Value.Count; }
        }


        internal bool CanBeMultiRankArray( int[] rankLengths )
        {
            return CanBeMultiRankArray( 0, rankLengths );
        }

        private bool CanBeMultiRankArray( int rank, int[] rankLengths )
        {
            var count = m_Value.Count;
            rankLengths[rank] = count;

            if (rank == rankLengths.Length - 1)
            {
                return true;
            }

            var firstItem = m_Value[0] as ProxyArray;
            if (firstItem == null)
            {
                return false;
            }
            var firstItemCount = firstItem.Count;

            for (int i = 1; i < count; i++)
            {
                var item = m_Value[i] as ProxyArray;

                if (item == null)
                {
                    return false;
                }

                if (item.Count != firstItemCount)
                {
                    return false;
                }

                if (!item.CanBeMultiRankArray( rank + 1, rankLengths ))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

