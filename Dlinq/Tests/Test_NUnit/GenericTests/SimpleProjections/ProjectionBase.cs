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
    public class SimpleProjectionsBase : TestBase
    {
        public virtual void Project<T, R>(Expression<Func<T, R>> Projection)
        {
            Northwind db = CreateDB();
            Table<T> source = db.GetTable<T>();

            IEnumerable<R> res = source.Select(Projection.Compile());
            List<R> queryResults = res.ToList();

            if (queryResults.Count() == 0)
                Assert.Ignore("Inconclusive when query returns 0 elements");

            IEnumerable<T> memorySource = db.GetTable<T>().ToList();
            IEnumerable<R> memoryResults = memorySource.Select(Projection.Compile());

            IEnumerator<R> memoryIterator = memoryResults.GetEnumerator();
            IEnumerator<R> queryIterator = queryResults.GetEnumerator();

            while (memoryIterator.MoveNext() && queryIterator.MoveNext())
            {
                Assert.AreEqual(memoryIterator.Current, queryIterator.Current);
            }
            Assert.AreEqual(memoryIterator.MoveNext(), queryIterator.MoveNext());
        }

        [Test]
        public void Constants_Logic_Arithmetics_Maths()
        {
            Project(Projections.ConstantChar);
            Project(Projections.ConstantDateTime);
            Project(Projections.ConstantDecimal);
            Project(Projections.ConstantDouble);
            Project(Projections.ConstantFloat);
            Project(Projections.ConstantInt);
            Project(Projections.ConstantString);
            Project(Projections.ConstantBoolean);

            Project(Projections.AricmeticAdd);
            Project(Projections.AricmeticSub);
            Project(Projections.AritmeticMult);
            Project(Projections.AritmeticDiv);
            Project(Projections.AritmeticSign);

            Project(Projections.LogicEqual);
            Project(Projections.LogicGreaterEqual);
            Project(Projections.LogicGreater);
            Project(Projections.LogicInlineIf);
            Project(Projections.LogicLessEqual);
            Project(Projections.LogicLess);
            Project(Projections.LogicNot);
            Project(Projections.LogicNotEqual);
            Project(Projections.LogicComplexExpression);

                Project(Projections.MathCos);
                Project(Projections.MathExp);
                Project(Projections.MathFloor);
                Project(Projections.MathLog);
                Project(Projections.MathPow);
                Project(Projections.MathRound);
                Project(Projections.MathSign);
                Project(Projections.MathSin);
                Project(Projections.MathSqrt);
                Project(Projections.MathSqrt);

                Project(Projections.MathComplexExpression);
        }

    
        [Test]
        public void ProjectIdentity()
        {
            Project(Projections.Identity);
        }
        [Test]
        public void ProjectIdentityExpression()
        {
            Project(Projections.IdentityWithExpression);
        }

        [Test]
        public void ProjectCasting()
        {
            Project(Projections.Casting);
        }

        [Test]
        public void ProjectConcat()
        {
            Project(Projections.Concat);
        }

        [Test]
        public void ProjectMemberAccess()
        {
            Project(Projections.MemberPropertyAccess);
        }

        [Test]
        public void ProjectAnonymousTuple()
        {
            Project(Projections.AnonymousProjectionA);
        }

        [Test]
        public void ProjectAnonymousWithSubCountExpression()
        {
            Project(Projections.AnonymousProjectionWhithSubCountExpression);
        }

        [Test]
        public void ProjectAnonymousWithSubGroupExpression()
        {
            Project(Projections.AnonymousProjectionWhithSubGroupByExpression);
        }

        [Test]
        public void ProjectAnonymousWithSubOrderByExpression()
        {
            Project(Projections.AnonymousProjectionWhithSubOrderByExpression);
        }

        [Test]
        public void ProjectAnonymousWithSubProjectionByExpression()
        {
            Project(Projections.AnonymousProjectionWhithSubProjectionExpression);
        }

        public void ProjectAnonymousWithSubWhereByExpression()
        {
            Project(Projections.AnonymousProjectionWhithSubWhereExpression);
        }

        [Test]
        public void ProjectInvokeToString()
        {
            Project(Projections.InvokeToString);
        }

        [Test]
        public void ProjectMemberAny()
        {
            Project(Projections.InvokeMemberAny);
        }

        [Test]
        public void ProjectMemberToString()
        {
            Project(Projections.InvokeMemberToString);
        }

        [Test]
        public void ProjectMemberCount()
        {
            Project(Projections.InvokeMemberCount);
        }
    }
}
