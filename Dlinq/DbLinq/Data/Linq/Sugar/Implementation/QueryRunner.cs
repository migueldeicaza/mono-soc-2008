﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

using DbLinq.Data.Linq.Database;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class QueryRunner : IQueryRunner
    {
        protected class DbCommand : IDisposable
        {
            private IDisposable _connection;
            private IDatabaseTransaction _transaction;
            public readonly IDbCommand Command;

            public virtual void Dispose()
            {
                Command.Dispose();
                if (_transaction != null)
                    _transaction.Dispose();
                _connection.Dispose();
            }

            /// <summary>
            /// Commits the current transaction.
            /// throws NRE if _transaction is null. Behavior is intentional.
            /// </summary>
            public void Commit()
            {
                // TODO: do not commit if participating in a higher transaction
                _transaction.Commit();
            }

            public DbCommand(string commandText, bool createTransaction, DataContext dataContext)
            {
                // TODO: check if all this stuff is necessary
                // the OpenConnection() checks that the connection is already open
                // TODO: see if we can move this here (in theory the final DataContext shouldn't use)
                _connection = dataContext.DatabaseContext.OpenConnection();
                // the transaction is optional
                if (createTransaction)
                    _transaction = dataContext.DatabaseContext.Transaction();
                Command = dataContext.DatabaseContext.CreateCommand();
                Command.CommandText = commandText;
                if (createTransaction)
                    Command.Transaction = _transaction.Transaction;
            }
        }

        protected virtual DbCommand UseDbCommand(string commandText, bool createTransaction, DataContext dataContext)
        {
            return new DbCommand(commandText, createTransaction, dataContext);
        }

        /// <summary>
        /// Enumerates all records return by SQL request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Select<T>(SelectQuery selectQuery)
        {
            var rowObjectCreator = selectQuery.GetRowObjectCreator<T>();

            // handle the special case where the query is empty, meaning we don't need the DB
            if (string.IsNullOrEmpty(selectQuery.Sql))
            {
                yield return rowObjectCreator(null, null);
                yield break;
            }

            using (var dbCommand = UseDbCommand(selectQuery.Sql, false, selectQuery.DataContext))
            {
                foreach (var parameter in selectQuery.InputParameters)
                {
                    var dbParameter = dbCommand.Command.CreateParameter();
                    dbParameter.ParameterName = selectQuery.DataContext.Vendor.SqlProvider.GetParameterName(parameter.Alias);
                    dbParameter.Value = parameter.GetValue();
                    dbCommand.Command.Parameters.Add(dbParameter);
                }
                using (var reader = dbCommand.Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // someone told me one day this could happen (in SQLite)
                        if (reader.FieldCount == 0)
                            continue;

                        var row = rowObjectCreator(reader, selectQuery.DataContext._MappingContext);
                        // the conditions to register and watch an entity are:
                        // - not null (can this happen?)
                        // - registered in the model
                        if (row != null && selectQuery.DataContext.Mapping.GetTable(row.GetType()) != null)
                        {
                            row = (T)selectQuery.DataContext.Register(row, typeof(T));
                        }

                        yield return row;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a unique row (common reference)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="t"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        protected virtual object GetUniqueRow(object row, Type t, DataContext dataContext)
        {
            if (row != null && dataContext.Mapping.GetTable(row.GetType()) != null)
                row = dataContext.Register(row, t);
            return row;
        }

        /// <summary>
        /// Returns a unique row (common reference)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        protected virtual T GetUniqueRow<T>(object row, DataContext dataContext)
        {
            return (T)GetUniqueRow(row, typeof(T), dataContext);
        }

        public virtual S SelectScalar<S>(SelectQuery selectQuery)
        {
            switch (selectQuery.ExecuteMethodName)
            {
            case null: // some calls, like Count() generate SQL and the resulting projection method name is null (never initialized)
                return SelectSingle<S>(selectQuery, false); // Single() for safety, but First() should work
            case "First":
                return SelectFirst<S>(selectQuery, false);
            case "FirstOrDefault":
                return SelectFirst<S>(selectQuery, true);
            case "Single":
                return SelectSingle<S>(selectQuery, false);
            case "SingleOrDefault":
                return SelectSingle<S>(selectQuery, true);
            case "Last":
                return SelectLast<S>(selectQuery, false);
            }
            throw Error.BadArgument("S0077: Unhandled method '{0}'", selectQuery.ExecuteMethodName);
        }

        /// <summary>
        /// Returns first item in query.
        /// If no row is found then if default allowed returns default(S), throws exception otherwise
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S SelectFirst<S>(SelectQuery selectQuery, bool allowDefault)
        {
            foreach (var row in Select<S>(selectQuery))
                return row;
            if (!allowDefault)
                throw new InvalidOperationException();
            return default(S);
        }

        /// <summary>
        /// Returns single item in query
        /// If more than one item is found, throws an exception
        /// If no row is found then if default allowed returns default(S), throws exception otherwise
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S SelectSingle<S>(SelectQuery selectQuery, bool allowDefault)
        {
            S firstRow = default(S);
            int rowCount = 0;
            foreach (var row in Select<S>(selectQuery))
            {
                if (rowCount > 1)
                    throw new InvalidOperationException();
                firstRow = row;
                rowCount++;
            }
            if (!allowDefault && rowCount == 0)
                throw new InvalidOperationException();
            return firstRow;
        }

        /// <summary>
        /// Returns last item in query
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S SelectLast<S>(SelectQuery selectQuery, bool allowDefault)
        {
            S lastRow = default(S);
            int rowCount = 0;
            foreach (var row in Select<S>(selectQuery))
            {
                lastRow = row;
                rowCount++;
            }
            if (!allowDefault && rowCount == 0)
                throw new InvalidOperationException();
            return lastRow;
        }

        /// <summary>
        /// Runs an InsertQuery on a provided object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="insertQuery"></param>
        public void Insert(object target, UpsertQuery insertQuery)
        {
            Upsert(target, insertQuery);
        }

        private void Upsert(object target, UpsertQuery insertQuery)
        {
            var sqlProvider = insertQuery.DataContext.Vendor.SqlProvider;
            using (var dbCommand = UseDbCommand(insertQuery.Sql, true, insertQuery.DataContext))
            {
                foreach (var inputParameter in insertQuery.InputParameters)
                {
                    var dbParameter = dbCommand.Command.CreateParameter();
                    dbParameter.ParameterName = sqlProvider.GetParameterName(inputParameter.Alias);
                    dbParameter.SetValue(inputParameter.GetValue(target), inputParameter.ValueType);
                    dbCommand.Command.Parameters.Add(dbParameter);
                }
                if (insertQuery.DataContext.Vendor.SupportsOutputParameter)
                {
                    int outputStart = insertQuery.InputParameters.Count;
                    /*foreach (var outputParameter in insertQuery.OutputParameters)
                    {
                        var dbParameter = dbCommand.Command.CreateParameter();
                        dbParameter.ParameterName = sqlProvider.GetParameterName(outputParameter.Alias);
                        // Oracle is lost if output variables are uninitialized. Another winner story.
                        dbParameter.SetValue(null, outputParameter.ValueType);
                        dbParameter.Size = 100;
                        dbParameter.Direction = ParameterDirection.Output;
                        dbCommand.Command.Parameters.Add(dbParameter);
                    }*/
                    int rowsCount = dbCommand.Command.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(insertQuery.IdQuerySql))
                    {
                        IDbCommand cmd = dbCommand.Command.Connection.CreateCommand();
                        cmd.Transaction = dbCommand.Command.Transaction;
                        cmd.CommandText = insertQuery.IdQuerySql;
                        IDataReader rdr = cmd.ExecuteReader();
                        rdr.Read();
                        for (int outputParameterIndex = 0;
                             outputParameterIndex < insertQuery.OutputParameters.Count;
                             outputParameterIndex++)
                        {
                            var outputParameter = insertQuery.OutputParameters[outputParameterIndex];
                            var outputDbParameter = rdr.GetValue(outputParameterIndex);
                            SetOutputParameterValue(target, outputParameter, outputDbParameter);
                        }
                        rdr.Close();

                    }
                    /*
                    for (int outputParameterIndex = 0;
                         outputParameterIndex < insertQuery.OutputParameters.Count;
                         outputParameterIndex++)
                    {
                        var outputParameter = insertQuery.OutputParameters[outputParameterIndex];
                        var outputDbParameter =
                            (IDbDataParameter)dbCommand.Command.Parameters[outputParameterIndex + outputStart];
                        SetOutputParameterValue(target, outputParameter, outputDbParameter.Value);
                    }
                     */
                }
                else
                {
                    object result = dbCommand.Command.ExecuteScalar();
                    if (insertQuery.OutputParameters.Count > 1)
                        throw new ArgumentException();
                    if (insertQuery.OutputParameters.Count == 1)
                        SetOutputParameterValue(target, insertQuery.OutputParameters[0], result);
                }
                dbCommand.Commit();
            }
        }

        protected virtual void SetOutputParameterValue(object target, ObjectOutputParameterExpression outputParameter, object value)
        {
            if (value is DBNull)
                outputParameter.SetValue(target, null);
            else
                outputParameter.SetValue(target, TypeConvert.To(value, outputParameter.ValueType));
        }

        /// <summary>
        /// Performs an update
        /// </summary>
        /// <param name="target">Entity to be flushed</param>
        /// <param name="updateQuery">SQL update query</param>
        /// <param name="modifiedMembers">List of modified members, or null to update all members</param>
        public void Update(object target, UpsertQuery updateQuery, IList<MemberInfo> modifiedMembers)
        {
            Upsert(target, updateQuery);
        }

        /// <summary>
        /// Performs a delete
        /// </summary>
        /// <param name="target">Entity to be deleted</param>
        /// <param name="deleteQuery">SQL delete query</param>
        public void Delete(object target, DeleteQuery deleteQuery)
        {
            var sqlProvider = deleteQuery.DataContext.Vendor.SqlProvider;
            using (var dbCommand = UseDbCommand(deleteQuery.Sql, true, deleteQuery.DataContext))
            {
                foreach (var inputParameter in deleteQuery.InputParameters)
                {
                    var dbParameter = dbCommand.Command.CreateParameter();
                    dbParameter.ParameterName = sqlProvider.GetParameterName(inputParameter.Alias);
                    dbParameter.SetValue(inputParameter.GetValue(target), inputParameter.ValueType);
                    dbCommand.Command.Parameters.Add(dbParameter);
                }
                int rowsCount = dbCommand.Command.ExecuteNonQuery();
                dbCommand.Commit();
            }
        }

        /// <summary>
        /// Fills dbCommand parameters, given names and values
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="parameterNames"></param>
        /// <param name="parameterValues"></param>
        private void FeedParameters(IDbCommand dbCommand, IList<string> parameterNames, IList<object> parameterValues)
        {
            for (int parameterIndex = 0; parameterIndex < parameterNames.Count; parameterIndex++)
            {
                var dbParameter = dbCommand.CreateParameter();
                dbParameter.ParameterName = parameterNames[parameterIndex];
                dbParameter.SetValue(parameterValues[parameterIndex]);
                dbCommand.Parameters.Add(dbParameter);
            }
        }

        /// <summary>
        /// Runs a direct scalar command
        /// </summary>
        /// <param name="directQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Execute(DirectQuery directQuery, params object[] parameters)
        {
            using (var dbCommand = UseDbCommand(directQuery.Sql, false, directQuery.DataContext))
            {
                FeedParameters(dbCommand.Command, directQuery.Parameters, parameters);
                var result = dbCommand.Command.ExecuteScalar();
                if (result == null || result is DBNull)
                    return 0;
                var intResult = TypeConvert.ToNumber<int>(result);
                return intResult;
            }
        }

        // TODO: move method?
        protected virtual Delegate GetTableBuilder(Type elementType, IDataReader dataReader, DataContext dataContext)
        {
            var fields = new List<string>();
            for (int fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                fields.Add(dataReader.GetName(fieldIndex));
            return dataContext.QueryBuilder.GetTableReader(elementType, fields, new QueryContext(dataContext));
        }

        /// <summary>
        /// Runs a query with a direct statement
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="directQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable ExecuteSelect(Type tableType, DirectQuery directQuery, params object[] parameters)
        {
            var dataContext = directQuery.DataContext;
            using (var dbCommand = UseDbCommand(directQuery.Sql, false, directQuery.DataContext))
            {
                FeedParameters(dbCommand.Command, directQuery.Parameters, parameters);
                using (var dataReader = dbCommand.Command.ExecuteReader())
                {
                    // Did you know? "return EnumerateResult(tableType, dataReader, dataContext);" disposes resources first
                    // before the enumerator is used
                    foreach (var result in EnumerateResult(tableType, dataReader, dataContext))
                        yield return result;
                }
            }
        }

        /// <summary>
        /// Enumerates results from a request.
        /// The result shape can change dynamically
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dataReader"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public IEnumerable EnumerateResult(Type tableType, IDataReader dataReader, DataContext dataContext)
        {
            return EnumerateResult(tableType, true, dataReader, dataContext);
        }

        /// <summary>
        /// Enumerates results from a request.
        /// The result shape can change dynamically
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dynamicallyReadShape">Set True to change reader shape dynamically</param>
        /// <param name="dataReader"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        protected virtual IEnumerable EnumerateResult(Type tableType, bool dynamicallyReadShape, IDataReader dataReader, DataContext dataContext)
        {
            Delegate tableBuilder = null;
            while (dataReader.Read())
            {
                if (tableBuilder == null || dynamicallyReadShape)
                    tableBuilder = GetTableBuilder(tableType, dataReader, dataContext);
                var row = tableBuilder.DynamicInvoke(dataReader, dataContext._MappingContext);
                row = GetUniqueRow(row, tableType, dataContext);
                yield return row;
            }
        }
    }
}
