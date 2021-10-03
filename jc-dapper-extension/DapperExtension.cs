using Dapper;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace jc_dapper_extension
{
    public class DapperExtension
    {
        protected string TableName;
        protected string PrimaryKeyField;
        protected SqlConnection Connection;

        private void _CommonValidations(object Record)
        {
            if (Record == null)
                throw new ApplicationException("Record is required");
        }

        private bool _GetIgnoreAttributeProperty(PropertyInfo Property)
        {
            bool ignore_field = false;

            //Check if column needs to be ignore
            var dapper_tools_attribute = (Attributes.DapperExtensionAttribute)Attribute.GetCustomAttribute(element: Property, attributeType: typeof(Attributes.DapperExtensionAttribute));
            if (dapper_tools_attribute != null)
                ignore_field = dapper_tools_attribute.Ignore;

            return ignore_field;
        }

        internal async Task<long> InsertAsync(object Record)
        {
            _CommonValidations(Record: Record);

            //Get object properties
            var properties = Record.GetType().GetProperties();

            //Build sql statements
            var SQL = new StringBuilder();
            SQL.Append($@" INSERT INTO {TableName} (");

            int count = 1;
            foreach (var property in properties)
            {
                var ignore_field = _GetIgnoreAttributeProperty(Property: property);
                if (ignore_field == false)
                {
                    if (count == properties.Count())
                    {
                        SQL.Append($"{property.Name})");
                    }
                    else
                    {
                        SQL.Append($"{property.Name},");
                    }
                }

                count++;
            }

            SQL.Append(" VALUES (");

            count = 1;
            foreach (var property in properties)
            {
                var ignore_field = _GetIgnoreAttributeProperty(Property: property);
                if (ignore_field == false)
                {
                    if (count == properties.Count())
                    {
                        SQL.Append($"@{property.Name})");
                    }
                    else
                    {
                        SQL.Append($"@{property.Name},");
                    }
                }

                count++;
            }

            SQL.Append(" SELECT CAST(SCOPE_IDENTITY() as int)");

            var retuned_value = await Connection.QuerySingleAsync<long>(sql: SQL.ToString(), param: Record);

            return retuned_value;
        }

        internal async Task UpdateAsync(object Record)
        {
            _CommonValidations(Record: Record);

            //Get object properties
            var properties = Record.GetType().GetProperties();

            //Build sql statements
            var SQL = new StringBuilder();
            SQL.Append($"UPDATE {TableName} SET ");

            int count = 1;
            foreach (var property in properties)
            {
                var ignore_field = _GetIgnoreAttributeProperty(Property: property);
                if (ignore_field == false)
                {
                    if (count == properties.Count())
                    {
                        SQL.Append($"{property.Name} = @{property.Name}");
                    }
                    else
                    {
                        SQL.Append($"{property.Name} = @{property.Name},");
                    }
                }

                count++;
            }

            SQL.Append($" WHERE {PrimaryKeyField} = @{PrimaryKeyField} ");

            //Get primary key property
            var primary_key_property = properties.FirstOrDefault(t => t.Name == PrimaryKeyField);
            if (primary_key_property == null)
                throw new ApplicationException("Invalid primary key field");

            var primary_key_value = primary_key_property.GetValue(Record);

            long.TryParse(primary_key_value?.ToString(), out long ID);

            if (ID == 0)
                throw new ApplicationException("Invalid primary key field");

            await Connection.ExecuteAsync(sql: SQL.ToString(), param: Record);
        }

        internal async Task DeleteAsync(long ID)
        {
            await Connection.ExecuteAsync(sql: $" DELETE {TableName} WHERE {PrimaryKeyField} = @ID", param: new { ID });
        }
    }
}
