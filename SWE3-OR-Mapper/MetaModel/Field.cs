using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SWE3_OR_Mapper.MetaModel
{
    /// <summary> Class containing field metadata </summary>
    internal class Field
    {
        /// <summary> Creates a new instance of this class with given entity </summary>
        /// <param name="entity"> Parent entity </param>
        public Field(Entity entity)
        {
            Entity = entity;
        }

        /// <summary> Gets the parent entity </summary>
        public Entity Entity { get; internal set; }

        /// <summary> Gets the field member </summary>
        public MemberInfo Member { get; internal set; }

        /// <summary> Gets the field type </summary>
        public Type Type
        {
            get
            {
                if (Member is PropertyInfo)
                {
                    return ((PropertyInfo) Member).PropertyType;
                }

                throw new NotSupportedException();
            }
        }

        /// <summary> Gets the column name in the table </summary>
        public string ColumnName { get; internal set; }

        /// <summary> Gets the column type in the database </summary>
        public Type ColumnType{ get; internal set; }

        /// <summary> Checks if the field is the primary key </summary>
        public bool IsPrimaryKey { get; internal set; } = false;

        /// <summary> Checks if the field is a foreign key </summary>
        public bool IsForeignKey { get; internal set; } = false;

        /// <summary> Assignment table name for M to N relations </summary>
        public string AssignmentTable
        {
            get; internal set;
        }

        /// <summary> Remote column name </summary>
        public string RemoteColumnName
        {
            get; internal set;
        }

        /// <summary> Checks if the field belongs to an M to N relation </summary>
        public bool IsManyToMany
        {
            get; internal set;
        }

        /// <summary> Checks if the field is nullable </summary>
        public bool IsNullable
        {
            get; internal set;
        } = false;

        /// <summary> Checks if the field is external </summary>
        public bool IsExternal
        {
            get; internal set;
        } = false;
        
        
        
        /// <summary> Gets the foreign key SQL for deleting objects </summary>
        /// /// <returns> SQL string for deleting </returns>
        internal string DeleteFkSql
        {
            get
            {
                if(IsManyToMany)
                {
                    return "SELECT " + Type.GenericTypeArguments[0].GetEntity().PrimaryKey.ColumnName + " FROM " +
                           Type.GenericTypeArguments[0].GetEntity().TableName;
                }

                return "DELETE FROM " + Type.GenericTypeArguments[0].GetEntity().TableName + " WHERE " + ColumnName + " = :fk";
            }
        }



        /// <summary> Gets the value of the field </summary>
        /// <param name="obj"> Object </param>
        /// <returns> Field value </returns>
        public object GetValue(object obj)
        {
            if (Member is PropertyInfo)
            {
                return ((PropertyInfo) Member).GetValue(obj);
            }

            throw new NotSupportedException("Member type not supported!");
        }

        /// <summary> Sets the value of the field </summary>
        /// <param name="obj"> Object </param>
        /// <param name="value"> Value </param>
        public void SetValue(object obj, object value)
        {
            if (Member is PropertyInfo)
            {
                ((PropertyInfo)Member).SetValue(obj, value);
            }
            else
            {
                throw new NotSupportedException("Member type not supported!");
            }
        }

        /// <summary> Convert a field type value to a database column type </summary>
        /// <param name="value"> Value to be converted </param>
        /// <returns> Database type representation of the value </returns>
        public object ToColumnType(object value)
        {
            if (IsForeignKey)
            {
                return Type.GetEntity().PrimaryKey.ToColumnType(Type.GetEntity().PrimaryKey.GetValue(value));
            }

            if (value is bool)
            {
                if (ColumnType == typeof(int))
                {
                    return ((bool) value ? 1 : 0);
                }
                if (ColumnType == typeof(short))
                {
                    return ((bool)value ? 1 : 0);
                }
                if (ColumnType == typeof(long))
                {
                    return ((bool)value ? 1 : 0);
                }
            }

            return value;
        }

        /// <summary>Converts a database column type value to a field type </summary>
        /// <param name="value"> Value </param>
        /// <returns> Field type representation of the value </returns>
        public object ToFieldType(object value)
        {
            if (IsForeignKey)
            {
                return Orm.GetObject(Type, value);
            }

            if (Type == typeof(bool))
            {
                if (value is int)
                {
                    return ((int) value != 0);
                }
                if (value is short)
                {
                    return ((short)value != 0);
                }
                if (value is long)
                {
                    return ((long)value != 0);
                }
            }

            if (Type == typeof(short))
            {
                return Convert.ToInt16(value);
            }
            if (Type == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            if (Type == typeof(long))
            {
                return Convert.ToInt64(value);
            }

            if (Type.IsEnum)
            {
                return Enum.ToObject(Type, value);
            }

            return value;
        }

        /// <summary> Converts the column type to a string representation of the database type. Used for creating tables </summary>
        /// <returns> String representation of the database type </returns>
        public string ToDatabaseType()
        {
            if (ColumnType == typeof(bool))
            {
                return "boolean";
            }
            if (ColumnType == typeof(short))
            {
                return "smallint";
            }
            if (ColumnType == typeof(int))
            {
                return "integer";
            }
            if (ColumnType == typeof(long))
            {
                return "bigint";
            }
            if (ColumnType.IsEnum)
            {
                return "integer";
            }
            if (ColumnType == typeof(DateTime))
            {
                return "date";
            }

            return "varchar";
        }

        /// <summary> Fills a list with externals for a foreign key </summary>
        /// <param name="list"> List </param>
        /// <param name="obj"> object </param>
        /// <returns> Filled List </returns>
        public object FillExternals(object list, object obj)
        {
            IDbCommand cmd = Orm.Connection.CreateCommand();

            if (IsManyToMany)
            {
                cmd.CommandText = Type.GenericTypeArguments[0].GetEntity().GetSQLQuery() +
                                  " WHERE ID IN (SELECT " + RemoteColumnName + " FROM " + AssignmentTable + " WHERE " + ColumnName + " = :fk)";
            }
            else
            {
                cmd.CommandText = Type.GenericTypeArguments[0].GetEntity().GetSQLQuery() + " WHERE " + ColumnName + " = :fk";
            }

            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = ":fk";
            p.Value = Entity.PrimaryKey.GetValue(obj);
            cmd.Parameters.Add(p);

            IDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                Type type = Type.GenericTypeArguments[0];
                list.GetType().GetMethod("Add").Invoke(list, new object[] { Orm.CreateObject(type, re, true) });
                if (re.IsClosed)
                {
                    break;
                }
            }
            re.Close();
            re.Dispose();
            cmd.Dispose();

            if (IsManyToMany)
            {
                IDbCommand cmdExt = Orm.Connection.CreateCommand();
                cmdExt.CommandText = "DELETE FROM " + AssignmentTable + " WHERE " + RemoteColumnName + " NOT IN (" + DeleteFkSql + ")";
                IDataParameter pExt = cmd.CreateParameter();
                pExt.ParameterName = ":fk";
                pExt.Value = Entity.PrimaryKey.GetValue(obj);
                cmdExt.Parameters.Add(pExt);
                cmdExt.ExecuteNonQuery();
                cmdExt.Dispose();
            }

            return list;
        }
        
        /// <summary> Updates external field references </summary>
        /// <param name="obj"> Object </param>
        public void UpdateReferences(object obj)
        {
            if(!IsExternal) return;

            Type innerType = Type.GetGenericArguments()[0];
            Entity innerEntity = innerType.GetEntity();
            object pk = Entity.PrimaryKey.ToColumnType(Entity.PrimaryKey.GetValue(obj));

            if(IsManyToMany)
            {
                IDbCommand cmd = Orm.Connection.CreateCommand();
                cmd.CommandText = ("DELETE FROM " + AssignmentTable + " WHERE " + ColumnName + " = :pk");
                IDataParameter p = cmd.CreateParameter();
                p.ParameterName = ":pk";
                p.Value = pk;
                cmd.Parameters.Add(p);

                cmd.ExecuteNonQuery();
                cmd.Dispose();

                if(GetValue(obj) != null)
                {
                    foreach(object i in (IEnumerable) GetValue(obj))
                    {
                        cmd = Orm.Connection.CreateCommand();
                        cmd.CommandText = ("INSERT INTO " + AssignmentTable + "(" + ColumnName + ", " + RemoteColumnName + ") VALUES (:pk, :fk)");
                        p = cmd.CreateParameter();
                        p.ParameterName = ":pk";
                        p.Value = pk;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":fk";
                        p.Value = innerEntity.PrimaryKey.ToColumnType(innerEntity.PrimaryKey.GetValue(i));
                        cmd.Parameters.Add(p);

                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                }
            }
            else
            {
                Field remoteField = innerEntity.GetFieldForColumn(ColumnName);

                if(remoteField.IsNullable)
                {
                    try
                    {
                        IDbCommand cmd = Orm.Connection.CreateCommand();
                        cmd.CommandText = ("UPDATE " + innerEntity.TableName + " SET " + ColumnName + " = NULL WHERE " + ColumnName + " = :fk");
                        IDataParameter p = cmd.CreateParameter();
                        p.ParameterName = ":fk";
                        p.Value = pk;
                        cmd.Parameters.Add(p);

                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                    catch(Exception) {}
                }

                if(GetValue(obj) != null)
                {
                    foreach(object i in (IEnumerable) GetValue(obj))
                    {
                        remoteField.SetValue(i, obj);

                        IDbCommand cmd = Orm.Connection.CreateCommand();
                        cmd.CommandText = ("UPDATE " + innerEntity.TableName + " SET " + ColumnName + " = :fk WHERE " + innerEntity.PrimaryKey.ColumnName + " = :pk");
                        IDataParameter p = cmd.CreateParameter();
                        p.ParameterName = ":fk";
                        p.Value = pk;
                        cmd.Parameters.Add(p);

                        p = cmd.CreateParameter();
                        p.ParameterName = ":pk";
                        p.Value = innerEntity.PrimaryKey.ToColumnType(innerEntity.PrimaryKey.GetValue(i));
                        cmd.Parameters.Add(p);

                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                }
            }
        }
    }
}
