using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.MetaModel;

namespace SWE3_OR_Mapper.Cache
{
    /// <summary> This class is a implementation with change tracking of an Orm Cache </summary>
    public class HashCache : Cache
    {
        /// <summary> Hash dictionary witch stores a dictionary for every type </summary>
        protected Dictionary<Type, Dictionary<object, string>> Hashes = new Dictionary<Type, Dictionary<object, string>>();

        /// <summary> Returns the hash dictionary for a given type </summary>
        /// <param name="t"> Type of the hash dictionary objects </param>
        /// <returns> Type hash dictionary </returns>
        protected virtual Dictionary<object, string> GetHash(Type t)
        {
            if (Hashes.ContainsKey(t))
            {
                return Hashes[t];
            }

            Dictionary<object, string> newHash = new Dictionary<object, string>();
            Hashes.Add(t, newHash);

            return newHash;
        }

        /// <summary> Generates the hash for a given object </summary>
        /// <param name="obj"> Object for witch the hash is generated </param>
        /// <returns> Generated hash </returns>
        protected string GenerateHash(object obj)
        {
            string hash = "";
            foreach (Field i in obj.GetEntity().Internals)
            {
                if (i.IsForeignKey)
                {
                    object m = i.GetValue(obj);
                    if (m != null)
                    {
                        hash += m.GetEntity().PrimaryKey.GetValue(m).ToString();
                    }
                }
                else
                {
                    hash += (i.ColumnName + "=" + i.GetValue(obj).ToString() + ";");
                }
            }

            foreach (Field i in obj.GetEntity().Externals)
            {
                IEnumerable m = (IEnumerable)i.GetValue(obj);

                if (m != null)
                {
                    hash += (i.ColumnName + "=");
                    foreach (object k in m)
                    {
                        hash += k.GetEntity().PrimaryKey.GetValue(k).ToString() + ",";
                    }
                }
            }

            return Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(hash)));
        }


        /// <summary> Sets an object in the cache </summary>
        /// <param name="obj"> Object to be set </param>
        public override void Set(object obj)
        {
            base.Set(obj);
            if (obj != null)
            {
                GetHash(obj.GetType())[obj.GetEntity().PrimaryKey.GetValue(obj)] = GenerateHash(obj);
            }
        }

        /// <summary> Removes an object from the cache </summary>
        /// <param name="obj"> Object to be removed </param>
        public override void Remove(object obj)
        {
            base.Remove(obj);
            GetHash(obj.GetType()).Remove(obj.GetEntity().PrimaryKey.GetValue(obj));
        }

        /// <summary> Checks if an object has changed </summary>
        /// <param name="obj"> Object to be checked </param>
        /// <returns> Returns TRUE if the object has changed. Returns FALSE if the object has not changed </returns>
        public override bool Changed(object obj)
        {
            Dictionary<object, string> hash = GetHash(obj.GetType());
            object pk = obj.GetEntity().PrimaryKey.GetValue(obj);

            if (hash.ContainsKey(pk))
            {
                return hash[pk] != GenerateHash(obj);
            }

            return true;
        }
    }
}
