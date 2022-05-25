using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Threading;


namespace Threadv2
{
	public class Program
	{
		private static bool Rule1(ulong number)
		{
			if (number == 2) return true;
			return (number & 1) == 1;	
		}

		private static bool Rule2(ulong number)
		{
			if (number == 3) return true;
			ulong counter = 0;
			while (number != 0)
			{
				counter += number % 10;
				number /= 10;	
			}
			return (counter % 3) != 0;
		}

		private static bool Rule3(ulong number)
		{
			if (number == 5) return true;
			number %= 10;
			return (number % 5) != 0;
		}

		private static bool Factorization(ulong number) 
		{
			if (number == 1) return false;
			int factors = 0;
        	for (ulong i = 1; i <= Math.Sqrt(number); i++)
			{
				if ((number % i) == 0)
				{
					factors++;
				}
			}
			if (factors == 1)
			{
			// These streams are synchronized for you
				Console.WriteLine("{0}: {1}", Thread.CurrentThread.ManagedThreadId, number);
				return true;
			}
			return false;
    	}

		private static int NumberOfPrimes(ulong begin, ulong end)
		{
			return (int)((end / Math.Log(end)) - (begin / Math.Log(begin)));
		}

		public static void Main(string[] args)
		{
			var config = args
				.Select(arg => arg.Split('='))
				.ToDictionary(pair => pair[0], pair => pair[1]);
	
			int threads = Int32.Parse(config["threads"]);
			ulong begin = UInt64.Parse(config["begin"]);
			ulong end = UInt64.Parse(config["end"]);

			ThreadPool.SetMinThreads(threads, Environment.ProcessorCount);	
			ThreadPool.SetMaxThreads(threads, Environment.ProcessorCount);

			int primes = NumberOfPrimes(begin, end);
			Console.WriteLine("Approximate number of primes: {0}", primes);
		
			Stopwatch sw = Stopwatch.StartNew();
			if (primes != 0)
			{
				using (var mre = new ManualResetEvent(false))
				{
					for (ulong i = begin; i < end; i++)
					{
						if (Rule1(i) && Rule2(i) && Rule3(i))
						{		
							ThreadPool.QueueUserWorkItem(x =>
							{
								if (Factorization((ulong)x))
								{
									if (Interlocked.Decrement(ref primes) == 0)
									{
										mre.Set();
									}

								}
							}, i); 	
						}
					}
					mre.WaitOne();
				}
			}
			sw.Stop();

			Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);
		}

	}
}