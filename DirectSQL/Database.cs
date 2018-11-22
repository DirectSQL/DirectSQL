using System;
using System.Data;
using System.Threading.Tasks;

namespace DirectSQL
{
    public abstract class Database
    {

        public delegate void SqlExecution(IDbConnection connection, IDbTransaction transaction);
        public delegate Task AsyncSqlExecution(IDbConnection connection, IDbTransaction transaction);

        public delegate void ReadSqlResult(SqlResult result);

        public async Task ProcessAsync(AsyncSqlExecution execute)
        {

            using ( var connection = CreateConnection())
            {
                connection.Open();
                    
                using( var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await execute(connection, transaction);

                        transaction.Commit();
                    }
                    catch(Exception exception)
                    {
                        transaction.Rollback();
                        throw new DatabaseException(MessageResource.msg_error_asyncSqlExecutionError, exception);
                    }
                }
            }

        }

        public void Process(SqlExecution execute)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        execute(connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        transaction.Rollback();
                        throw new DatabaseException(MessageResource.msg_error_sqlExecutionError, exception);
                    }
                }
            }

        }

        protected abstract IDbConnection CreateConnection();
        public abstract IDbDataParameter CreateDbDataParameter(String name, Object value);

        public static int ExecuteNonQuery(
            string sql,
            IDbDataParameter[] parameters,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            using( var command = connection.CreateCommand())
            {
                command.Transaction = transaction;

                command.CommandText = sql;
                SetParameters(command, parameters);

                return command.ExecuteNonQuery();

            }
        }


        public static int ExecuteNonQuery(
            string sql,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            return ExecuteNonQuery(sql, new IDbDataParameter[0], connection, transaction);
        }


        public static void Query(
            string sql, 
            IDbDataParameter[] parameters,  
            IDbConnection connection, 
            IDbTransaction transaction,
            ReadSqlResult readResult)
        {
            using (var result = new SqlResult(sql,parameters,connection,transaction))
            {
                result.Init();
                readResult(result);
            }
        }


        public static void Query(
            string sql,
            IDbConnection connection,
            IDbTransaction transaction,
            ReadSqlResult readResult)
        {
            Query(sql, new IDbDataParameter[0], connection, transaction, readResult);
        }


        internal static void SetParameters(IDbCommand command, IDbDataParameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i ++)
            {
                command.Parameters.Add(parameters[i]);
            }
        }

    }
}
