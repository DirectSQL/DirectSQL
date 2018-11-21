using System;
using System.Data;
using System.Threading.Tasks;

namespace DirectSQL
{
    public abstract class Database
    {

        public delegate void SqlExecution(IDbConnection connection, IDbTransaction transaction);
        public delegate Task AsyncSqlExecution(IDbConnection connection, IDbTransaction transaction);

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

    }
}
