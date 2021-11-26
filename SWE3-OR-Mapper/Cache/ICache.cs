using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3_OR_Mapper.Cache
{
    public interface ICache
    {
        object Get(Type t, object pk);

        void Set(object obj);

        void Remove(object obj);

        bool ContainsKey(Type t, object pk);

        bool Contains(object obj);

        bool Changed(object obj);
    }
}
