using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloads
{
   public class KeyValueElementCollection
    {
        public KeyValueElementCollection()
        {
            Elements = new List<KeyValueElement>();
        }

        internal List<KeyValueElement> Elements { get; }

        public string this[string key]
        {
            get { return Elements.SingleOrDefault(x => x.Key == key) ?? ""; }
        }
    }

    public class KeyValueElement
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static implicit operator string(KeyValueElement key)
        {
            return key.Value;
        }
    }
}
