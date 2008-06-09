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
    public class ScalarProjection : TestBase
    {
        public void ProjectScalar(Expression<Func<IEnumerable<int>, int>> scalarProjection)
        {

            Northwind db = CreateDB();
            int resQuery = db.OrderDetails.Select(od => (int)od.Quantity).Provider.Execute<int>(scalarProjection.Body);
            int resMemory = scalarProjection.Compile()(db.OrderDetails.Select(od => (int)od.Quantity).ToList());
            Assert.AreEqual(resQuery, resMemory);
        }

        [Test]
        public void MaxField()
        {
            ProjectScalar(Projections.Max);
        }

        [Test]
        public void MinField()
        {
            ProjectScalar(Projections.Min);
        }

        [Test]
        public void Count()
        {
            ProjectScalar(Projections.Count);
        }

        [Test]
        public void Sum()
        {
            ProjectScalar(Projections.Sum);
        }

        [Test]
        public void MaxFieldPredicate()
        {
            ProjectScalar(Projections.MaxPredicate);
        }

        [Test]
        public void MinFieldPredicate()
        {
            ProjectScalar(Projections.MinPredicate);
        }

        [Test]
        public void CountPredicate()
        {
            ProjectScalar(Projections.CountPredicate);
        }

        [Test]
        public void SumPredicate()
        {
            ProjectScalar(Projections.SumPredicate);
        }
    }
}
