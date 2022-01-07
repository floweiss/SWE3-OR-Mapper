using System;
using System.Collections;
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
    /// <summary> Class implementing OR framework functionalities </summary>
    public static class Orm
    {
        /// <summary>Entities.</summary>
        private static Dictionary<Type, Entity> _entities = new Dictionary<Type, Entity>();

        /// <summary> Database connection used by the framework </summary>
        public static IDbConnection Connection { get; set; }

        /// <summary> Cache used by the framework </summary>
        public static ICache Cache { get; set; }

        /// <summary> Returns an entity for a given object </summary>
        /// <param name="obj"> Object </param>
        /// <returns> Entity for the object </returns>
        internal static Entity GetEntity(this object obj)
        {
            Type type = ((obj is Type) ? (Type) obj : obj.GetType());

            if (!_entities.ContainsKey(type))
            {
                _entities.Add(type, new Entity(type));
            }

            return _entities[type];
        }



        /// <summary> Saves an object </summary>
        /// <param name="obj"> Object </param>
        public static void Save(object obj)
        {
            if (Cache != null && !Cache.Changed(obj))
            {
                return;
            }

            Entity ent = obj.GetEntity();
            CreateTable(obj);

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
            
            foreach(Field i in ent.Externals) { i.UpdateReferences(obj); }
        }
        
        /// <summary> Deletes an object </summary>
        /// <param name="obj"> Object </param>
        public static void Delete(object obj)
        {
            Entity ent = obj.GetEntity();
            CreateTable(obj);
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = "DELETE FROM " + ent.TableName + " WHERE " + ent.PrimaryKey.ColumnName + " = :pk";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = (":pk");
            p.Value = ent.PrimaryKey.GetValue(obj);
            cmd.Parameters.Add(p);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            foreach (Field externalField in ent.Externals)
            {
                IDbCommand cmdExt = Connection.CreateCommand();
                IDataParameter pExt = cmdExt.CreateParameter();
                if (externalField.IsManyToMany)
                {
                    cmdExt.CommandText = "DELETE FROM " + externalField.AssignmentTable + " WHERE " + externalField.ColumnName + " = :fk";
                    pExt.ParameterName = (":fk");
                    pExt.Value = ent.PrimaryKey.GetValue(obj);
                    cmdExt.Parameters.Add(pExt);
                }
                else
                {
                    cmdExt.CommandText = externalField.DeleteFkSql;
                    pExt.ParameterName = (":fk");
                    pExt.Value = ent.PrimaryKey.GetValue(obj);
                    cmdExt.Parameters.Add(pExt);
                }
                
                cmdExt.ExecuteNonQuery();
                cmdExt.Dispose();
            }

            if (Cache != null)
            {
                Cache.Remove(obj);
            }
        }

        /// <summary> Counts stored objects of a given type </summary>
        /// <typeparam name="T"> Type </typeparam>
        /// <returns> Number of stored objects </returns>
        public static int Count<T>()
        {
            Type type = typeof(T);
            CreateTable(Activator.CreateInstance(type));
            IDbCommand cmd = Connection.CreateCommand();
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

        /// <summary> Gets an object of a type by its primary key </summary>
        /// <typeparam name="T"> Type </typeparam>
        /// <param name="pk"> Primary key </param>
        /// <returns> Object </returns>
        public static T Get<T>(object pk)
        {
            Type type = typeof(T);
            CreateTable(Activator.CreateInstance(type));
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = type.GetEntity().GetSQLQuery() + " WHERE " + type.GetEntity().PrimaryKey.ColumnName + " = :pk";

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = (":pk");
            p.Value = pk;
            cmd.Parameters.Add(p);

            IDataReader reader = cmd.ExecuteReader();
            cmd.Dispose();
            object obj = null;
            if (reader.Read())
            {
                Entity ent = type.GetEntity();
                obj = SearchCache(type, ent.PrimaryKey.ToFieldType(reader.GetValue(reader.GetOrdinal(ent.PrimaryKey.ColumnName))));

                if (obj == null)
                {
                    obj = Activator.CreateInstance(type);
                }
                else
                {
                    reader.Close();
                    return (T)obj;
                }

                List<object> readerObjects = new List<object>();
                foreach (Field i in ent.Internals)
                {
                    readerObjects.Add(reader.GetValue(reader.GetOrdinal(i.ColumnName)));
                }
                reader.Close();
                foreach (Field i in ent.Internals)
                {
                    object value = i.ToFieldType(readerObjects[0]);
                    readerObjects.RemoveAt(0);
                    i.SetValue(obj, value);
                }

                foreach (Field i in ent.Externals)
                {
                    object list = Activator.CreateInstance(i.Type);
                    i.SetValue(obj, i.FillExternals(list, obj));
                }
            }
            else
            {
                reader.Close();
            }

            if (Cache != null)
            {
                Cache.Set(obj);
            }
            return (T) obj;
        }

        /// <summary> Gets all objects of a type </summary>
        /// <typeparam name="T"> Type </typeparam>
        /// <returns> List of objects </returns>
        public static List<T> GetAll<T>()
        {
            Type type = typeof(T);
            CreateTable(Activator.CreateInstance(type));
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = type.GetEntity().GetSQLQuery();

            List<T> objects = new List<T>();
            object obj = null;
            Entity ent = type.GetEntity();
            List<object>[] readerObjects = new List<object>[Count<T>()];
            int counter = 0;
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                readerObjects[counter] = new List<object>();
                foreach (Field i in ent.Internals)
                {
                    readerObjects[counter].Add(reader.GetValue(reader.GetOrdinal(i.ColumnName)));
                }

                counter++;
            }

            reader.Close();


            foreach (var readerObject in readerObjects)
            {
                obj = Activator.CreateInstance(type);

                foreach (Field i in ent.Internals)
                {
                    object value = i.ToFieldType(readerObject[0]);
                    readerObject.RemoveAt(0);
                    i.SetValue(obj, value);
                }

                foreach (Field i in ent.Externals)
                {
                    object list = Activator.CreateInstance(i.Type);
                    i.SetValue(obj, i.FillExternals(list, obj));
                }

                objects.Add((T)obj);
            }

            cmd.Dispose();

            return objects;
        }


        /// <summary> Searches the cached objects for an object and returns it if it exists </summary>
        /// <param name="t"> Type </param>
        /// <param name="pk"> Primary key </param>
        /// <returns> Returns the cached object that contains the primary key. Returns NULL if no such object has been found </returns>
        internal static object SearchCache(Type t, object pk)
        {
            if (Cache != null && Cache.ContainsKey(t, pk))
            {
                return Cache.Get(t, pk);
            }

            return null;
        }

        /// <summary> Creates an object of a given type from a database reader </summary>
        /// <typeparam name="type"> Type </typeparam>
        /// <param name="reader"> Reader </param>
        /// <param name="inLoop"> Flag indicating if the method is called in a loop </param>
        /// <returns>Object.</returns>
        internal static object CreateObject(Type type, IDataReader reader, bool inLoop = false)
        {
            Entity ent = type.GetEntity();
            object obj = SearchCache(type, ent.PrimaryKey.ToFieldType(reader.GetValue(reader.GetOrdinal(ent.PrimaryKey.ColumnName))));

            if (obj == null)
            {
                obj = Activator.CreateInstance(type);
            }

            List<object> readerObjects = new List<object>();
            foreach (Field i in ent.Internals)
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

            foreach (Field i in ent.Internals)
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

            foreach (Field i in ent.Externals)
            {
                i.SetValue(obj, i.FillExternals(Activator.CreateInstance(i.Type), obj));
            }

            return obj;
        }

        /// <summary> Creates an object of a type by its primary key </summary>
        /// <param name="type">Type.</param>
        /// <param name="pk"> Primary key </param>
        /// <returns> Object </returns>
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
        
        /// <summary> Fills a list with object of a type from a database reader </summary>
        /// <param name="type"> Type </param>
        /// <param name="list"> List </param>
        /// <param name="reader"> Reader </param>
        internal static void FillList(Type type, object list, IDataReader reader)
        {
            while(reader.Read())
            {
                list.GetType().GetMethod("Add").Invoke(list, new object[] { CreateObject(type, reader) });
            }
        }

        /// <summary> Creates the database table for an object if the table does not exist </summary>
        /// <param name="obj"> Object </param>
        internal static void CreateTable(object obj)
        {
            Entity ent = obj.GetEntity();
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = ("CREATE TABLE IF NOT EXISTS " + ent.TableName + " (");

            int i = 0;
            foreach (Field field in ent.Internals)
            {
                cmd.CommandText += field.ColumnName.ToLower() + " " + field.ToDatabaseType();
                i++;
                if (field.IsPrimaryKey)
                {
                    cmd.CommandText += " NOT NULL PRIMARY KEY";
                }
                if (i < ent.Internals.Length)
                {
                    cmd.CommandText += ", ";
                }
            }

            i = 0;
            string assignmentTableSql = null;
            List<object> externals = new List<object>();
            foreach (Field field in ent.Externals)
            {
                if (field.AssignmentTable != null)
                {
                    assignmentTableSql += "CREATE TABLE IF NOT EXISTS " + field.AssignmentTable + "(" +
                                         field.ColumnName + " " + ent.PrimaryKey.ToDatabaseType() + ", " +
                                         field.RemoteColumnName + " ";
                    if (field.Type.GetInterface(nameof(IEnumerable)) != null)
                    {
                        assignmentTableSql += Activator.CreateInstance(field.Type.GetGenericArguments()[0]).GetEntity().PrimaryKey
                            .ToDatabaseType();
                    }
                    else
                    {
                        assignmentTableSql += field.Type;
                    }

                    assignmentTableSql += ")";
                }
                else
                {
                    if (field.Type.GetInterface(nameof(IEnumerable)) != null)
                    {
                        externals.Add(Activator.CreateInstance(field.Type.GetGenericArguments()[0]));
                    }
                    else
                    {
                        externals.Add(Activator.CreateInstance(field.Type));
                    }
                }
            }
            
            cmd.CommandText += ")";
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            if (assignmentTableSql != null)
            {
                IDbCommand cmdAssignment = Connection.CreateCommand();
                cmdAssignment.CommandText = assignmentTableSql;
                cmdAssignment.ExecuteNonQuery();
                cmdAssignment.Dispose();
            }

            externals.ForEach(CreateTable);
        }
    }
}
