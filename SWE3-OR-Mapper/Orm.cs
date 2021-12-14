using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.Cache;
using SWE3_OR_Mapper.MetaModel;

namespace SWE3_OR_Mapper
{
    public static class Orm
    {
        private static Dictionary<Type, __Entity> _entities = new Dictionary<Type, __Entity>();

        public static IDbConnection Connection { get; set; }

        public static ICache Cache { get; set; }

        internal static __Entity GetEntity(this object obj)
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
            if (Cache != null && !Cache.Changed(obj))
            {
                return;
            }

            __Entity ent = obj.GetEntity();

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

            if (Cache != null)
            {
                Cache.Set(obj);
            }
            
            foreach(__Field i in ent.Externals) { i.UpdateReferences(obj); }
        }

        public static int Count<T>()
        {
            IDbCommand cmd = Connection.CreateCommand();

            Type type = typeof(T);
            cmd.CommandText = type.GetEntity().GetCountSQLQuery();

            IDataReader reader = cmd.ExecuteReader();
            int counter = 0;
            while (reader.Read())
            {
                counter++;
            }

            reader.Close();
            cmd.Dispose();

            return counter;
        }

        public static T Get<T>(object pk)
        {
            IDbCommand cmd = Connection.CreateCommand();
            
            Type type = typeof(T);
            cmd.CommandText = type.GetEntity().GetSQLQuery() + " WHERE " + type.GetEntity().PrimaryKey.ColumnName + " = :pk";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = (":pk");
            p.Value = pk;
            cmd.Parameters.Add(p);

            IDataReader reader = cmd.ExecuteReader();
            object obj = null;
            if (reader.Read())
            {
                __Entity ent = type.GetEntity();
                obj = SearchCache(type, ent.PrimaryKey.ToFieldType(reader.GetValue(reader.GetOrdinal(ent.PrimaryKey.ColumnName))));

                if (obj == null)
                {
                    obj = Activator.CreateInstance(type);
                }
                else
                {
                    reader.Close();
                    cmd.Dispose();
                    return (T)obj;
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
                    object list = Activator.CreateInstance(i.Type);
                    i.SetValue(obj, i.FillExternals(list, obj));
                }
            }
            
            cmd.Dispose();

            if (Cache != null)
            {
                Cache.Set(obj);
            }
            return (T) obj;
        }

        public static List<T> GetAll<T>()
        {
            IDbCommand cmd = Connection.CreateCommand();

            Type type = typeof(T);
            cmd.CommandText = type.GetEntity().GetSQLQuery();

            List<T> objects = new List<T>();
            object obj = null;
            __Entity ent = type.GetEntity();
            List<object>[] readerObjects = new List<object>[Count<T>()];
            int counter = 0;
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                readerObjects[counter] = new List<object>();
                foreach (__Field i in ent.Internals)
                {
                    readerObjects[counter].Add(reader.GetValue(reader.GetOrdinal(i.ColumnName)));
                }

                counter++;
            }

            reader.Close();


            foreach (var readerObject in readerObjects)
            {
                obj = Activator.CreateInstance(type);

                foreach (__Field i in ent.Internals)
                {
                    object value = i.ToFieldType(readerObject[0]);
                    readerObject.RemoveAt(0);
                    i.SetValue(obj, value);
                }

                foreach (__Field i in ent.Externals)
                {
                    object list = Activator.CreateInstance(i.Type);
                    i.SetValue(obj, i.FillExternals(list, obj));
                }

                objects.Add((T)obj);
            }

            cmd.Dispose();

            return objects;
        }


        internal static object SearchCache(Type t, object pk)
        {
            if (Cache != null && Cache.ContainsKey(t, pk))
            {
                return Cache.Get(t, pk);
            }

            return null;
        }

        internal static object CreateObject(Type type, IDataReader reader, bool inLoop = false)
        {
            __Entity ent = type.GetEntity();
            object obj = SearchCache(type, ent.PrimaryKey.ToFieldType(reader.GetValue(reader.GetOrdinal(ent.PrimaryKey.ColumnName))));

            if (obj == null)
            {
                //if (_localCache == null) { _localCache = new List<object>(); }
                obj = Activator.CreateInstance(type);
                //_localCache.Add(obj);
            }

            List<object> readerObjects = new List<object>();
            foreach (__Field i in ent.Internals)
            {
                if (i.IsForeignKey && inLoop)
                {
                    continue;
                }
                readerObjects.Add(reader.GetValue(reader.GetOrdinal(i.ColumnName)));
            }

            if (ent.Externals.Length > 0)
            {
                reader.Close();
            }

            foreach (__Field i in ent.Internals)
            {
                if (i.IsForeignKey && inLoop)
                {
                    continue;
                }
                object value = i.ToFieldType(readerObjects[0]);
                readerObjects.RemoveAt(0);
                i.SetValue(obj, value);
            }

            if (!inLoop)
            {
                reader.Close();
            }

            foreach (__Field i in ent.Externals)
            {
                i.SetValue(obj, i.FillExternals(Activator.CreateInstance(i.Type), obj));
            }

            return obj;
        }

        internal static object GetObject(Type type, object pk)
        {
            object obj = SearchCache(type, pk);

            if (obj == null)
            {
                IDbCommand cmd = Connection.CreateCommand();

                cmd.CommandText = type.GetEntity().GetSQLQuery() + " WHERE " + type.GetEntity().PrimaryKey.ColumnName + " = :pk";

                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = (":pk");
                p.Value = pk;
                cmd.Parameters.Add(p);

                IDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    obj = CreateObject(type, reader);
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
        
        internal static void FillList(Type t, object list, IDataReader re)
        {
            while(re.Read())
            {
                list.GetType().GetMethod("Add").Invoke(list, new object[] { CreateObject(t, re) });
            }
        }
        
        internal static void FillList(Type t, object list, string sql, IEnumerable<Tuple<string, object>> parameters)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = sql;

            foreach(Tuple<string, object> i in parameters)
            {
                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = i.Item1;
                p.Value = i.Item2;
                cmd.Parameters.Add(p);
            }

            IDataReader re = cmd.ExecuteReader();
            FillList(t, list, re);
            re.Close();
            re.Dispose();
            cmd.Dispose();
        }
    }
}
