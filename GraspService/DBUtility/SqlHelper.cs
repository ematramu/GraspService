using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace GraspService.DBUtility
{
    public abstract class SqlHelper
    {

        #region "Private Members"
        private static Hashtable connectionStrings = null;

        public static readonly int CMD_TIMEOUT = Convert.ToInt32((ConfigurationManager.AppSettings["QUERY_TIMEOUT"]));
        // Hashtable to store cached parameters
        #endregion
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        #region "Costructor"
        static SqlHelper()
        {
            connectionStrings = new Hashtable();

            foreach (ConnectionStringSettings a in ConfigurationManager.ConnectionStrings)
            {
                connectionStrings.Add(a.Name, a.ConnectionString);
            }
        }
        #endregion

        #region "Public Shared Methods"
        /// <summary>
        /// Restituisce la connection string
        /// </summary>
        /// <param name="Nome">Nome della connessione nella sezione .config </param>        
        /// <returns>Stringa di connessione</returns>
        /// <remarks></remarks>
        public static string ConnectionString(string Nome)
        {
            return connectionStrings[Nome].ToString();
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="cmdType ">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText ">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        ///  Execute a SqlCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection ">an existing database connection</param>
        /// <param name="cmdType ">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText ">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>        
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;


            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) using an existing SQL Transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">an existing sql transaction</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;


            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);

            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="cmdType ">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText ">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>        
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;


            SqlConnection conn = new SqlConnection(connectionString);

            //// we use a try/catch here because if the method throws an exception we want to 
            //// close the connection throw code, because no datareader will exist, hence the 
            //// commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);

                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw ex;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">transazione</param>
        /// <param name="cmdType ">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText ">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>        
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;


            SqlConnection conn = trans.Connection;

            //// we use a try/catch here because if the method throws an exception we want to 
            //// close the connection throw code, because no datareader will exist, hence the 
            //// commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, conn, trans, cmdType, cmdText, commandParameters);

                SqlDataReader rdr = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw ex;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">a valid Transaction</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;


            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();

            return val;
        }


        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection ">an existing database connection</param>
        /// <param name="cmdType ">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText ">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandTimeout = CMD_TIMEOUT;

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }


        /// <summary>
        /// Retrieve cached parameters
        /// </summary>
        /// <param name="cacheKey">key used to lookup parameters</param>
        /// <returns>Cached SqlParamters array</returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];

            if ((cachedParms == null))
            {
                return null;
            }

            SqlParameter[] clonedParms = null;
            clonedParms = new SqlParameter[cachedParms.Length];

            int j = cachedParms.Length - 1;
            for (int i = 0; i <= j; i++)
            {
                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();
            }

            return clonedParms;
        }

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="cacheKey">Key to the parameter cache</param>
        /// <param name="cmdParms">an array of SqlParamters to be cached</param>
        public static void CacheParameters(string cacheKey, SqlParameter[] cmdParms)
        {
            parmCache[cacheKey] = cmdParms;
        }
        #endregion

        #region "Private Shared Methods"
        /// <summary>
        /// Prepare a command for execution
        /// </summary>
        /// <param name="cmd">SqlCommand object</param>
        /// <param name="conn">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            cmd.CommandTimeout = CMD_TIMEOUT;

            if ((conn.State != ConnectionState.Open))
            {
                conn.Open();
            }

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (((trans != null)))
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = cmdType;

            if (((cmdParms != null)))
            {
                SqlParameter parm = null;
                foreach (SqlParameter parm_loopVariable in cmdParms)
                {
                    parm = parm_loopVariable;
                    cmd.Parameters.Add(parm);
                }
            }
        }

        #endregion

    }
}