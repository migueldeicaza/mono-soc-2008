#region MIT license
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
using System.Diagnostics;
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Expressions
#else
namespace DbLinq.Data.Linq.Sugar.Expressions
#endif
{
    /// <summary>
    /// A table is a default table, or a joined table
    /// Different joins specify different tables
    /// </summary>
    [DebuggerDisplay("TableExpression {Name} (as {Alias})")]
    internal class TableExpression : MutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.Table;

        // Table idenfitication
        public string Name { get; private set; }

        // Join: if this table is related to another, the following informations are filled
        public Expression JoinExpression { get; private set; } // full information is here, ReferencedTable and Join could be (in theory) extracted from this
        public TableJoinType JoinType { get; private set; }
        public string JoinID { get; private set; }
        public TableExpression JoinedTable { get; private set; }

        public string Alias { get; set; }

        public void Join(TableJoinType joinType, TableExpression joinedTable, Expression joinExpression)
        {
            JoinExpression = joinExpression;
            JoinType = joinType;
            JoinedTable = joinedTable;
        }

        public void Join(TableJoinType joinType, TableExpression joinedTable, Expression joinExpression, string joinID)
        {
            Join(joinType, joinedTable, joinExpression);
            JoinID = joinID;
        }
        /// <summary>
        /// Ctor for associated table
        /// </summary>
        /// <param name="type">.NET type</param>
        /// <param name="name">Table base name</param>
        /// <param name="joinID"></param>
        public TableExpression(Type type, string name, string joinID)
            : base(ExpressionType, type)
        {
            Name = name;
            JoinID = joinID;
        }

        /// <summary>
        /// Ctor for default table
        /// </summary>
        /// <param name="type">.NET type</param>
        /// <param name="name">Table base name</param>
        public TableExpression(Type type, string name)
            : this(type, name, null)
        {
        }

        protected TableExpression(ExpressionType expressionType, TableExpression tableExpression)
            : base(expressionType, tableExpression.Type)
        {
            Name = tableExpression.Name;
        }

        public bool IsEqualTo(TableExpression expression)
        {
            return Name == expression.Name && JoinID == expression.JoinID;
        }
    }
}