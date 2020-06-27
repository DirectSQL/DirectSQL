using System;
using System.Data;

namespace DirectSQL
{
        /// <summary>
        /// Marker object indicates default transaction is used.
        /// </summary>
        internal class DefaultTransaction : IDbTransaction
        {
            /// <summary>
            /// Instance of DefaultTransaction
            /// </summary>
            internal static readonly DefaultTransaction defaultTransaction = new DefaultTransaction();

            private DefaultTransaction()
            {
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Commit()
            {
                throw new NotImplementedException();
            }

            public void Rollback()
            {
                throw new NotImplementedException();
            }

            public IDbConnection Connection { get; }
            public IsolationLevel IsolationLevel { get; }
        }
}