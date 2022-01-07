using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.Attributes;

namespace SWE3_OR_Mapper.MetaModel
{
    /// <summary> Class containing entity metadata </summary>
    internal class Entity
    {
        /// <summary> Creates a new instance of this class with given type </summary>
        /// <param name="t"> Type </param>
        public Entity(Type type)
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

            List<Field> fields = new List<Field>();
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if ((IgnoreAttribute) info.GetCustomAttribute(typeof(IgnoreAttribute)) != null) continue;

                Field field = new Field(this);

                FieldAttribute fieldAttribute = (FieldAttribute) info.GetCustomAttribute(typeof(FieldAttribute));

                if (fieldAttribute != null)
                {
                    if (fieldAttribute is PrimaryKeyAttribute)
                    {
                        PrimaryKey = field;
                        field.IsPrimaryKey = true;
                    }

                    field.ColumnName = (fieldAttribute?.ColumnName ?? info.Name);
                    field.ColumnType = (fieldAttribute?.ColumnType ?? info.PropertyType);
                    field.IsNullable = fieldAttribute.Nullable;

                    if (field.IsForeignKey = (fieldAttribute is ForeignKeyAttribute))
                    {
                        field.IsExternal = typeof(IEnumerable).IsAssignableFrom(info.PropertyType);

                        field.AssignmentTable = ((ForeignKeyAttribute)fieldAttribute).AssignmentTable;
                        field.RemoteColumnName = ((ForeignKeyAttribute)fieldAttribute).RemoteColumnName;
                        field.IsManyToMany = (!string.IsNullOrWhiteSpace(field.AssignmentTable));
                    }
                }
                else
                {
                    if ((info.GetGetMethod() == null) || (!info.GetGetMethod().IsPublic)) continue;

                    field.ColumnName = info.Name;
                    field.ColumnType = info.PropertyType;
                    //field.IsNullable = true;
                }

                field.Member = info;
                fields.Add(field);
            }
            Fields = fields.ToArray();
            Internals = fields.Where(m => (!m.IsExternal)).ToArray();
            Externals = fields.Where(m => m.IsExternal).ToArray();
        }


        /// <summary> Gets the type of the member </summary>
        public Type Member { get; private set; }

        /// <summary> Gets the name of the table </summary>
        public string TableName { get; private set; }

        /// <summary> Gets all fields </summary>
        public Field[] Fields { get; private set; }

        /// <summary> Gets external fields. These are not stored in the underlying table </summary>
        public Field[] Externals { get; private set; }
        
        /// <summary> Gets internal fields </summary>
        public Field[] Internals { get; private set; }

        /// <summary> Gets the primary key </summary>
        public Field PrimaryKey { get; private set; }

        /// <summary> Gets the SQL for all fields </summary>
        /// <returns> SQL string </returns>
        public string GetSQLQuery()
        {
            string query = "SELECT ";
            for (int i = 0; i < Internals.Length; i++)
            {
                if (i > 0)
                {
                    query += ", ";
                }

                query += Internals[i].ColumnName;
            }

            query += " FROM " + TableName;
            return query;
        }

        /// <summary> Gets the SQL for count purposes </summary>
        /// <returns> SQL count string </returns>
        public string GetCountSQLQuery()
        {
            return "SELECT " + Internals[0].ColumnName + " FROM " + TableName;
        }

        /// <summary> Gets a field by its column name </summary>
        /// <param name="columnName"> Column name </param>
        /// <returns> Field </returns>
        public Field GetFieldForColumn(string columnName)
        {
            columnName = columnName.ToUpper();
            foreach (Field i in Internals)
            {
                if (i.ColumnName.ToUpper() == columnName) { return i; }
            }

            return null;
        }
    }
}
