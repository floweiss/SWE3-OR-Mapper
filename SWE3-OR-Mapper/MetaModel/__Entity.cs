using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.MetaModel
{
    internal class __Entity
    {
        public __Entity(Type type)
        {
            EntityAttribute typeAttr = (EntityAttribute) type.GetCustomAttribute(typeof(EntityAttribute));
            if ((typeAttr == null) || (string.IsNullOrWhiteSpace(typeAttr.TableName)))
            {
                TableName = type.Name.ToUpper();
            }
            else
            {
                TableName = typeAttr.TableName;
            }

            Member = type;

            List<__Field> fields = new List<__Field>();
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if ((IgnoreAttribute) info.GetCustomAttribute(typeof(IgnoreAttribute)) == null) continue;

                __Field field = new __Field(this);

                FieldAttribute fieldAttribute = (FieldAttribute) info.GetCustomAttribute(typeof(FieldAttribute));

                if (fieldAttribute != null)
                {
                    if (fieldAttribute is PrimaryKeyAttribute)
                    {
                        (PrimaryKey = field).IsPrimaryKey = true;
                    }

                    field.ColumnName = (fieldAttribute?.ColumnName ?? info.Name);
                    field.ColumnType = (fieldAttribute?.ColumnType ?? info.PropertyType);
                    field.IsNullable = fieldAttribute.Nullable;
                    field.IsForeignKey = (fieldAttribute is ForeignKeyAttribute);
                }
                else
                {
                    if ((info.GetGetMethod() == null) || (!info.GetGetMethod().IsPublic)) continue;

                    field.ColumnName = info.Name.ToUpper();
                    field.ColumnType = info.PropertyType;
                    field.IsNullable = true;
                }

                fields.Add(field);
            }
            Fields = fields.ToArray();
        }


        public Type Member { get; private set; }

        public string TableName { get; private set; }

        public __Field[] Fields { get; private set; }

        public __Field PrimaryKey { get; private set; }

        public string GetSQLQuery(string prefix = null)
        {
            if (prefix == null)
            {
                prefix = "";
            }

            string rval = "SELECT ";
            for (int i = 0; i < Fields.Length; i++)
            {
                if (i > 0)
                {
                    rval += ", ";
                }

                rval += prefix.Trim() + Fields[i].ColumnName;
            }

            rval += " FROM " + TableName;
            return rval;
        }
    }
}
