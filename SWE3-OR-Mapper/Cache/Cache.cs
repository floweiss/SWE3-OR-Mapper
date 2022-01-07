using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3_OR_Mapper.Cache
{
    /// <summary> This class is a basic implementation of an Orm Cache </summary>
    public class Cache : ICache
    {
        /// <summary> Cache dictionary witch stores a dictionary for every type </summary>
        protected Dictionary<Type, Dictionary<object, object>> Caches = new Dictionary<Type, Dictionary<object, object>>();

        /// <summary> Returns the cache dictionary for a given type </summary>
        /// <param name="t"> Type of the cache objects </param>
        /// <returns> Type cache </returns>
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

        /// <summary> Gets an object from the cache </summary>
        /// <param name="t"> Type of the object </param>
        /// <param name="pk"> Primary key of the object </param>
        /// <returns> object </returns>
        public virtual object Get(Type t, object pk)
        {
            Dictionary<object, object> cache = GetCache(t);

            if (cache.ContainsKey(pk))
            {
                return cache[pk];
            }

            return null;
        }

        /// <summary> Sets an object in the cache </summary>
        /// <param name="obj"> Object to be set </param>
        public virtual void Set(object obj)
        {
            if (obj != null)
            {
                GetCache(obj.GetType())[obj.GetEntity().PrimaryKey.GetValue(obj)] = obj;
            }
        }

        /// <summary> Removes an object from the cache </summary>
        /// <param name="obj"> Object to be removed </param>
        public virtual void Remove(object obj)
        {
            GetCache(obj.GetType()).Remove(obj.GetEntity().PrimaryKey.GetValue(obj));
        }

        /// <summary> Checks if the cache contains an object of given type with the given primary key </summary>
        /// <param name="t"> Type of the object </param>
        /// <param name="pk"> Primary key of the object </param>
        /// <returns> Returns TRUE if the object is in the Cache. Returns FALSE otherwise </returns>
        public virtual bool ContainsKey(Type t, object pk)
        {
            return GetCache(t).ContainsKey(pk);
        }

        /// <summary> Checks if the cache contains a given object </summary>
        /// <param name="obj"> Object to be checked </param>
        /// <returns> Returns TRUE if the object is in the Cache. Returns FALSE otherwise </returns>
        public virtual bool Contains(object obj)
        {
            return ContainsKey(obj.GetType(), obj.GetEntity().PrimaryKey.GetValue(obj));
        }

        /// <summary> Checks if an object has changed </summary>
        /// <param name="obj"> Object to be checked </param>
        /// <returns> Always returns TRUE because this cache has no change tracking functionality </returns>
        public virtual bool Changed(object obj)
        {
            return true;
        }
    }
}
