using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using nwind;
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
    public class Projections
    {
        public static readonly Expression<Func<IEnumerable<int>, int>> Max = c => c.Max();
        public static readonly Expression<Func<IEnumerable<int>, int>> Min = c => c.Min();
        public static readonly Expression<Func<IEnumerable<int>, int>> Count = c => c.Count();
        public static readonly Expression<Func<IEnumerable<int>, int>> Sum = c => c.Sum();
        public static readonly Expression<Func<IEnumerable<int>, double>> Avg = c => c.Average();

        public static readonly Expression<Func<IEnumerable<int>, int>> MaxPredicate = c => c.Max(i => i % 2);
        public static readonly Expression<Func<IEnumerable<int>, int>> MinPredicate = c => c.Min(i => i % 2);
        public static readonly Expression<Func<IEnumerable<int>, int>> CountPredicate = c => c.Count(i => i % 2 == 0);
        public static readonly Expression<Func<IEnumerable<int>, int>> SumPredicate = c => c.Sum(i => i % 2);
        public static readonly Expression<Func<IEnumerable<int>, double>> AvgPredicate = c => c.Average(i => i % 2);

        public static readonly Expression<Func<OrderDetail, int>> ConstantInt = od => 3;
        public static readonly Expression<Func<OrderDetail, string>> ConstantString = od => "hi";
        public static readonly Expression<Func<OrderDetail, DateTime>> ConstantDateTime = od => DateTime.Parse("10/10/2008");
        public static readonly Expression<Func<OrderDetail, float>> ConstantFloat = od => 3f;
        public static readonly Expression<Func<OrderDetail, char>> ConstantChar = od => 'a';
        public static readonly Expression<Func<OrderDetail, decimal>> ConstantDecimal = od => 10m;
        public static readonly Expression<Func<OrderDetail, double>> ConstantDouble = od => 4d;
        public static readonly Expression<Func<OrderDetail, bool>> ConstantBoolean = od => true;


        public static readonly Expression<Func<OrderDetail, bool>> LogicEqual = od => od == od;
        public static readonly Expression<Func<OrderDetail, bool>> LogicLess = od => od.Discount < 3;
        public static readonly Expression<Func<OrderDetail, bool>> LogicLessEqual = od => od.Discount <= 3;
        public static readonly Expression<Func<OrderDetail, bool>> LogicGreater = od => od.Discount > 3;
        public static readonly Expression<Func<OrderDetail, bool>> LogicGreaterEqual = od => od.Discount >= 3;
        public static readonly Expression<Func<OrderDetail, bool>> LogicNot = od => !(od.ProductID == 0);
        public static readonly Expression<Func<OrderDetail, bool>> LogicNotEqual = od => (od.ProductID != 0);
        public static readonly Expression<Func<OrderDetail, bool>> LogicInlineIf = od => (od.ProductID != 0 ? true : false);

        public static readonly Expression<Func<OrderDetail, bool>> LogicComplexExpression = od => (((od.Discount > 4) || od.OrderID > 20 || od.Discount == 32) && (od.ProductID < 100)) ? true : false;

        public static readonly Expression<Func<OrderDetail, double>> MathSin = od => Math.Sin(od.Discount);
        public static readonly Expression<Func<OrderDetail, double>> MathCos = od => Math.Cos(od.Discount);

        public static readonly Expression<Func<OrderDetail, double>> MathFloor = od => Math.Floor(od.Discount);
        public static readonly Expression<Func<OrderDetail, double>> MathRound = od => Math.Round(od.Discount);
        public static readonly Expression<Func<OrderDetail, double>> MathTrunc = od => Math.Truncate(od.Discount);
        public static readonly Expression<Func<OrderDetail, double>> MathSign = od => Math.Round(od.Discount);

        public static readonly Expression<Func<OrderDetail, double>> MathLog = od => Math.Log(od.Discount);
        public static readonly Expression<Func<OrderDetail, double>> MathExp = od => Math.Exp(od.Discount);
        public static readonly Expression<Func<OrderDetail, double>> MathPow = od => Math.Pow(od.Discount, 2);
        public static readonly Expression<Func<OrderDetail, double>> MathSqrt = od => Math.Sqrt(od.Discount);

        public static readonly Expression<Func<OrderDetail, double>> MathComplexExpression = od => (Math.Cos(Math.Sin(23)) * 34 + Math.Log(Math.Floor(32.4)) - Math.Pow(3, Math.Round(4.3)));


        public static readonly Expression<Func<OrderDetail, int>> AricmeticAdd = od => od.Quantity + 3;
        public static readonly Expression<Func<OrderDetail, int>> AricmeticSub = od => od.Quantity - 3;
        public static readonly Expression<Func<OrderDetail, int>> AritmeticMult = od => od.Quantity * 3;
        public static readonly Expression<Func<OrderDetail, int>> AritmeticDiv = od => od.Quantity / 3;
        public static readonly Expression<Func<OrderDetail, int>> AritmeticSign = od => -od.Quantity;


        public static readonly Expression<Func<OrderDetail, OrderDetail>> Identity = o => o;
        public static readonly Expression<Func<OrderDetail, OrderDetail>> IdentityWithExpression = o => o.Order.OrderDetails.Where(od2 => od2.ProductID == od2.ProductID).First();


        public static readonly Expression<Func<OrderDetail, double>> Casting = od => (double)od.Quantity;
        public static readonly Expression<Func<OrderDetail, double>> Convert = od => System.Convert.ToDouble(od.Quantity);


        public static readonly Expression<Func<OrderDetail, string>> InvokeToString = od => od.ToString();
        public static readonly Expression<Func<OrderDetail, string>> Concat = od => "Quantity=" + od.ToString();

        //In theory OrderDetail has a references on existence to order so it shouldn't get error
        public static readonly Expression<Func<OrderDetail, Order>> Member = od => od.Order;
        public static readonly Expression<Func<OrderDetail, int>> MemberPropertyAccess = od => od.Product.ProductID;
        public static readonly Expression<Func<OrderDetail, string>> InvokeMemberToString = od => od.Order.ToString();
        public static readonly Expression<Func<OrderDetail, int>> InvokeMemberCount = od => od.Order.OrderDetails.Count();
        public static readonly Expression<Func<OrderDetail, bool>> InvokeMemberAny = od => od.Order.OrderDetails.Any();

        public static readonly Expression<Func<OrderDetail, object>> AnonymousProjectionA = od => new { Description = od.ToString(), Quantity = od.Quantity };
        public static readonly Expression<Func<OrderDetail, object>> AnonymousProjectionWhithSubCountExpression = od => new { Description = od.Order.OrderDetails.Count(), Quantity = od.Quantity };
        public static readonly Expression<Func<OrderDetail, object>> AnonymousProjectionWhithSubProjectionExpression = od => new { BrothersSumatory = od.Order.OrderDetails.Select(sod => sod.Quantity), Quantity = od.Quantity };
        public static readonly Expression<Func<OrderDetail, object>> AnonymousProjectionWhithSubWhereExpression = od => new { BrothersSumatory = od.Order.OrderDetails.Where(sod => sod.Quantity % 2 == 0), Quantity = od.Quantity };
        public static readonly Expression<Func<OrderDetail, object>> AnonymousProjectionWhithSubOrderByExpression = od => new { BrothersSumatory = od.Order.OrderDetails.OrderBy(sod => sod.Quantity), Quantity = od.Quantity };
        public static readonly Expression<Func<OrderDetail, object>> AnonymousProjectionWhithSubGroupByExpression = od => new { BrothersSumatory = od.Order.OrderDetails.OrderBy(sod => sod.Quantity), Quantity = od.Quantity };
    }
}
