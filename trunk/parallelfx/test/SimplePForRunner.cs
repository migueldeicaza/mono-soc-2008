using System;
using System.Threading;
using System.Threading.Tasks;

class MainClass
{
	const int max = 50;
	
	static Random r = new Random();
	
	public static void Main()
	{
		Parallel.For (0, max, delegate (int i) {
			//int temp = Interlocked.Increment(ref c);
			Console.WriteLine("scwl num {0} from {1}", i.ToString(), Thread.CurrentThread.ManagedThreadId.ToString());
			Thread.Sleep(r.Next(500, 1000));
		});
		Task.WaitAllInTaskManager(TaskManager.Default);
	}
}
