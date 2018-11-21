using System;
using System.Data;
using System.Threading.Tasks;

namespace DirectSQL
{
    public abstract class Database
    {

        public delegate Task SqlExecution(IDbConnection connection, IDbTransaction transaction);

        public async Task ProcessAsync(SqlExecution execute)
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
            Task task = ProcessAsync(execute);
            task.Wait();

            if (task.Exception == null)
                return;

            throw new DatabaseException(MessageResource.msg_error_sqlExecutionError, task.Exception);

        }

        protected abstract IDbConnection CreateConnection();

    }
}
