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
    public class OrderBy : TestBase
    {
        public IEnumerable<R> ProyectOrdered<T, R, Q>(Expression<Func<T, R>> projection, Expression<Func<T, Q>> orderByprojection, bool desc)
        {
            Northwind db = CreateDB();
            Table<T> source = db.GetTable<T>();

            IEnumerable<R> res;

            if (desc)
                res = source
                      .OrderByDescending(orderByprojection.Compile())
                      .Select(projection.Compile());
            else
                res = source
                      .OrderBy(orderByprojection.Compile())
                      .Select(projection.Compile());

            var q = res.ToList();
            return res;
        }


        [Test]
        public void ByConstant()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantChar, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantDateTime, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantDecimal, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantDouble, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantFloat, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantInt, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantString, false);
        }
        [Test]
        public void DescByConstant()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantChar, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantDateTime, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantDecimal, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantDouble, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantFloat, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantInt, false);
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantString, false);
        }

        [Test]
        public void ByAricmeticExpression()
        {

            ProyectOrdered(Projections.ConstantChar, Projections.AricmeticAdd, false);
            ProyectOrdered(Projections.ConstantChar, Projections.AricmeticSub, false);
            ProyectOrdered(Projections.ConstantChar, Projections.AritmeticMult, false);
            ProyectOrdered(Projections.ConstantChar, Projections.AritmeticDiv, false);
        }

        [Test]
        public void DescByAricmeticExpression()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.ConstantInt, true);
            ProyectOrdered(Projections.ConstantChar, Projections.AricmeticAdd, true);
            ProyectOrdered(Projections.ConstantChar, Projections.AricmeticSub, true);
            ProyectOrdered(Projections.ConstantChar, Projections.AritmeticMult, true);
            ProyectOrdered(Projections.ConstantChar, Projections.AritmeticDiv, true);
        }

        [Test]
        public void LogicExpressions()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.LogicEqual, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicGreaterEqual, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicGreater, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicInlineIf, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicLessEqual, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicLess, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicNot, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicNotEqual, false);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicComplexExpression, false);
        }


        [Test]
        public void DescLogicExpressions()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.LogicEqual, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicGreaterEqual, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicGreater, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicInlineIf, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicLessEqual, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicLess, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicNot, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicNotEqual, true);
            ProyectOrdered(Projections.ConstantChar, Projections.LogicComplexExpression, true);
        }

        [Test]
        public void DescByComplexExpressions()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.InvokeToString, true);
            ProyectOrdered(Projections.ConstantChar, Projections.MemberPropertyAccess, true);
        }
        [Test]
        public void ByCasting()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.Casting, true);
            ProyectOrdered(Projections.ConstantChar, Projections.Convert, true);

        }
        [Test]
        public void DescByCasting()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.Casting, false);
            ProyectOrdered(Projections.ConstantChar, Projections.Convert, false);
        }

        [Test]
        public void ByMathComplexExpression()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.MathComplexExpression, true);
        }
        [Test]
        public void DescByMathComplexExpression()
        {
            ProyectOrdered(Projections.ConstantChar, Projections.MathComplexExpression, true);
        }

        [Test]
        public void ByMultiplesFileds()
        {
            ProyectOrdered(Projections.Identity, Projections.AnonymousProjectionA, true);
        }

        [Test]
        public void DescByMultiplesFileds()
        {
            ProyectOrdered(Projections.Identity, Projections.AnonymousProjectionA, false);
        }

        [Test]
        public void ByMultiplesFiledsAndExpression()
        {
            ProyectOrdered(Projections.Identity, Projections.AnonymousProjectionWhithSubCountExpression, true);
        }

        [Test]
        public void DescByMultiplesFiledsAndExpression()
        {
            ProyectOrdered(Projections.Identity, Projections.AnonymousProjectionWhithSubCountExpression, false);
        }
    }
}
