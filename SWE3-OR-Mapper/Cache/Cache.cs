using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3_OR_Mapper.Cache
{
    public class Cache : ICache
    {
        protected Dictionary<Type, Dictionary<object, object>> Caches = new Dictionary<Type, Dictionary<object, object>>();

        protected virtual Dictionary<object, object> GetCache(Type t)
        {
            if (Caches.ContainsKey(t))
            {
                return Caches[t];
            }

            Dictionary<object, object> newCache = new Dictionary<object, object>();
            Caches.Add(t, newCache);

            return newCache;
        }

        public virtual object Get(Type t, object pk)
        {
            Dictionary<object, object> cache = GetCache(t);

            if (cache.ContainsKey(pk))
            {
                return cache[pk];
            }

            return null;
        }

        public virtual void Set(object obj)
        {
            if (obj != null)
            {
                GetCache(obj.GetType())[obj.GetEntity().PrimaryKey.GetValue(obj)] = obj;
            }
        }

        public virtual void Remove(object obj)
        {
            GetCache(obj.GetType()).Remove(obj.GetEntity().PrimaryKey.GetValue(obj));
        }

        public virtual bool ContainsKey(Type t, object pk)
        {
            return GetCache(t).ContainsKey(pk);
        }

        public virtual bool Contains(object obj)
        {
            return ContainsKey(obj.GetType(), obj.GetEntity().PrimaryKey.GetValue(obj));
        }

        public virtual bool Changed(object obj)
        {
            return true;
        }
    }
}
