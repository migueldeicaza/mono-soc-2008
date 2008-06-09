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
    public class SimpleProyection : SimpleProjectionsBase
    {
    }
}
