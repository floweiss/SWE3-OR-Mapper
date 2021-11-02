using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.MetaModel;

namespace SWE3_OR_Mapper
{
    public static class Orm
    {
        private static Dictionary<Type, __Entity> _entities = new Dictionary<Type, __Entity>();

        private static ICollection<object> _localCache = new List<object>();

        public static IDbConnection Connection { get; set; }

        internal static __Entity _GetEntity(this object obj)
        {
            Type type = ((obj is Type) ? (Type) obj : obj.GetType());

            if (!_entities.ContainsKey(type))
            {
                _entities.Add(type, new __Entity(type));
            }

            return _entities[type];
        }



        public static void Save(object obj)
        {
            __Entity ent = obj._GetEntity();
            _localCache.Add(obj);

            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = ("INSERT INTO " + ent.TableName + " (");

            string update = "ON CONFLICT (" + ent.PrimaryKey.ColumnName + ") DO UPDATE SET ";
            string insert = "";

            IDbDataParameter p;
            bool first = true;
            for (int i = 0; i < ent.Internals.Length; i++)
            {
                if (i > 0)
                {
                    cmd.CommandText += ", ";
                    insert += ", ";
                }

                cmd.CommandText += ent.Internals[i].ColumnName;
                insert += ("@v" + i.ToString());

                p = cmd.CreateParameter();
                p.ParameterName = ("@v" + i.ToString());
                p.Value = ent.Internals[i].ToColumnType(ent.Internals[i].GetValue(obj));
                if (p.Value is Enum)
                {
                    p.Value = (int) p.Value;
                }
                cmd.Parameters.Add(p);

                if (!ent.Internals[i].IsPrimaryKey)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        update += ", ";
                    }

                    update += (ent.Internals[i].ColumnName + " = @w" + i.ToString());

                    p = cmd.CreateParameter();
                    p.ParameterName = ("@w" + i.ToString());
                    p.Value = ent.Internals[i].ToColumnType(ent.Internals[i].GetValue(obj));
                    if (p.Value is Enum)
                    {
                        p.Value = (int)p.Value;
                    }
                    cmd.Parameters.Add(p);
                }
            }

            cmd.CommandText += (") VALUES (" + insert + ") " + update);

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public static T Get<T>(object pk)
        {
            IDbCommand cmd = Connection.CreateCommand();
            
            Type type = typeof(T);
            cmd.CommandText = type._GetEntity().GetSQLQuery() + " WHERE " + type._GetEntity().PrimaryKey.ColumnName + " = :pk";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = (":pk");
            p.Value = pk;
            cmd.Parameters.Add(p);

            IDataReader reader = cmd.ExecuteReader();
            object obj = null;
            if (reader.Read())
            {
                //obj = _CreateObject(type, reader, null);

                __Entity ent = type._GetEntity();
                obj = _SearchCache(type, ent.PrimaryKey.ToFieldType(reader.GetValue(reader.GetOrdinal(ent.PrimaryKey.ColumnName))));

                if (obj == null)
                {
                    if (_localCache == null) { _localCache = new List<object>(); }
                    _localCache.Add(obj = Activator.CreateInstance(type));
                }
                else
                {
                    reader.Close();
                    cmd.Dispose();
                    return (T) obj;
                }

                List<object> readerObjects = new List<object>();
                foreach (__Field i in ent.Internals)
                {
                    readerObjects.Add(reader.GetValue(reader.GetOrdinal(i.ColumnName)));
                }
                reader.Close();
                foreach (__Field i in ent.Internals)
                {
                    object value = i.ToFieldType(readerObjects[0]);
                    readerObjects.RemoveAt(0);
                    i.SetValue(obj, value);
                }

                foreach (__Field i in ent.Externals)
                {
                    i.SetValue(obj, i.Fill(Activator.CreateInstance(i.Type), obj));
                }
            }
            
            cmd.Dispose();
            return (T) obj;
        }


        internal static object _SearchCache(Type t, object pk)
        {
            if (_localCache != null)
            {
                foreach (object i in _localCache)
                {
                    if (i.GetType() != t) continue;

                    if (t._GetEntity().PrimaryKey.GetValue(i).Equals(pk)) { return i; }
                }
            }

            return null;
        }

        internal static object _CreateObject(Type type, IDataReader reader, bool inLoop = false)
        {
            __Entity ent = type._GetEntity();
            object obj = _SearchCache(type, ent.PrimaryKey.ToFieldType(reader.GetValue(reader.GetOrdinal(ent.PrimaryKey.ColumnName))));

            if (obj == null)
            {
                if (_localCache == null) { _localCache = new List<object>(); }
                _localCache.Add(obj = Activator.CreateInstance(type));
            }

            foreach (__Field i in ent.Internals)
            {
                object value = i.ToFieldType(reader.GetValue(reader.GetOrdinal(i.ColumnName)));
                if (i.IsForeignKey && !inLoop)
                {
                    reader.Close();
                }
                i.SetValue(obj, value);
            }

            if (!inLoop)
            {
                reader.Close();
            }

            foreach (__Field i in ent.Externals)
            {
                i.SetValue(obj, i.Fill(Activator.CreateInstance(i.Type), obj));
            }

            return obj;
        }

        internal static object _CreateObject(Type type, object pk)
        {
            object obj = _SearchCache(type, pk);

            if (obj == null)
            {
                IDbCommand cmd = Connection.CreateCommand();

                cmd.CommandText = type._GetEntity().GetSQLQuery() + " WHERE " + type._GetEntity().PrimaryKey.ColumnName + " = :pk";

                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = (":pk");
                p.Value = pk;
                cmd.Parameters.Add(p);

                IDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    obj = _CreateObject(type, reader);
                }

                reader.Close();
                cmd.Dispose();
            }

            if (obj == null)
            {
                throw new Exception("No data.");
            }
            return obj;
        }
    }
}
