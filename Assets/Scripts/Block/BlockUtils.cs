using System;
using System.Collections.Generic;
using System.Linq;

public static class BlockUtils {
    public static IEnumerable<Type> GetAllTypesDerivedFrom<T>() {
#if UNITY_EDITOR && UNITY_2019_2_OR_NEWER
        return UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
#else
            return GetAllAssemblyTypes().Where(t => t.IsSubclassOf(typeof(T)));
#endif
    }
    
    private static IEnumerable<Type> assemblyTypes;

    public static IEnumerable<Type> GetAllAssemblyTypes() {
        if (assemblyTypes == null) {
            assemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => {
                    // Ugly hack to handle mis-versioned dlls
                    var innerTypes = new Type[0];
                    try
                    {
                        innerTypes = t.GetTypes();
                    }
                    catch {}
                    return innerTypes;
                });
        }

        return assemblyTypes;
    }
}