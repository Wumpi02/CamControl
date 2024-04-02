﻿namespace CamControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]

    public class MatchParentAttribute : Attribute

    {

        public readonly string ParentPropertyName;

        public MatchParentAttribute(string parentPropertyName)

        {

            ParentPropertyName = parentPropertyName;

        }

    }
}
