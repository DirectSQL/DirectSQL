using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Dynamic;

namespace DirectSQL
{

    /// <summary>
    /// Object to get result of SQL
    /// </summary>
    /// <remarks>This stands for cursor in RDB</remarks>
    public class SqlResult:IDisposable
    {
        private IDataReader _reader;
        private IDbCommand _command;

        private ImmutableArray<String> _resultFields;

        private ExpandoObject _resultValues;
        private (String name, Object value)[] _resultTuples;

        /// <summary>
        /// variable not to execute not needed initialization
        /// </summary>
        private bool _allowInitlialize;

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
        /// <typeparam name="T">Type of result object</typeparam>
        /// <param name="convert">convert from dynamic to T</param>
        /// <returns>result object</returns>
        /// <remarks>dynamic object is same as ResultValues</remarks>
        public T ResultObject<T>(Func<dynamic,T> convert){
            return convert(ResultValues);
        }


        /// <summary>
        /// Return enumerable of SqlResult
        /// </summary>
        /// <typeparam name="T">Type of object to be enumerated</typeparam>
        /// <param name="convert">Convert from dynamic to T</param>
        /// <returns>Object which enumerate result of SqlResult</returns>
        public IEnumerable<T> AsEnumerable<T>(Func<dynamic,T> convert)
        {
            return new Enumerable<T>(this, convert);
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


        internal SqlResult ( 
            String sql, 
            (String name,object value)[] parameters, 
            IDbConnection connection, 
            IDbTransaction transaction)
        {
            _command = connection.CreateCommand();

            _command.CommandText = sql;
            Database.SetParameters(_command, parameters);

            _command.Transaction = transaction;

            _allowInitlialize = true;

        }


        /// <summary>
        /// Move cursor to next
        /// </summary>
        /// <returns>New row has values or not</returns>
        public bool Next()
        {
            _resultValues = null;
            _resultTuples = null;

            _allowInitlialize = true;

            return _reader.Read();

        }


        internal void Init()
        {
            if (_allowInitlialize)
            {
                if (_reader != null)
                    _reader.Close();

                _reader = _command.ExecuteReader();
                _allowInitlialize = false;
            }
        }

        private void InitResultFields()
        {
            if (_resultFields != null)
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

        private static ExpandoObject CreateResultValue(IDataReader reader, ImmutableArray<String> fields)
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

        private static (string, object)[] CreateResultTuples(IDataReader reader, ImmutableArray<string> resultFields)
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


        private class Enumerable<T> : IEnumerable<T>
        {

            private SqlResult _sqlResult;
            private Func<dynamic, T> _convert;

            internal Enumerable(SqlResult sqlResult,Func<dynamic, T> converter){
                _sqlResult = sqlResult;
                _convert = converter;
            }

            public IEnumerator<T> GetEnumerator()
            {
                _sqlResult.Init();
                return new Enumerator<T>(_sqlResult, _convert);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

        }

        private class Enumerator<T> : IEnumerator<T>, IEnumerator
        {

            private SqlResult _sqlResult;
            private Func<dynamic, T> _convert;

            internal Enumerator(SqlResult sqlResult, Func<dynamic, T> converter){
                _sqlResult = sqlResult;
                _convert = converter;
            }

            public T Current => _sqlResult.ResultObject<T>(_convert);

            object IEnumerator.Current => _sqlResult.ResultObject<T>(_convert);

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
