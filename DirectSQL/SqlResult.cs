using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Dynamic;

namespace DirectSQL
{
    public class SqlResult:IDisposable
    {
        private IDataReader _reader;
        private IDbCommand _command;

        private ImmutableArray<String> _resultFields;

        private ExpandoObject _resultValues;

        public IDataReader Reader
        {
            get
            {
                return _reader;
            }
        }

        public IDbCommand Command
        {
            get
            {
                return _command;
            }
        }

        public ImmutableArray<String> ResultFields
        {
            get
            {
                InitResultFields();
                return _resultFields;
            }
        }


        public dynamic ResultValues
        {
            get
            {
                InitResultValues();
                return _resultValues;
            }
        }


        internal SqlResult ( 
            String sql, 
            (String,object)[] parameters, 
            IDbConnection connection, 
            IDbTransaction transaction)
        {
            _command = connection.CreateCommand();

            _command.CommandText = sql;
            Database.SetParameters(_command, parameters);

            _command.Transaction = transaction;

        }


        public bool Next()
        {
            _resultValues = null;
            return _reader.Read();
        }


        internal void Init()
        {
            _reader = _command.ExecuteReader();
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

        internal void Close()
        {
            if (_reader != null)
                _reader.Close();

            if ( _command != null )
                _command.Dispose();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
