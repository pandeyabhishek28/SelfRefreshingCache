using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment.SelfRefreshingCache
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello there!");
            Console.WriteLine("I am going to show you an example of SelfRefreshingCache.");


            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger selfRefreshingCachelogger = loggerFactory.CreateLogger<SelfRefreshingCache<int>>();



            var random = new Random();
            Func<int> createFunction = () =>
            {
                Thread.Sleep(2000);// Putting a delay here, so that we can assume it is doing something
                // great that taking time and we are waiting to see the result.

                Console.WriteLine(Environment.NewLine + "Hello from Create Function." + Environment.NewLine);

                return random.Next(0, int.MaxValue);
            };

            SelfRefreshingCache<int> selfRefreshingCache = null;
            try
            {
                #region First test scenario

                selfRefreshingCache = new SelfRefreshingCache<int>(selfRefreshingCachelogger,
                                           refreshPeriodInSeconds: 5,
                                           validityOfResultInSeconds: 10, createFunction);

                ParallelOptions options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.For(0, 50, options, (index) =>
                  {
                      try
                      {
                          Thread.Sleep(100 * index);
                          // Putting a delay here, so that we can check our selfRefreshingCache is properly working or not.
                          var result = selfRefreshingCache.GetOrCreate();
                          Console.WriteLine(" Result :  " + result);
                      }
                      catch (Exception ex)
                      {
                          selfRefreshingCachelogger.LogError(ex, "An exception while creating using the selfRefreshingCache.");
                      }
                  });

                #endregion

                #region Second test scenario


                //Parallel.For(0, 50, (index) =>
                //{
                //    var selfRefreshingCache = new SelfRefreshingCache<int>(logger,
                //                            refreshPeriodInSeconds: 5,
                //                            validityOfResultInSeconds: 10, createFunction);
                //    Thread.Sleep(100 * index); // Putting a delay here, so that we can check our selfRefreshingCache is properly working or not.
                //    var result = selfRefreshingCache.GetOrCreate();
                //    Console.WriteLine(" Result :  " + result);
                //    selfRefreshingCache.Dispose();
                //});


                #endregion

                #region Third test scenario

                //var concurrentStack = new ConcurrentStack<SelfRefreshingCache<int>>();
                //Parallel.For(0, 50, (index) =>
                //{
                //    var selfRefreshingCache = new SelfRefreshingCache<int>(logger,
                //                            refreshPeriodInSeconds: 5,
                //                            validityOfResultInSeconds: 10, createFunction);
                //    concurrentStack.Push(selfRefreshingCache);
                //    Thread.Sleep(100 * index); // Putting a delay here, so that we can check our selfRefreshingCache is properly working or not.
                //    var result = selfRefreshingCache.GetOrCreate();
                //    Console.WriteLine(" Result :  " + result);
                //});

                //// You can wait as long as you want and after that
                //foreach (var item in concurrentStack)
                //{
                //    item.Dispose();
                //}
                #endregion
            }
            catch (Exception ex)
            {
                selfRefreshingCachelogger.LogError(ex, "An exception while excuting the Main.");
            }
            finally
            {
                if (selfRefreshingCache != null)
                    selfRefreshingCache.Dispose();
                loggerFactory.Dispose();
            }

            Console.WriteLine("Done.");
            Console.Read();
        }
    }
}
