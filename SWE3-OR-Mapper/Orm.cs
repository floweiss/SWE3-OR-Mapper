using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWE3_OR_Mapper.MetaModel;

namespace SWE3_OR_Mapper
{
    public class Orm
    {
        private static Dictionary<Type, __Entity> _entities = new Dictionary<Type, __Entity>();

        internal static __Entity _GetEntity(object obj)
        {
            Type type = ((obj is Type) ? (Type) obj : obj.GetType());

            if (!_entities.ContainsKey(type))
            {
                _entities.Add(type, new __Entity(type));
            }

            return _entities[type];
        }
    }
}
