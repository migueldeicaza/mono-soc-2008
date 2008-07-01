using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

class MainClass
{
	const int max = 10000;
	
	static int c = 3000;
	
	static Random r = new Random();
	
	public static void Main()
	{
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < max; i++) {
			double temp = Interlocked.Increment(ref c);
			for (int j = 0; j < 200; j++)
				temp = Math.Exp(Math.Log10(Math.Log(temp)));
			//Console.WriteLine("{0} from {1}", temp.ToString(), Thread.CurrentThread.ManagedThreadId.ToString());
			//Thread.Sleep(r.Next(500, 1000));
			//Console.WriteLine("Out of " + temp.ToString());
		}
		sw.Stop();
		Console.WriteLine("\n\n[Time elapsed for synchronous : {0}]\n\n", sw.Elapsed.ToString());
		sw.Reset();
		
		c = 3000;
		
		sw.Start();
		for (int i = 0; i < max; i++) {
			Task.Create(delegate {
				double temp = Interlocked.Increment(ref c);
				for (int j = 0; j < 200; j++)
					temp = Math.Exp(Math.Log10(Math.Log(temp)));
				//Console.WriteLine("scwl num {0} from {1}", temp.ToString(), Thread.CurrentThread.ManagedThreadId.ToString());
				//Thread.Sleep(r.Next(500, 1000));
				//Console.WriteLine("Out of " + temp.ToString());
			});
		}
		Task.WaitAllInTaskManager(TaskManager.Default);
		sw.Stop();
		Console.WriteLine("\n\n[Time elapsed for asynchronous : {0}]\n\n", sw.Elapsed.ToString());
	}
}
