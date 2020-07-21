using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Dynamic;
using System.Threading.Tasks;

namespace DirectSQL
{
    /// <summary>
    /// Object to get result of SQL
    /// </summary>
    /// <remarks>This stands for cursor in RDB</remarks>
    /// <typeparam name="R">Type of DataReader</typeparam>
    /// <typeparam name="CMD">Type of DbCommand</typeparam>
    /// <typeparam name="T">Type of Transaction</typeparam>
    /// <typeparam name="C">Type of Connection</typeparam>
    /// <typeparam name="P">Type of DataParameter</typeparam>
    public class SqlResult<R,CMD,T,C,P>:IDisposable 
        where R : IDataReader 
        where CMD : IDbCommand 
        where T : IDbTransaction 
        where C : IDbConnection 
        where P : IDataParameter, 
        new()
    {
        private R _reader;
        private CMD _command;

        private ImmutableArray<String> _resultFields = emptyFields;
        private readonly static ImmutableArray<string> emptyFields = new ImmutableArray<string>();

        private ExpandoObject _resultValues;
        private (String name, Object value)[] _resultTuples;

        /// <summary>
        /// variable not to execute not needed initialization
        /// </summary>
        private bool _allowInitialize;

        /// <summary>
        /// Reader in ADO.NET
        /// </summary>
        public IDataReader Reader
        {
            get
            {
                return _reader;
            }
        }

        /// <summary>
        /// Command in ADO.NET
        /// </summary>
        public IDbCommand Command
        {
            get
            {
                return _command;
            }
        }

        /// <summary>
        /// Sql
        /// </summary>
        /// <remarks>CommandText of command</remarks>
        public String Sql
        {
            get
            {
                return _command.CommandText;
            }
        }

        /// <summary>
        /// Fields in result
        /// </summary>
        public ImmutableArray<String> ResultFields
        {
            get
            {
                InitResultFields();
                return _resultFields;
            }
        }

        /// <summary>
        /// Result values as dynamic object
        /// </summary>
        /// <remarks>
        /// Each column in result has a field in dynamic object.
        /// </remarks>
        public dynamic ResultValues
        {
            get
            {
                InitResultValues();
                return _resultValues;
            }
        }

        /// <summary>
        /// Result object of type T
        /// </summary>
        /// <typeparam name="TP">Type of result object</typeparam>
        /// <param name="convert">convert from dynamic to T</param>
        /// <returns>result object</returns>
        /// <remarks>dynamic object is same as ResultValues</remarks>
        public TP ResultObject<TP>(Func<dynamic,TP> convert){
            return convert(ResultValues);
        }

        /// <summary>
        /// Return enumerable of SqlResult
        /// </summary>
        /// <typeparam name="TP">Type of object to be enumerated</typeparam>
        /// <param name="convert">Convert from dynamic to T</param>
        /// <returns>Object which enumerate result of SqlResult</returns>
        public IEnumerable<TP> AsEnumerable<TP>(Func<dynamic,TP> convert)
        {
            return new Enumerable<TP>(this, convert);
        }

        /// <summary>
        /// Return enumerable of SqlResult as dynamic
        /// </summary>
        /// <returns>Object which enumerate result of SqlResult</returns>
        public IEnumerable<dynamic> AsEnumerable()
        {
            return new Enumerable<dynamic>(this, (obj => obj ));
        }        

        /// <summary>
        /// Result values as an array of tuples
        /// </summary>
        /// <remarks>
        /// Each tuple has name and value.
        /// Name is name of column and 
        /// value is value of column
        /// in result row
        /// </remarks>
        public (String name,Object value)[] ResultTuples
        {
            get
            {
                InitResultTuples();
                return _resultTuples;
            }
        }

        private SqlResult ( 
            String sql, 
            P[] parameters, 
            C connection, 
            IDbTransaction transaction)
        {
            _command = (CMD) connection.CreateCommand();

            _command.CommandText = sql;

            foreach(var param in parameters){
                _command.Parameters.Add(param);
            }

            if(transaction != 
                   DefaultTransaction.defaultTransaction) {
                _command.Transaction = (T)transaction;
            }

            _allowInitialize = true;
        }

        internal SqlResult ( 
            String sql, 
            P[] parameters, 
            C connection, 
            T transaction) : this( sql, parameters, connection, (IDbTransaction) transaction)
        {
        }

        internal SqlResult ( 
            String sql, 
            P[] parameters, 
            C connection) : 
            this( 
                sql, 
                parameters, 
                connection, 
                DefaultTransaction.defaultTransaction)
        {
        }        

        /// <summary>
        /// Move cursor to next
        /// </summary>
        /// <returns>New row has values or not</returns>
        public bool Next()
        {
            _resultValues = null;
            _resultTuples = null;

            _allowInitialize = true;

            return _reader.Read();
        }

        internal void Init()
        {
            if (_allowInitialize)
            {
                if (_reader != null)
                    _reader.Close();

                _reader = (R) _command.ExecuteReader();
                _allowInitialize = false;

                _resultValues = null;
                _resultTuples = null;
                _resultFields = emptyFields;
            }
        }

        private void InitResultFields()
        {
            if (_resultFields != emptyFields)
                return; //Already initialized. No need to init again.

            List<string> list = new List<string>();
            for(int i = 0; i < _reader.FieldCount; i ++)
            {
                list.Add(_reader.GetName(i));
            }

            _resultFields = ImmutableArray.ToImmutableArray<String>(list);
        }

        private void InitResultValues()
        {
            if (_resultValues != null)
                return;
            _resultValues = CreateResultValue(_reader, ResultFields);
        }

        private static ExpandoObject CreateResultValue(
            R reader, 
            ImmutableArray<String> fields)
        {
            var values = new ExpandoObject();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                values.TryAdd(fields[i], reader.GetValue(i));
            }

            return values;
        }

        private void InitResultTuples()
        {
            if (_resultTuples != null)
                return;
            _resultTuples = CreateResultTuples(_reader, ResultFields);
        }

        private static (string, object)[] CreateResultTuples(
            R reader, 
            ImmutableArray<string> resultFields)
        {
            var array = new (String, object)[resultFields.Length];
            for(int i = 0; i < resultFields.Length; i ++)
            {
                array[i] = (resultFields[i], reader.GetValue(i));
            }
            return array;
        }

        internal void Close()
        {
            if (_reader != null)
                _reader.Close();

            if ( _command != null )
                _command.Dispose();
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        public static dynamic[] LoadSqlResult(
            String sql,
            P[] parameters,
            C connection, 
            T transaction)
        {
            var list = new List<dynamic>();

            Database<C, T, CMD, R, P>.Query(
                sql,
                parameters,
                connection,
                transaction,
                (result) => {
                    while (result.Next())
                    {
                        list.Add(result.ResultValues);
                    }
                });

            return list.ToArray();
        }

        public static dynamic[] LoadSqlResult(
            String sql,
            (String name, object value)[] parameters,
            C connection, 
            T transaction)
        {
            return LoadSqlResult(
                sql,
                Database<C,T,CMD,R,P>.ConvertToDbParameter(parameters),
                connection,
                transaction
            );
        }

        public static dynamic[] LoadSqlResult(
            String sql,
            C connection,
            T transaction)
        {
            return LoadSqlResult(
                sql, 
                new (String, object)[0], 
                connection, 
                transaction
            );
        }
        
        public static async Task<dynamic[]> LoadSqlResultAsync(
            String sql,
            C connection,
            T transaction)
        {
            return await LoadSqlResultAsync(
                sql, 
                new (String, object)[0], 
                connection, 
                transaction);
        }

        public static async Task<dynamic[]> LoadSqlResultAsync(
            String sql,
            (String name, object value)[] parameters,
            C connection,
            T transaction)
        {
            return await LoadSqlResultAsync(
                sql,
                Database<C,T,CMD,R,P>.ConvertToDbParameter(parameters),
                connection,
                transaction
            );
        }

         public static async Task<dynamic[]> LoadSqlResultAsync(
            String sql,
            P[] parameters,
            C connection,
            T transaction)
        {
            Task<dynamic[]> task = Task.Run<dynamic[]>(() =>
            {
                var list = new List<dynamic>();

                Database<C, T, CMD, R, P>.Query(
                    sql,
                    parameters,
                    connection,
                    transaction,
                    (result) =>
                    {
                        while (result.Next())
                        {
                            list.Add(result.ResultValues);
                        }
                    });

                return list.ToArray();
            });

            return await task;
        }

        private class Enumerable<TP> : IEnumerable<TP> 
        {
            private SqlResult<R,CMD,T,C,P> _sqlResult;
            private Func<dynamic, TP> _convert;

            internal Enumerable(
                SqlResult<R,CMD,T,C, P> sqlResult,
                Func<dynamic, TP> converter)
            {                
                _sqlResult = sqlResult;
                _convert = converter;
            }

            public IEnumerator<TP> GetEnumerator()
            {
                _sqlResult.Init();
                return new Enumerator<TP>(_sqlResult, _convert);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class Enumerator<TP> : IEnumerator<TP>, IEnumerator 
        {
            private SqlResult<R,CMD,T,C,P> _sqlResult;
            private Func<dynamic, TP> _convert;

            internal Enumerator(
                SqlResult<R,CMD,T,C,P> sqlResult, 
                Func<dynamic, TP> converter)
            {
                _sqlResult = sqlResult;
                _convert = converter;
            }

            public TP Current => _sqlResult.ResultObject<TP>(_convert);

            object IEnumerator.Current => _sqlResult.ResultObject<TP>(_convert);

            public void Dispose()
            {
                _sqlResult = null;
            }

            public bool MoveNext()
            {
                return _sqlResult.Next();
            }

            public void Reset()
            {
                _sqlResult.Init();
            }
        }
    }
}
