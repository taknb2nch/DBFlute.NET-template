
using System;
using System.Data;
using Seasar.Extension.ADO;
using Seasar.Extension.Tx;
using Seasar.Extension.Tx.Impl;
using Seasar.Quill.Database.DataSource.Impl;
using Seasar.Quill.Database.Tx.Impl;

namespace Aaa.Bbb.Ccc.DBFlute.AllCommon.S2Dao {

    public class TxSetting : AbstractTransactionSetting {
        protected override void SetupTransaction(IDataSource dataSource) {
            _transactionContext = new TransactionContext();
            TransactionContext txContext = (TransactionContext)_transactionContext;
            txContext.DataSouce = dataSource;
            txContext.IsolationLevel = this.IsolationLevel;

            Type dataSourceType = dataSource.GetType();
            if (typeof(SelectableDataSourceProxyWithDictionary).IsAssignableFrom(dataSourceType)) {
                SelectableDataSourceProxyWithDictionary dataSourceProxyWithDictionary = (SelectableDataSourceProxyWithDictionary)dataSource;
                if (!string.IsNullOrEmpty(DataSourceName)) {
                    IDataSource usingDataSource = dataSourceProxyWithDictionary.GetDataSource(DataSourceName);
                    if (usingDataSource is TxDataSource) {
                        ((TxDataSource)usingDataSource).Context = txContext;
                    }
                } else { // when only one dataSource
                    dataSourceProxyWithDictionary.SetTransactionContext(txContext);
                }
            } else if (typeof(TxDataSource).IsAssignableFrom(dataSourceType)) {
                ((TxDataSource)dataSource).Context = txContext;
            }

            ITransactionHandler handler = CreateTransactionHandler(txContext);
            _transactionInterceptor = CreateTransactionInterceptor(handler);
            ((TransactionInterceptor)_transactionInterceptor).TransactionStateHandler = txContext;
        }

        protected virtual IsolationLevel IsolationLevel {
            get { return IsolationLevel.ReadCommitted; }
        }

        protected virtual ITransactionHandler CreateTransactionHandler(TransactionContext txContext) {
            LocalRequiredTxHandler handler = new LocalRequiredTxHandler();
            handler.Context = txContext;
            return handler;
        }

        protected virtual TransactionInterceptor CreateTransactionInterceptor(ITransactionHandler handler) {
            return new TransactionInterceptor(handler);
        }
    }
}
