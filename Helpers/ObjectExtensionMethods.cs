using CamControl.Attributes;

namespace CamControl.Helpers
{
    public static class ObjectExtensionMethods
    {
        public static void CopyPropertiesFrom(this object self, object parent)

        {

            var fromProperties = parent.GetType().GetProperties();

            var toProperties = self.GetType().GetProperties();


            foreach (var fromProperty in fromProperties)

            {

                foreach (var toProperty in toProperties)

                {

                    if (fromProperty.Name == toProperty.Name && fromProperty.PropertyType == toProperty.PropertyType)

                    {

                        toProperty.SetValue(self, fromProperty.GetValue(parent));

                        break;

                    }

                }

            }

        }

        public static Boolean CopyPropertiesFrom(this object self, object parent, IFormCollection colls)

        {
            Boolean hasChanged = false;
            var fromProperties = parent.GetType().GetProperties();

            var toProperties = self.GetType().GetProperties();
            var selfType = self.GetType();

            foreach (var fromProperty in fromProperties)

            {
                if (!colls.Select(a => a.Key).Contains(selfType.Name + "." + fromProperty.Name))
                    continue;
                foreach (var toProperty in toProperties)

                {

                    if (fromProperty.Name == toProperty.Name && fromProperty.PropertyType == toProperty.PropertyType)

                    {
                        object? newProp = fromProperty.GetValue(parent);
                        object? oldProp = toProperty.GetValue(self);
                        if (newProp != null && oldProp != null && newProp != oldProp)
                        {
                            hasChanged = true;
                            toProperty.SetValue(self, newProp);
                        }

                        break;

                    }

                }

            }
            return hasChanged;
        }
        public static void MatchPropertiesFrom(this object self, object parent)

        {

            var childProperties = self.GetType().GetProperties();

            foreach (var childProperty in childProperties)

            {

                var attributesForProperty = childProperty.GetCustomAttributes(typeof(MatchParentAttribute), true);

                var isOfTypeMatchParentAttribute = false;


                MatchParentAttribute currentAttribute = null;


                foreach (var attribute in attributesForProperty)

                {

                    if (attribute.GetType() == typeof(MatchParentAttribute))

                    {

                        isOfTypeMatchParentAttribute = true;

                        currentAttribute = (MatchParentAttribute)attribute;

                        break;

                    }

                }


                if (isOfTypeMatchParentAttribute)

                {

                    var parentProperties = parent.GetType().GetProperties();

                    object parentPropertyValue = null;

                    foreach (var parentProperty in parentProperties)

                    {

                        if (parentProperty.Name == currentAttribute.ParentPropertyName)

                        {

                            if (parentProperty.PropertyType == childProperty.PropertyType)

                            {

                                parentPropertyValue = parentProperty.GetValue(parent);

                            }

                        }

                    }

                    childProperty.SetValue(self, parentPropertyValue);

                }

            }

        }

        public static void MatchPropertiesFrom(this object self, object parent, IFormCollection colls)

        {

            var childProperties = self.GetType().GetProperties();

            foreach (var childProperty in childProperties)

            {
                if (!colls.Select(a => a.Key).Contains(childProperty.Name))
                    continue;
                var attributesForProperty = childProperty.GetCustomAttributes(typeof(MatchParentAttribute), true);

                var isOfTypeMatchParentAttribute = false;


                MatchParentAttribute currentAttribute = null;


                foreach (var attribute in attributesForProperty)

                {

                    if (attribute.GetType() == typeof(MatchParentAttribute))

                    {

                        isOfTypeMatchParentAttribute = true;

                        currentAttribute = (MatchParentAttribute)attribute;

                        break;

                    }

                }


                if (isOfTypeMatchParentAttribute)

                {

                    var parentProperties = parent.GetType().GetProperties();

                    object parentPropertyValue = null;

                    foreach (var parentProperty in parentProperties)

                    {

                        if (parentProperty.Name == currentAttribute.ParentPropertyName)

                        {

                            if (parentProperty.PropertyType == childProperty.PropertyType)

                            {

                                parentPropertyValue = parentProperty.GetValue(parent);

                            }

                        }

                    }

                    childProperty.SetValue(self, parentPropertyValue);

                }

            }

        }
    }
}
