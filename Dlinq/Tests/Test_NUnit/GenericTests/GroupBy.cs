using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using nwind;
using DbLinq.Linq;
using System.Linq.Expressions;

#if MYSQL
namespace Test_NUnit_MySql.GenericTests
#elif ORACLE
#if ODP
        namespace Test_NUnit_OracleODP.GenericTests
#else
        namespace Test_NUnit_Oracle.GenericTests
#endif
#elif POSTGRES
    namespace Test_NUnit_PostgreSql.GenericTests
#elif SQLITE
    namespace Test_NUnit_Sqlite.GenericTests
#elif INGRES
    namespace Test_NUnit_Ingres.GenericTests
#elif MSSQL
namespace Test_NUnit_MsSql.GenericTests
#else
#error unknown target
#endif
{
    [TestFixture]
    public class GroupBy : TestBase
    {
        public void ProyectGroupBy<K, T, R>(Expression<Func<T, K>> groupBy, Expression<Func<IGrouping<K, T>, R>> groupProject)
        {
            Northwind db = CreateDB();
            Table<T> querySource = db.GetTable<T>();
            IEnumerable<R> queryResults = querySource.GroupBy(groupBy).Select(groupProject);

            if (queryResults.Count() == 0)
                Assert.Ignore("Inconclusive when query returns 0 elements");

            IEnumerable<T> memorySource = db.GetTable<T>().ToList();
            IEnumerable<R> memoryResults = memorySource.GroupBy(groupBy.Compile()).Select(groupProject.Compile());

            IEnumerator<R> memoryIterator = memoryResults.GetEnumerator();
            IEnumerator<R> queryIterator = queryResults.GetEnumerator();

            while (memoryIterator.MoveNext() && queryIterator.MoveNext())
            {
                Assert.AreEqual(memoryIterator.Current, queryIterator.Current);
            }
            Assert.AreEqual(memoryIterator.MoveNext(), queryIterator.MoveNext());
        }


        [Test]
        public void GroupByMember()
        {
            ProyectGroupBy(Projections.Member, a => a.Key);
            ProyectGroupBy(Projections.MemberPropertyAccess, a => a.Count());
            ProyectGroupBy(Projections.MemberPropertyAccess, a => a.Select(od => od.ProductID).Sum());
            ProyectGroupBy(Projections.MemberPropertyAccess, a => new { a.First().Order, a.First().Quantity });

        }

        [Test]
        public void GroupByArithmeticExpresion()
        {
            ProyectGroupBy(Projections.AricmeticAdd, a => a.Key);
            ProyectGroupBy(Projections.AricmeticAdd, a => a.Count());
            ProyectGroupBy(Projections.AricmeticAdd, a => a.Select(od => od.ProductID).Sum());
            ProyectGroupBy(Projections.AricmeticAdd, a => new { a.First().Order, a.First().Quantity });
        }

        [Test]
        public void GroupByConstant()
        {
            ProyectGroupBy(Projections.ConstantInt, a => a.Key);
            ProyectGroupBy(Projections.ConstantInt, a => a.Count());
            ProyectGroupBy(Projections.ConstantInt, a => a.Select(od => od.ProductID).Sum());
            ProyectGroupBy(Projections.ConstantInt, a => new { a.First().Order, a.First().Quantity });
        }

        [Test]
        public void GroupByMathExpression()
        {
            ProyectGroupBy(Projections.MathComplexExpression, a => a.Key);
            ProyectGroupBy(Projections.MathComplexExpression, a => a.Count());
            ProyectGroupBy(Projections.MathComplexExpression, a => a.Select(od => od.ProductID).Sum());
            ProyectGroupBy(Projections.MathComplexExpression, a => new { a.First().Order, a.First().Quantity });
        }

        [Test]
        public void GroupByLogicExpression()
        {
            ProyectGroupBy(Projections.LogicComplexExpression, a => a.Key);
            ProyectGroupBy(Projections.LogicComplexExpression, a => a.Count());
            ProyectGroupBy(Projections.LogicComplexExpression, a => a.Select(od => od.ProductID).Sum());
            ProyectGroupBy(Projections.LogicComplexExpression, a => new { a.First().Order, a.First().Quantity });
        }

        [Test]
        public void GroupCastExpression()
        {
            ProyectGroupBy(Projections.Casting, a => a.Key);
            ProyectGroupBy(Projections.Casting, a => a.Count());
            ProyectGroupBy(Projections.Casting, a => a.Select(od => od.ProductID).Sum());
            ProyectGroupBy(Projections.Casting, a => new { a.First().Order, a.First().Quantity });
        }

        [Test]
        public void GroupConvertExpression()
        {
            ProyectGroupBy(Projections.Convert, a => a.Key);
            ProyectGroupBy(Projections.Convert, a => a.Count());
            ProyectGroupBy(Projections.Convert, a => a.Select(od => od.ProductID).Sum());
            ProyectGroupBy(Projections.Convert, a => new { a.First().Order, a.First().Quantity });
        }


    }
}
