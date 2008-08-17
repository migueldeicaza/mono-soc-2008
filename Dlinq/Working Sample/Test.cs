using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsNorthwind;
using System.Data;

namespace SDLinqTest
{
    public class Test
    {
        public static void Main()
        {
            Console.WriteLine("OS:" + System.Environment.OSVersion + ", Environment.Version:" + System.Environment.Version.Build);
            IDbConnection conn = new System.Data.SqlClient.SqlConnection("Data Source=192.168.1.61\\SQLEXPRESS,1154;Integrated Security=False;Initial Catalog=Northwind;User Id=dblinqUser;Password=linq2");
            conn.Open();
            Console.WriteLine(conn.State);

            Northwind nwindDb = new Northwind(conn);

            //simple scalar query
            Console.WriteLine("Number of Employees:" + nwindDb.Customers.Count());


            //filter + orderby
            var customers = nwindDb.Customers.Where(e => e.Orders.Any())
                                            .OrderByDescending(c => c.CustomerID)
                                            .ToList();

            foreach (var a in customers)
            {
                Console.WriteLine("Customer '{0}':", a.CustomerID);

                //anonymous projections
                var customerOrdersInfo = a.Orders.Select(o => new { o, o.ShipAddress });
                Console.WriteLine("\tNumber of pending orders:" + a.Orders.Count);

                //entitySet access
                if (a.Orders.Any())
                {
                    foreach (var orderInfo in customerOrdersInfo)
                    {
                        Console.WriteLine("\t·order: {0}", orderInfo.o.OrderID);
                        if (!string.IsNullOrEmpty(orderInfo.o.ShipAddress))
                            Console.WriteLine("\tIt should be sent to: {0} {1} {2}", orderInfo.ShipAddress, orderInfo.o.ShipCity, orderInfo.o.ShipCountry);
                    };

                }
                Console.WriteLine();
            }

            //more complex scalar query
            bool d = nwindDb.Orders.Any(o => o.Customer.ContactName == "WARTH");
            Console.WriteLine();
            Console.WriteLine("Are there any order for WARTH? " + (d ? "yes" : "no").ToString());
            Console.WriteLine();


            //anonymous projection+ filter query + orderby 
            var q = (from e in nwindDb.Employees
                     where e.Orders.Any(o => o.Customer.CustomerID == "AIRBU" || o.Customer.CustomerID == "BONAP")
                     orderby e.FirstName
                     select new { Name = e.FirstName + "," + e.LastName, e.HomePhone }).ToList();


            foreach (var emp in q)
                Console.WriteLine(emp.Name + ", Phone:" + emp.HomePhone);


            //strings operations
            var shortphones = nwindDb.Customers.Where(c => (c.CustomerID.Trim().Contains("T") || c.Country == "France"))
                                       .Select(z => new { z.Phone, Name = z.CustomerID.ToLower() });

            foreach (var shortphone in shortphones)
                if (shortphone != null)
                    Console.WriteLine(shortphone.Name + ":" + shortphone.Phone);

            Customer newCust = nwindDb.Customers.FirstOrDefault(x => x.CustomerID == "PAUL");
            if (newCust != null)
            {
                //write/delete
                Console.WriteLine("customer 'PAUL' already exists, removing...");
                nwindDb.Customers.DeleteOnSubmit(newCust);
                nwindDb.SubmitChanges();
                Console.WriteLine("done");
            }

            
            Console.WriteLine("Checking if Paul is in the database");
            newCust = nwindDb.Customers.FirstOrDefault(x => x.CustomerID == "PAUL");
            if (newCust != null)
                Console.WriteLine("Paul is in the database");
            else
                Console.WriteLine("Paul is not in the database");

            //write/insert
            Console.WriteLine("Inserting new customer..");
            newCust = new Customer { CustomerID = "PAUL", City = "Seville", Country = "Spain" ,CompanyName="Mono"};
            nwindDb.Customers.InsertOnSubmit(newCust);
            nwindDb.SubmitChanges();
            Console.WriteLine("done");

            
            newCust = nwindDb.Customers.FirstOrDefault(x => x.CustomerID == "PAUL");
            if (newCust != null)
            {
                Console.WriteLine("Paul is in the database");
                Console.WriteLine("{0} {1} {2}", newCust.CustomerID, newCust.City, newCust.Country);
            }
            else
                Console.WriteLine("Paul is not in the database");


            Console.ReadLine();
        }
    }
}
