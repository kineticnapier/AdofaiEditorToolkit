using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace ADOFAI.EditorToolkit
{
    internal static class EventValueConverter
    {
        public static object Convert(object value, Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            var nullableType = Nullable.GetUnderlyingType(targetType);
            var actualTarget = nullableType ?? targetType;

            if (value == null)
            {
                if (targetType.IsValueType && nullableType == null)
                    throw new InvalidCastException("null cannot be assigned to " + targetType.FullName + ".");
                return null;
            }

            if (actualTarget.IsInstanceOfType(value)) return value;

            var components = value as ComponentValue;
            if (components != null) return ConvertComponents(components, actualTarget);

            if (actualTarget.IsEnum)
            {
                var text = value as string;
                if (text != null) return Enum.Parse(actualTarget, text, true);
                var enumBase = Enum.GetUnderlyingType(actualTarget);
                return Enum.ToObject(actualTarget, System.Convert.ChangeType(value, enumBase, CultureInfo.InvariantCulture));
            }

            if (actualTarget == typeof(Guid)) return Guid.Parse(System.Convert.ToString(value, CultureInfo.InvariantCulture));
            if (actualTarget == typeof(string)) return System.Convert.ToString(value, CultureInfo.InvariantCulture);

            var targetConverter = TypeDescriptor.GetConverter(actualTarget);
            if (targetConverter != null && targetConverter.CanConvertFrom(value.GetType()))
                return targetConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);

            var sourceConverter = TypeDescriptor.GetConverter(value.GetType());
            if (sourceConverter != null && sourceConverter.CanConvertTo(actualTarget))
                return sourceConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, actualTarget);

            if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(actualTarget))
                return System.Convert.ChangeType(value, actualTarget, CultureInfo.InvariantCulture);

            throw new InvalidCastException(
                "Cannot convert " + value.GetType().FullName + " to " + actualTarget.FullName + ".");
        }

        private static object ConvertComponents(ComponentValue value, Type targetType)
        {
            var constructors = targetType.GetTypeInfo().DeclaredConstructors;
            foreach (var constructor in constructors)
            {
                if (!constructor.IsPublic) continue;
                var parameters = constructor.GetParameters();
                if (parameters.Length != value.Values.Length) continue;

                try
                {
                    var arguments = new object[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++)
                        arguments[i] = Convert(value.Values[i], parameters[i].ParameterType);
                    return constructor.Invoke(arguments);
                }
                catch
                {
                    // A different overload may be the intended one.
                }
            }

            var instance = Activator.CreateInstance(targetType);
            for (var i = 0; i < value.Names.Length; i++)
            {
                var name = value.Names[i];
                var property = targetType.GetRuntimeProperty(name);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, Convert(value.Values[i], property.PropertyType), null);
                    continue;
                }

                var field = targetType.GetRuntimeField(name);
                if (field == null)
                    throw new InvalidCastException(targetType.FullName + " has no writable component '" + name + "'.");
                field.SetValue(instance, Convert(value.Values[i], field.FieldType));
            }
            return instance;
        }
    }
}
