using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

public static class TypeExtensions
{
    public static IEnumerable<Type> FindInheritedTypesInAssembly(Assembly assembly, Type baseType, bool excludeAbstractTypes)
    {
        return assembly.GetTypes().Where(t =>
        {
            if (t == baseType) return false;
            if (excludeAbstractTypes && t.IsAbstract) return false;
            if (baseType.IsAssignableFrom(t) == false) return false;
            return true;
        });
    }
    public static List<Type> FindAllInheritedTypesInProject(Type baseType, bool excludeAbstractTypes)
    {
        List<Type> allTypes = new List<Type>();
        
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        // foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            allTypes.AddRange(FindInheritedTypesInAssembly(assembly, baseType, excludeAbstractTypes));
        }
        return allTypes;
    }
}
