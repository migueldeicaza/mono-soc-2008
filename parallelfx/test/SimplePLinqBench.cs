using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

class MainClass
{
	
	public static void Main()
	{
		Stopwatch sw = Stopwatch.StartNew();
		long synch;
		
		sw.Start();
		foreach (int i in Enumerable.Range(1, 20).Select(i => { Thread.Sleep(500);  return i + 1; }).Where(i => i % 2 == 0)) {
			Console.Write(i + ", ");
			Thread.Sleep(100); 
		}
		sw.Stop();
		Console.WriteLine("\n\n[Time elapsed for synchronous : {0}]\n\n", sw.Elapsed.ToString());
		synch = sw.ElapsedMilliseconds;
		sw.Reset();
		Console.WriteLine();
		
		sw.Start();
		foreach (int i in 
			ParallelEnumerable.Range(1, 20).Select(i => { Thread.Sleep(500); return i + 1; }).Where(i => { return i % 2 == 0; })) {
			Console.Write(i + ", ");
			Thread.Sleep(100); 	
		}
		sw.Stop();
		Console.WriteLine("\n\n[Time elapsed for asynchronous : {0}]\n\n", sw.Elapsed.ToString());
		Console.WriteLine("Speedup : {0:F2}", synch / (double)sw.ElapsedMilliseconds);
	}
}
