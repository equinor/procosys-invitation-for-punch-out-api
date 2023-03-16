﻿using System;
using System.Reflection;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods
{
    public static class EntityBaseTestExtensions
    {
        public static void SetProtectedIdForTesting(this EntityBase entityBase, int id)
        {
            var objType = typeof(EntityBase);
            var property = objType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            property.SetValue(entityBase, id);
        }
    }
}
