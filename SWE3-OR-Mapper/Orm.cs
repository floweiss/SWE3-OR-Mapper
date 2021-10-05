using System;
using System.Collections.Generic;
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

            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = ("INSERT INTO " + ent.TableName + " (");

            string update = "ON CONFLICT (" + ent.PrimaryKey.ColumnName + ") DO UPDATE SET ";
            string insert = "";

            IDbDataParameter p;
            bool first = true;
            for (int i = 0; i < ent.Fields.Length; i++)
            {
                if (i > 0)
                {
                    cmd.CommandText += ", ";
                    insert += ", ";
                }

                cmd.CommandText += ent.Fields[i].ColumnName;
                insert += (":v" + i.ToString());

                p = cmd.CreateParameter();
                p.ParameterName = (":v" + i.ToString());
                p.Value = ent.Fields[i].ToColumnType(ent.Fields[i].GetValue(obj));
                cmd.Parameters.Add(p);

                if (!ent.Fields[i].IsPrimaryKey)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        update += ", ";
                    }

                    update += (ent.Fields[i].ColumnName + " = :w" + i.ToString());

                    p = cmd.CreateParameter();
                    p.ParameterName = (":w" + i.ToString());
                    p.Value = ent.Fields[i].ToColumnType(ent.Fields[i].GetValue(obj));
                    cmd.Parameters.Add(p);
                }
            }

            cmd.CommandText += (") VALUES (" + insert + ") " + update);

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public static T Get<T>(object pk)
        {
            return (T) _CreateObject(typeof(T), pk);
        }



        private static object _CreateObject(Type type, IDataReader reader)
        {
            object obj = Activator.CreateInstance(type);

            foreach (__Field i in type._GetEntity().Fields)
            {
                i.SetValue(obj, i.ToFieldType(reader.GetValue(reader.GetOrdinal(i.ColumnName))));
            }

            return obj;
        }

        private static object _CreateObject(Type type, object pk)
        {
            IDbCommand cmd = Connection.CreateCommand();

            cmd.CommandText = type._GetEntity().GetSQLQuery() + " WHERE " + type._GetEntity().PrimaryKey.ColumnName + " = :pk";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = ":pk";
            p.Value = pk;
            cmd.Parameters.Add(p);

            object obj = null;
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                obj = _CreateObject(type, reader);
            }
            reader.Close();
            reader.Dispose();
            cmd.Dispose();

            if (obj == null)
            {
                throw new Exception("No data");
            }

            return obj;
        }
    }
}
