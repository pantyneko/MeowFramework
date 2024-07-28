using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Panty
{
    public class ReflectionKit
    {
        public static FieldInfo GetField(object target, string fieldName)
        {
            return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
        }
        public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        {
            if (target == null)
            {
                "The target object is null. Check for missing scripts.".Log();
                yield break;
            }
            var types = new List<Type>() { target.GetType() };
            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;
            for (int i = types.Count - 1; i >= 0; i--)
            {
                foreach (var fieldInfo in types[i].GetFields(flags).Where(predicate))
                {
                    yield return fieldInfo;
                }
            }
        }
    }
}