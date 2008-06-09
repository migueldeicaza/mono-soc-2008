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
    public class Distinct : SimpleProjectionsBase
    {
        public override void Project<T,R>(Expression<Func<T, R>> Projection)
        {

            Northwind db = CreateDB();
            Table<T> source = db.GetTable<T>();

            IEnumerable<R> res = source.Select(Projection).Distinct();
            List<R> queryResults = res.ToList();

            if (queryResults.Count() == 0)
                Assert.Ignore("Inconclusive when query returns 0 elements");

            IEnumerable<T> memorySource = db.GetTable<T>().ToList();
            IEnumerable<R> memoryResults = memorySource.Select(Projection.Compile()).Distinct();

            IEnumerator<R> memoryIterator = memoryResults.GetEnumerator();
            IEnumerator<R> queryIterator = queryResults.GetEnumerator();

            while (memoryIterator.MoveNext() && queryIterator.MoveNext())
            {
                Assert.AreEqual(memoryIterator.Current, queryIterator.Current);
            }
            Assert.AreEqual(memoryIterator.MoveNext(), queryIterator.MoveNext());

        }
    }
}
