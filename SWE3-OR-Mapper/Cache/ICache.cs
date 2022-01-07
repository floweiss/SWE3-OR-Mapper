using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3_OR_Mapper.Cache
{
    /// <summary> Interface for all Orm Caches </summary>
    public interface ICache
    {
        /// <summary> Gets an object from the cache </summary>
        /// <param name="t"> Type of the object </param>
        /// <param name="pk"> Primary key of the object </param>
        /// <returns> object </returns>
        object Get(Type t, object pk);

        /// <summary> Sets an object in the cache </summary>
        /// <param name="obj"> Object to be set </param>
        void Set(object obj);

        /// <summary> Removes an object from the cache </summary>
        /// <param name="obj"> Object to be removed </param>
        void Remove(object obj);

        /// <summary> Checks if the cache contains an object of given type with the given primary key </summary>
        /// <param name="t"> Type of the object </param>
        /// <param name="pk"> Primary key of the object </param>
        /// <returns> Returns TRUE if the object is in the Cache. Returns FALSE otherwise </returns>
        bool ContainsKey(Type t, object pk);

        /// <summary> Checks if the cache contains a given object </summary>
        /// <param name="obj"> Object to be checked </param>
        /// <returns> Returns TRUE if the object is in the Cache. Returns FALSE otherwise </returns>
        bool Contains(object obj);

        /// <summary> Checks if an object has changed </summary>
        /// <param name="obj"> Object to be checked </param>
        /// <returns> Returns TRUE if the object has changed. Returns FALSE if the object has not changed </returns>
        bool Changed(object obj);
    }
}
