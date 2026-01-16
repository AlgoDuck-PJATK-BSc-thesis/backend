using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AlgoDuck.Shared.Extensions;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions WithInternalFields(this JsonSerializerOptions options)
    {
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { AddInternalMembersModifier }
        };
        
        return options;
    }
    
    private static void AddInternalMembersModifier(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var property in typeInfo.Type.GetProperties(
                     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (typeInfo.Properties.Any(p => p.Name == property.Name))
                continue;
            
            var getter = property.GetGetMethod(true);
            var setter = property.GetSetMethod(true);
            
            var isInternal = (getter?.IsAssembly == true || setter?.IsAssembly == true);
            if (!isInternal)
                continue;

            var jsonProp = typeInfo.CreateJsonPropertyInfo(property.PropertyType, property.Name);
            jsonProp.Get = property.CanRead ? property.GetValue : null;
            jsonProp.Set = property.CanWrite ? property.SetValue : null;
            typeInfo.Properties.Add(jsonProp);
        }

        foreach (var field in typeInfo.Type.GetFields(
                     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (field is { IsAssembly: false, IsFamily: false, IsFamilyOrAssembly: false }) 
                continue;
            
            if (typeInfo.Properties.Any(p => p.Name == field.Name))
                continue;
                
            var prop = typeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
            prop.Get = field.GetValue;
            prop.Set = field.SetValue;
            typeInfo.Properties.Add(prop);
        }
    }
}