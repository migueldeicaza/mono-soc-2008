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
    public class ProjectIntegerAndThenScalarProjection:TestBase
    {
        [Test]
        public void ProjectConstant()
        {
            ProjectDouble(Projections.ConstantInt, Projections.Count);
            ProjectDouble(Projections.ConstantInt, Projections.Max);
            ProjectDouble(Projections.ConstantInt, Projections.Min);


            ProjectDouble(Projections.ConstantInt, Projections.CountPredicate);
            ProjectDouble(Projections.ConstantInt, Projections.MaxPredicate);
            ProjectDouble(Projections.ConstantInt, Projections.MinPredicate);
        }

        [Test]
        public void ProjectAdd()
        {

            ProjectDouble(Projections.AricmeticAdd, Projections.Count);
            ProjectDouble(Projections.AricmeticAdd, Projections.Max);
            ProjectDouble(Projections.AricmeticAdd, Projections.Min);


            ProjectDouble(Projections.AricmeticAdd, Projections.CountPredicate);
            ProjectDouble(Projections.AricmeticAdd, Projections.MaxPredicate);
            ProjectDouble(Projections.AricmeticAdd, Projections.MinPredicate);
        }

        [Test]
        public void ProjectSub()
        {

            ProjectDouble(Projections.AricmeticSub, Projections.Count);
            ProjectDouble(Projections.AricmeticSub, Projections.Max);
            ProjectDouble(Projections.AricmeticSub, Projections.Min);


            ProjectDouble(Projections.AricmeticSub, Projections.CountPredicate);
            ProjectDouble(Projections.AricmeticSub, Projections.MaxPredicate);
            ProjectDouble(Projections.AricmeticSub, Projections.MinPredicate);
        }

        [Test]
        public void ProjectMult()
        {

            ProjectDouble(Projections.AritmeticMult, Projections.Count);
            ProjectDouble(Projections.AritmeticMult, Projections.Max);
            ProjectDouble(Projections.AritmeticMult, Projections.Min);


            ProjectDouble(Projections.AritmeticMult, Projections.CountPredicate);
            ProjectDouble(Projections.AritmeticMult, Projections.MaxPredicate);
            ProjectDouble(Projections.AritmeticMult, Projections.MinPredicate);
        }

        [Test]
        public void ProjectDiv()
        {
            ProjectDouble(Projections.AritmeticDiv, Projections.Count);
            ProjectDouble(Projections.AritmeticDiv, Projections.Max);
            ProjectDouble(Projections.AritmeticDiv, Projections.Min);

            ProjectDouble(Projections.AritmeticDiv, Projections.CountPredicate);
            ProjectDouble(Projections.AritmeticDiv, Projections.MaxPredicate);
            ProjectDouble(Projections.AritmeticDiv, Projections.MinPredicate);
        }

        //[Test]
        //public void ProjectSimple()
        //{
        //    ProjectDouble(Projections.Simple);
        //}

        //[Test]
        //public void ProjectCasting()
        //{
        //    ProjectDouble(Projections.Casting);
        //}

        //[Test]
        //public void ProjectConcat()
        //{
        //    ProjectDouble(Projections.Concat);
        //}

        //[Test]
        //public void ProjectMemberAccess()
        //{
        //    ProjectDouble(Projections.MemberAccess);
        //}

        //[Test]
        //public void ProjectAnonymousTuple()
        //{
        //    ProjectDouble(Projections.AnonymousProjectionA);
        //}

        //[Test]
        //public void ProjectAnonymousWithExpression()
        //{
        //    ProjectDouble(Projections.AnonymousProjectionWhithExpression);
        //}

        //[Test]
        //public void ProjectInvokeToString()
        //{
        //    ProjectDouble(Projections.InvokeToString);
        //}

        //[Test]
        //public void ProjectMemberAny()
        //{
        //    ProjectDouble(Projections.InvokeMemberAny);
        //}

        //[Test]
        //public void ProjectMemberToString()
        //{
        //    ProjectDouble(Projections.InvokeMemberToString);
        //}

        //[Test]
        //public void ProjectMemberCount()
        //{
        //    ProjectDouble(Projections.InvokeMemberCount);
        //}



        public void ProjectDouble<T>(Expression<Func<T, int>> firstProjection, Expression<Func<IEnumerable<int>, int>> scalarFunction)
        {
            Northwind db = CreateDB();
            Table<T> source = db.GetTable<T>();

            int res = source
                      .Select(firstProjection).AsQueryable()
                      .Provider.Execute<int>(scalarFunction.Body);

            Console.WriteLine(res);
        }
    }
}
