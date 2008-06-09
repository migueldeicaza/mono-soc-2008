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
    public abstract class WhereBase : SimpleProjectionsBase
    {
        protected abstract Expression<Func<T, bool>>[] GetExpressionFamily<T>();

        public override void Project<T, R>(Expression<Func<T, R>> Projection)
        {
            foreach (var exp in GetExpressionFamily<T>())
                Test(Projection, exp);
        }
        public void Test<T, R>(Expression<Func<T, R>> projection, Expression<Func<T, bool>> whereExpression)
        {
            Northwind db = CreateDB();
            Table<T> source = db.GetTable<T>();

            IEnumerable<R> res = source.Where(whereExpression.Compile()).Select(projection.Compile());
            List<R> queryResults = res.ToList();

            IEnumerable<T> memorySource = db.GetTable<T>().ToList();
            IEnumerable<R> memoryResults = memorySource.Where(whereExpression.Compile()).Select(projection.Compile());

            IEnumerator<R> memoryIterator = memoryResults.GetEnumerator();
            IEnumerator<R> queryIterator = queryResults.GetEnumerator();

            while (memoryIterator.MoveNext() && queryIterator.MoveNext())
            {
                Assert.AreEqual(memoryIterator.Current, queryIterator.Current);
            }
            Assert.AreEqual(memoryIterator.MoveNext(), queryIterator.MoveNext());
        }
    }

    [Category("Where Tests")]
    [TestFixture]
    public class WhereLogicComprarers : WhereBase
    {
        protected override Expression<Func<T, bool>>[] GetExpressionFamily<T>()
        {
            if (typeof(T) != typeof(OrderDetail))
                Assert.Ignore("Test not applicable");

            return new Expression<Func<OrderDetail, bool>>[]{
                    od => od.Discount < 0.3,
                    od => od.Discount <= 0.4,
                    od => od.Discount > 0.3,
                    od => od.Discount >= 0.4
            } as Expression<Func<T, bool>>[];

        }
    }

    [Category("Where Tests")]
    [TestFixture]
    public class WhereLogicEqualAndNotEqual : WhereBase
    {
        protected override Expression<Func<T, bool>>[] GetExpressionFamily<T>()
        {
            if (typeof(T) != typeof(OrderDetail))
                Assert.Ignore("Test not applicable");

            return new Expression<Func<OrderDetail, bool>>[]{
                    od => !(od.ProductID == 0),
                    od => (od.ProductID != 0),
                    od => od == od,
                    od=>od.ProductID==od.ProductID,
                    od => (od.ProductID != 0) ? true : false 
                } as Expression<Func<T, bool>>[];

        }
    }


    [Category("Where Tests")]
    [TestFixture]
    public class WhereComplexLogicExpression : WhereBase
    {
        protected override Expression<Func<T, bool>>[] GetExpressionFamily<T>()
        {
            if (typeof(T) != typeof(OrderDetail))
                Assert.Ignore("Test not applicable");

            return new Expression<Func<OrderDetail, bool>>[]
                {
                    od => (((od.Discount > 4) || od.OrderID > 20 || od.Discount == 32) && (od.ProductID < 100)) ? true : false||true,
                    od => ((!(od.Discount < 4) && od.Discount > 20||false || od.Discount == 32)?true:false||true)
                } as Expression<Func<T, bool>>[];

        }
    }

    [Category("Where Tests")]
    [TestFixture]
    public class WhereCountMemberProperty : WhereBase
    {
        protected override Expression<Func<T, bool>>[] GetExpressionFamily<T>()
        {
            if (typeof(T) != typeof(OrderDetail))
                Assert.Ignore("Test not applicable");

            return new Expression<Func<OrderDetail, bool>>[]
                {
                    od => od.Order.OrderDetails.Any(od2=>od2.Discount<0.5)
                } as Expression<Func<T, bool>>[];

        }
    }

    [Category("Where Tests")]
    [TestFixture]
    public class WhereSelectAndComplexMathsIntoWhereProperty : WhereBase
    {
        protected override Expression<Func<T, bool>>[] GetExpressionFamily<T>()
        {
            if (typeof(T) != typeof(OrderDetail))
                Assert.Ignore("Test not applicable");

            return new Expression<Func<OrderDetail, bool>>[]
                {
                    od => od.Order.OrderDetails.Sum(od2=>od.UnitPrice)/(decimal)od.Order.OrderDetails.Count()>od.UnitPrice
                } as Expression<Func<T, bool>>[];
        }
    }

}
