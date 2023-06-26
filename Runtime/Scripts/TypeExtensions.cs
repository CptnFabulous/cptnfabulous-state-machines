using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public static class TypeExtensions
{
    public static Type[] FindInheritedTypes(Type baseType, bool excludeAbstractTypes)
    {
        Assembly assembly = Assembly.GetAssembly(baseType);
        //AssemblyName[] names = assembly.GetReferencedAssemblies();

        Type[] types = assembly.GetTypes();
        types = types.Where(t =>
        {
            if (t == baseType) return false;
            if (excludeAbstractTypes && t.IsAbstract) return false;
            if (baseType.IsAssignableFrom(t) == false) return false;
            return true;

        }).ToArray();
        return types;
    }
}
