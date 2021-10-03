using System;
using System.Collections.Generic;
using System.Text;

namespace jc_dapper_extension.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DapperExtensionAttribute : System.Attribute
    {
        public bool Ignore { get; set; }
    }
}
