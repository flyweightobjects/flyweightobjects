using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FlyweightObjects
{
    public delegate void ForDelegate(int i);
    public delegate void ThreadDelegate();

    internal sealed class Parallel
    {
        private static readonly object _syncLock = new object();

        public static void For(int index, int length, ForDelegate action)
        {
            // number of process() threads
            int threadCount = Environment.ProcessorCount;

            // ChunkSize = 1 makes items to be processed in order.
            // Bigger chunk size should reduce lock waiting time and thus
            // increase paralelism.
            int chunkSize = length > threadCount ? length / threadCount : threadCount;
            int cnt = index - chunkSize;
            
            // processing function
            // takes next chunk and processes it using action
            ThreadDelegate process = delegate()
            {
                while (true)
                {
                    int cntMem = 0;
                    lock (_syncLock)
                    {
                        cnt += chunkSize;
                        cntMem = cnt;
                    }

                    // process chunk
                    // here items can come out of order if chunkSize > 1
                    for (int i = cntMem; i < cntMem + chunkSize; ++i)
                    {
                        if (i >= length)
                        {
                            return;
                        }
                        action(i);
                    }
                }
            };

            // launch process() threads
            IAsyncResult[] asyncResults = new IAsyncResult[threadCount];
            for (int i = 0; i < threadCount; ++i)
            {
                asyncResults[i] = process.BeginInvoke(null, null);
            }

            // wait for all threads to complete
            for (int i = 0; i < threadCount; ++i)
            {
                process.EndInvoke(asyncResults[i]);
            }
        }
    }
}
