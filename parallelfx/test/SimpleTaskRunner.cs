using System;
using System.Threading;
using System.Threading.Tasks;

class MainClass
{
	const int max = 100;
	
	static int c = 0;
	
	static Random r = new Random();
	
	public static void Main()
	{
		for (int i = 0; i < max; i++) {
			Task.Create(delegate {
				int temp = Interlocked.Increment(ref c);
				Console.WriteLine("scwl num {0} from {1}", temp.ToString(), Thread.CurrentThread.ManagedThreadId.ToString());
				Thread.Sleep(r.Next(500, 1000));
				Console.WriteLine("Out of " + temp.ToString());
			});
		}
		Task.WaitAllInTaskManager(TaskManager.Default);
	}
}
