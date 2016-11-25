using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CodeBox.Core.CommandModel
{
    public static class Converter
    {
        public static bool Convert(object obj, Type targetType, out object result)
        {
            result = null;

            if (obj == null)
                return false;

            if (obj is string && targetType == typeof(string))
                result = obj;
            else if (obj is string && targetType != typeof(string))
            {
                var i4 = 0;
                var r4 = .0f;
                var r8 = .0;

                if (targetType == typeof(int) && int.TryParse(obj as string, out i4))
                    result = i4;
                else if (targetType == typeof(float) && float.TryParse(obj as string, out r4))
                    result = r4;
                else if (targetType == typeof(double) && double.TryParse(obj as string, out r8))
                    result = r8;
                else if (targetType == typeof(bool) && StringComparer.OrdinalIgnoreCase.Equals(obj as string, bool.TrueString))
                    result = StringComparer.OrdinalIgnoreCase.Equals(obj as string, bool.TrueString);
                else if (targetType == typeof(Color))
                {
                    try
                    {
                        result = new ColorConverter().ConvertFromString(obj as string);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else if (targetType.IsEnum)
                {
                    try
                    {
                        result = Enum.Parse(targetType, obj as string, true);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else if (targetType == typeof(Encoding))
                {
                    result = Encoding.GetEncodings()
                        .FirstOrDefault(e => e.Name.Equals((string)obj, StringComparison.OrdinalIgnoreCase))
                        ?.GetEncoding()
                        ?? Encoding.UTF8;
                }
                else
                    return false;
            }
            else if (targetType.IsAssignableFrom(obj.GetType()))
            {
                result = obj;
                return true;
            }
            else
            {
                try
                {
                    result = System.Convert.ChangeType(obj, targetType);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
