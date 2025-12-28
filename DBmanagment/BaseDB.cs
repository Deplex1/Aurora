using Mysqlx.Cursor;
using System.Collections.Generic;
using System.Data.Common;

//Version 3.4
namespace DBL
{
    public abstract class BaseDB<T> : DB
    {
        protected abstract string GetTableName();
        protected abstract string GetPrimaryKeyName();
        protected abstract Task<T> CreateModelAsync(object[] row);
        protected async virtual Task<List<T>> CreateListModelAsync(List<object[]> rows)
        {
            List<T> listDTOs = new List<T>();
            foreach (object[] row in rows)
            {
                T dto = (T)await CreateModelAsync(row);
                if (dto != null)
                {
                    listDTOs.Add(dto);
                }
            }
            return listDTOs;
        }

        /// <summary>
        /// A generic operation to retrieve ALL data from the database.
        /// </summary>
        /// <returns>List of Objects</returns>
        protected async Task<List<T>> SelectAllAsync()
        {
            return await SelectAllAsync("", new Dictionary<string, object>());
        }

        /// <summary>
        /// A generic operation to retrieve data from the database.
        /// </summary>
        /// <param name="parameters">Dictionary (Key & Value)</param>
        /// <returns>List of Objects</returns>
        protected async Task<List<T>> SelectAllAsync(Dictionary<string, object> parameters)
        {
            return await SelectAllAsync("", parameters);
        }

        /// <summary>
        /// A generic operation to retrieve data from the database.
        /// </summary>
        /// <param name="query">SQL string</param>
        /// <returns>List of Objects</returns>
        protected async Task<List<T>> SelectAllAsync(string query)
        {
            return await SelectAllAsync(query, new Dictionary<string, object>());
        }

        /// <summary>
        /// A generic operation to retrieve data from the database.
        /// </summary>
        /// <param name="query">SQL string</param>
        /// <param name="parameters">Dictionary (Key & Value)</param>
        /// <returns>List of Objects</returns>
        protected async Task<List<T>> SelectAllAsync(string query, Dictionary<string, object> parameters)
        {
            List<object[]> list = await StingListSelectAllAsync(query, parameters);
            return await CreateListModelAsync(list);
        }

        /// <summary>
        /// Insert new records in a table using INSERT Statement.
        /// </summary>
        /// <param name="keyAndValue">Dictionary (Key & Value)</param>
        /// <returns>The number of rows affected.</returns>
        protected async Task<int> InsertAsync(Dictionary<string, object> keyAndValue)
        {
            string sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);
            return await ExecNonQueryAsync(sqlCommand);
        }

        /// <summary>
        /// Insert new records in a table using INSERT Statement.
        /// </summary>
        /// <param name="keyAndValue">Dictionary (Key & Value)</param>
        /// <returns>An object that includes the ID attribute from the database.</returns>
        protected async Task<T> InsertGetObjAsync(Dictionary<string, object> keyAndValue)
        {
            string sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);
            sqlCommand += $" SELECT LAST_INSERT_ID();";
            object res = await ExecScalarAsync(sqlCommand);
            if (res != null)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("id", res.ToString());
                string sql = @$"SELECT * FROM {GetTableName()} WHERE ({GetPrimaryKeyName()} = @id)";
                List<T> list = (List<T>)await SelectAllAsync(sql, p);
                if (list.Count == 1)
                    return list[0];
                else
                    return default;
            }
            else
                return default;
        }

        /// <summary>
        /// Update records in a table using SQL UPDATE Statement.
        /// </summary>
        /// <param name="FildValue">Dictionary (Key & Value)</param>
        /// <param name="parameters">Dictionary (Key & Value)</param>
        /// <returns>The number of rows affected.</returns>
        protected async Task<int> UpdateAsync(Dictionary<string, object> FildValue, Dictionary<string, object> parameters)
        {
            string where = PrepareWhereQueryWithParameters(parameters);

            string InKeyValue = PrepareUpdateQueryWithParameters(FildValue);
            if (string.IsNullOrEmpty(InKeyValue))
                return 0;

            string sqlCommand = $"UPDATE {GetTableName()} SET {InKeyValue}  {where}";
            return await ExecNonQueryAsync(sqlCommand);
        }

        /// <summary>
        /// Delete records in a table using SQL DELETE Statement.
        /// </summary>
        /// <param name="parameters">Dictionary (Key & Value)</param>
        /// <returns>The number of rows affected.</returns>
        protected async Task<int> DeleteAsync(Dictionary<string, object> parameters)
        {
            string where = PrepareWhereQueryWithParameters(parameters);

            string sqlCommand = $"DELETE FROM {GetTableName()} {where}";
            return await ExecNonQueryAsync(sqlCommand);
        }

        /// <summary>
        /// Add one parameters to SQL statement.
        /// </summary>
        /// <param name="name">Parameter name example:@id</param>
        /// <param name="value">Parameter value</param>
        private void AddParameterToCommand(string name, object value)
        {
            DbParameter p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        /// <summary>
        /// Prepare command and Connection before executing SQL command
        /// </summary>
        /// <example>string query = "DELETE FROM Customers WHERE CustomerID = 17"</example>
        /// <param name="query">SQL query string</param>
        private async Task PreQueryAsync(string query)
        {
            cmd.CommandText = query;
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();
            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                cmd.Connection = conn;
        }

        /// <summary>
        /// Make cleanup after sql command was executed
        /// </summary>
        private async Task PostQueryAsync()
        {
            if (reader != null && !reader.IsClosed)
                await reader.CloseAsync();

            cmd.Parameters.Clear();
            if (conn.State == System.Data.ConnectionState.Open)
                await conn.CloseAsync();
        }

        /// <summary>
        /// Prepare a WHERE SQL Query, from given paremeters dictionary
        /// </summary>
        /// <param name="parameters">Dictionary (Key & Value)</param>
        /// <example>Where p1=v1 AND p2=v2</example>
        /// <returns>String of SQL Where closure</returns>
        private string PrepareWhereQueryWithParameters(Dictionary<string, object> parameters)
        {
            string where = "WHERE ";
            string AND = "AND";
            if (parameters != null && parameters.Count > 0)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    string prm = $"@W{param.Key}";
                    where += $"{param.Key}={prm} {AND} ";
                    AddParameterToCommand(prm, param.Value);
                }
                where = where.Remove(where.Length - AND.Length - 2);//remove last ' AND '
            }
            else
                where = "";
            return where;
        }

        /// <summary>
        /// Prepare Update Query With Parameters
        /// </summary>
        /// <param name="fields">Dictionary (Key & Value)</param>
        /// <returns>String of SQL</returns>
        private string PrepareUpdateQueryWithParameters(Dictionary<string, object> fields)
        {
            string InValue = "";
            if (fields != null && fields.Count > 0)
            {
                foreach (KeyValuePair<string, object> param in fields)
                {
                    string prm = $"@{param.Key}";
                    InValue += $"{param.Key}={prm},";
                    AddParameterToCommand(prm, param.Value);
                }
                InValue = InValue.Remove(InValue.Length - 1); //remove last ,
            }
            return InValue;
        }

        /// <summary>
        /// Prepare Insert Query With Parameters
        /// </summary>
        /// <param name="fields">Dictionary (Key & Value)</param>
        /// <returns>String of SQL</returns>
        private string PrepareInsertQueryWithParameters(Dictionary<string, object> fields)
        {
            if (fields == null || fields.Count == 0)
                return "";

            string InKey = "(" + string.Join(",", fields.Keys) + ")";
            string InValue = "VALUES(";
            for (int i = 0; i < fields.Values.Count; i++)
            {
                string pn = "@" + i;
                InValue += pn + ',';
                AddParameterToCommand(pn, fields.Values.ElementAt(i));
            }
            InValue = InValue.Remove(InValue.Length - 1);//remove last ,
            InValue += ")";

            string sqlCommand = $"INSERT INTO {GetTableName()}  {InKey} {InValue};";
            return sqlCommand;
        }

        /// <summary>
        /// Executes the command against its connection object, returning the number of rows affected. 
        /// </summary>
        /// <param name="query">SQL string</param>
        /// <example>DELETE FROM Customers WHERE CustomerID = 17</example>
        /// <returns>The number of rows affected.</returns>
        private async Task<int> ExecNonQueryAsync(string query)
        {
            if (String.IsNullOrEmpty(query))
                return 0;
            await PreQueryAsync(query);
            int rowsEffected = 0;
            try
            {
                rowsEffected = await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
            }
            finally
            {
                await PostQueryAsync();
            }
            return rowsEffected;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result
        /// </summary>
        /// <param name="query">SQL string</param>
        /// <returns>The first column of the first row in the result set, or a null.</returns>
        private async Task<object> ExecScalarAsync(string query)
        {
            if (String.IsNullOrEmpty(query))
                return null;
            await PreQueryAsync(query);
            object obj = null;
            try
            {
                obj = await cmd.ExecuteScalarAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
            }
            finally
            {
                await PostQueryAsync();
            }
            return obj;
        }

        /// <summary>
        /// Prepare the main WHERE SQL Query, from given paremeters dictionary
        /// </summary>
        /// <param name="query">SQL string</param>
        /// <param name="parameters">Dictionary (Key & Value)</param>
        /// <returns>List of Objects</returns>
        private async Task<List<object[]>> StingListSelectAllAsync(string query, Dictionary<string, object> parameters)
        {
            List<object[]> list = new List<object[]>();
            string sqlCommand = "";
            if (String.IsNullOrEmpty(query))
            {
                string where = PrepareWhereQueryWithParameters(parameters);
                sqlCommand = $"SELECT * FROM {GetTableName()} {where}";
            }
            else if (query.IndexOf("@") > 0)
            {
                sqlCommand = query;
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    AddParameterToCommand($"@{param.Key}", param.Value);
                }
            }
            else
            {
                string where = PrepareWhereQueryWithParameters(parameters);
                sqlCommand = $"{query} {where}";
            }
            await PreQueryAsync(sqlCommand);
            try
            {
                reader = await cmd.ExecuteReaderAsync();
                var readOnlyData = await reader.GetColumnSchemaAsync();
                int size = readOnlyData.Count;
                object[] row;
                while (reader.Read())
                {
                    row = new object[size];
                    reader.GetValues(row);
                    list.Add(row);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                list.Clear();
            }
            finally
            {
                await PostQueryAsync();
            }
            return list;
        }

        /// <summary>
        /// Execute non-query SQL commands (INSERT, UPDATE, DELETE)
        /// </summary>
        protected async Task ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            if (String.IsNullOrEmpty(query))
                return;

            await PreQueryAsync(query);
            try
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = "@" + param.Key;
                        parameter.Value = param.Value ?? DBNull.Value;
                        cmd.Parameters.Add(parameter);
                    }
                }
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
            }
            finally
            {
                await PostQueryAsync();
            }
        }

        /// <summary>
        /// Select a list of scalar values from database
        /// </summary>
        protected async Task<List<T>> SelectScalarListAsync<T>(string query, Dictionary<string, object> parameters = null)
        {
            List<T> list = new List<T>();
            if (String.IsNullOrEmpty(query))
                return list;

            await PreQueryAsync(query);
            try
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = "@" + param.Key;
                        parameter.Value = param.Value ?? DBNull.Value;
                        cmd.Parameters.Add(parameter);
                    }
                }

                using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            list.Add((T)Convert.ChangeType(reader[0], typeof(T)));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                list.Clear();
            }
            finally
            {
                await PostQueryAsync();
            }
            return list;
        }
    }
}