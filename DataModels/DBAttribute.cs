using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoScanFQCTest.DataModels
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DBAttribute : Attribute
    {
        public string DbName;
        public bool IsDbField;
        public DBAttribute(string dbName, bool isDbField = true)
        {
            DbName = dbName;
            IsDbField = isDbField;
        }
    }
}
