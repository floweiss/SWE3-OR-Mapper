using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public bool IsNullable { get; internal set; } = false;

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
                return Type._GetEntity(); // ToDo: finish
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

            if (Type == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            if (Type == typeof(short))
            {
                return Convert.ToInt32(value);
            }
            if (Type == typeof(long))
            {
                return Convert.ToInt32(value);
            }

            if (Type.IsEnum)
            {
                return Enum.ToObject(Type, value);
            }

            return value;
        }
    }
}
