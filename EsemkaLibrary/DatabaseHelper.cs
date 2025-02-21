using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EsemkaLibrary
{
    internal class DatabaseHelper
    {
        private string connString = "Data Source=DESKTOP-RNFEO80\\SQLEXPRESS;Initial Catalog=EsemkaLibrary;Integrated Security=True;";

        private SqlConnection getConnection()
        {
            return new SqlConnection(connString);
        }

        public int executeNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = getConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Terjadi Kesalahan Database : " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Terjadi Kesalahan : " + ex.Message, ex);
            }
        }

        public object executeScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = getConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Terjadi Kesalahan Database : " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Terjadi Kesalahan : " + ex.Message, ex);
            }
        }

        public DataTable getData(string query, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection conn = getConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Terjadi Kesalahan Database : " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Terjadi Kesalahan : " + ex.Message, ex);
            }
            return dataTable;
        }

        public List<Dictionary<string, object>> loadData(string query, SqlParameter[] parameters = null)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            try
            {
                using (SqlConnection conn = getConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object columnValue = reader.GetValue(i);
                                    row[columnName] = columnValue;
                                }
                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Terjadi Kesalahan Database : " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Terjadi Kesalahan : " + ex.Message, ex);
            }
            return result;
        }
    }
}