using AutoScanFQCTest.DataModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.Utilities
{
    public class ListHelper
    {
        public static List<T> DataTableToList<T>(DataTable table) where T : new()
        {
            List<T> list = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                T item = new T();
                PropertyInfo[] properties = item.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    DBAttribute dbattr = property.GetCustomAttribute<DBAttribute>();
                    if (dbattr == null)
                    {
                        continue;
                    }
                    if (dbattr.IsDbField == false)
                    {
                        continue;
                    }
                    if (table.Columns.Contains(dbattr.DbName))
                    {
                        if (row[dbattr.DbName] != DBNull.Value)
                        {
                            property.SetValue(item, ConvertTo(row[dbattr.DbName], property.PropertyType), null);
                        }
                    }
                }
                list.Add(item);
            }
            return list;
        }

        public static object ConvertTo(object convertibleValue, Type type)
        {
            if (!type.IsGenericType)
            {
                return Convert.ChangeType(convertibleValue, type);
            }
            else
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(type));
                }
            }
            throw new InvalidCastException(string.Format("Invalid cast from type \"{0}\" to type \"{1}\".", convertibleValue.GetType().FullName, type.FullName));
        }
    }
}
