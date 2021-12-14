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
    public class HashCache : Cache
    {
        protected Dictionary<Type, Dictionary<object, string>> Hashes = new Dictionary<Type, Dictionary<object, string>>();

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

        protected string GenerateHash(object obj)
        {
            string hash = "";
            foreach (__Field i in obj.GetEntity().Internals)
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

            foreach (__Field i in obj.GetEntity().Externals)
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



        public override void Set(object obj)
        {
            base.Set(obj);
            if (obj != null)
            {
                GetHash(obj.GetType())[obj.GetEntity().PrimaryKey.GetValue(obj)] = GenerateHash(obj);
            }
        }

        public override void Remove(object obj)
        {
            base.Remove(obj);
            GetHash(obj.GetType()).Remove(obj.GetEntity().PrimaryKey.GetValue(obj));
        }

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
