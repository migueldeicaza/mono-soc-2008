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
    public abstract class NestedProjections<T> : SimpleProjectionsBase
    {

        protected abstract Expression<Func<T, R>>[] FirstProjection<R>();
        public override void Project<R, Q>(Expression<Func<R, Q>> Projection)
        {
            foreach (var firstProjection in FirstProjection<R>())
                Test<R, Q>(firstProjection, Projection);
        }

        public void Test<R, Q>(Expression<Func<T, R>> Projection, Expression<Func<R, Q>> secondProjection)
        {
            Northwind db = CreateDB();
            Table<T> source = db.GetTable<T>();

            IEnumerable<Q> res = source.Select(Projection.Compile()).Select(secondProjection.Compile());
            var queryResults = res.ToList();

            if (queryResults.Count() == 0)
                Assert.Ignore("Inconclusive when query returns 0 elements");

            IEnumerable<T> memorySource = db.GetTable<T>().ToList();
            IEnumerable<Q> memoryResults = memorySource.Select(Projection.Compile()).Select(secondProjection.Compile());

            IEnumerator<Q> memoryIterator = memoryResults.GetEnumerator();
            IEnumerator<Q> queryIterator = queryResults.GetEnumerator();

            while (memoryIterator.MoveNext() && queryIterator.MoveNext())
            {
                Assert.AreEqual(memoryIterator.Current, queryIterator.Current);
            }
            Assert.AreEqual(memoryIterator.MoveNext(), queryIterator.MoveNext());
        }
    }

    [Category("NestedProjections")]
    [TestFixture]
    public class NestedProjection_OrderToOrderDetail : NestedProjections<Order>
    {

        protected override Expression<Func<Order, R>>[] FirstProjection<R>()
        {
            if (typeof(R) != typeof(OrderDetail))
                Assert.Ignore("Test not applicable");

            return new Expression<Func<Order, OrderDetail>>[]
            {
                o=>o.OrderDetails.First(od=>od.Discount<0.5)
            } as Expression<Func<Order, R>>[];
        }
    }

    [Category("NestedProjections")]
    [TestFixture]
    public class NestedProjection_IdentityOrderDetail : NestedProjections<OrderDetail>
    {
        protected override Expression<Func<OrderDetail, R>>[] FirstProjection<R>()
        {
            if (typeof(R) != typeof(OrderDetail))
                Assert.Ignore("Test not applicable");

            return new Expression<Func<OrderDetail, OrderDetail>>[]
            {
                od=>od
            } as Expression<Func<OrderDetail, R>>[];
        }
    }
}
