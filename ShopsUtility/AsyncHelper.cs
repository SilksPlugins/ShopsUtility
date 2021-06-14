using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ShopsUtility
{
    public static class AsyncHelper
    {
        public static void Run(Func<Task> task)
        {
            Task.Run(async () =>
            {
                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }
    }
}
