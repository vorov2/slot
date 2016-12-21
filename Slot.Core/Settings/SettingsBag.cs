using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Slot.Core.Settings
{
    using MAP = Dictionary<string, object>;
    using TUP = Tuple<SettingAttribute, PropertyInfo>;

    public abstract class SettingsBag
    {
        private List<TUP> propCache;

        internal void Fill(MAP settings, MAP userSettings, MAP workspaceSettings)
        {
            if (propCache == null)
                propCache = GetType().GetProperties()
                    .Select(pi => Tuple.Create(Attribute
                        .GetCustomAttribute(pi, typeof(SettingAttribute)) as SettingAttribute, pi))
                    .Where(t => t.Item1 != null)
                    .ToList();

            foreach (var t in propCache)
            {
                object val;

                if (workspaceSettings != null && workspaceSettings.TryGetValue(t.Item1.Name, out val))
                    SetPropertyValue(t.Item2, val);
                else if (userSettings != null && userSettings.TryGetValue(t.Item1.Name, out val))
                    SetPropertyValue(t.Item2, val);
                else if (settings != null && settings.TryGetValue(t.Item1.Name, out val))
                    SetPropertyValue(t.Item2, val);
                else if (t.Item2.PropertyType.IsValueType)
                    t.Item2.SetValue(this, Activator.CreateInstance(t.Item2.PropertyType));
                else
                    t.Item2.SetValue(this, null);
            }

            OnSettingsChanged();
        }

        private void SetPropertyValue(PropertyInfo prop, object value)
        {
            if (value is IList && typeof(IList).IsAssignableFrom(prop.PropertyType)
                && prop.PropertyType.GenericTypeArguments.Length > 0)
            {
                var elType = prop.PropertyType.GenericTypeArguments[0];
                var list = (IList)value;
                var propList = (IList)Activator.CreateInstance(prop.PropertyType);

                foreach (var e in list)
                {
                    object res;
                    if (Converter.Convert(e, elType, out res))
                        propList.Add(res);
                }

                prop.SetValue(this, propList);
            }
            else
            {
                object res;
                if (Converter.Convert(value, prop.PropertyType, out res))
                    prop.SetValue(this, res);
            }
        }

        public void UpdateSettings() => OnSettingsChanged();

        public event EventHandler SettingsChanged;
        protected virtual void OnSettingsChanged() => SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
