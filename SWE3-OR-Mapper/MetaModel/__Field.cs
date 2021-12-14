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
    internal class __Field
    {
        public __Field(__Entity entity)
        {
            Entity = entity;
        }

        public __Entity Entity { get; internal set; }

        public MemberInfo Member { get; internal set; }

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

        public string ColumnName { get; internal set; }

        public Type ColumnType{ get; internal set; }

        public bool IsPrimaryKey { get; internal set; } = false;

        public bool IsForeignKey { get; internal set; } = false;

        public string AssignmentTable
        {
            get; internal set;
        }

        public string RemoteColumnName
        {
            get; internal set;
        }

        public bool IsManyToMany
        {
            get; internal set;
        }

        public bool IsNullable
        {
            get; internal set;
        } = false;

        
        public bool IsExternal
        {
            get; internal set;
        } = false;
        
        
        
        internal string FkSql
        {
            get
            {
                if(IsManyToMany)
                {
                    return Type.GenericTypeArguments[0].GetEntity().GetSQLQuery() + 
                           " WHERE ID IN (SELECT " + RemoteColumnName + " FROM " + AssignmentTable + " WHERE " + ColumnName + " = :fk)";
                }

                return Type.GenericTypeArguments[0].GetEntity().GetSQLQuery() + " WHERE " + ColumnName + " = :fk";
            }
        }



        public object GetValue(object obj)
        {
            if (Member is PropertyInfo)
            {
                return ((PropertyInfo) Member).GetValue(obj);
            }

            throw new NotSupportedException("Member type not supported!");
        }

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

            return list;
        }
        
        public void UpdateReferences(object obj)
        {
            if(!IsExternal) return;

            Type innerType = Type.GetGenericArguments()[0];
            __Entity innerEntity = innerType.GetEntity();
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
                __Field remoteField = innerEntity.GetFieldForColumn(ColumnName);

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
