using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerprogSzekvencialis.BL
{
    class ParallelEncrypt : Encryption
    {
        private int maxNumberOfTasks;
        SemaphoreSlim semaphoreSlim;
        Object lockObject;
        Object lockObjectForList;
        ConcurrentBag<string> readyList;

        public ParallelEncrypt(string inputFile, string outputFile, CancellationToken cancellationToken, string key, ProcessType processType, int maxNumberOfTasks) 
            : base(inputFile, outputFile, cancellationToken, key, processType)
        {
            this.maxNumberOfTasks = maxNumberOfTasks;
            this.semaphoreSlim = new SemaphoreSlim(this.maxNumberOfTasks);
            this.lockObject = new object();
            this.lockObjectForList = new object();
            this.readyList = new ConcurrentBag<string>();
        }

        public override void Start()
        {
            InitializeKey(this.key);
            StartProcessing();
        }

        public void Stop()
        {
           
        }

        public void StartProcessing()
        {
            this.baseTime = DateTime.Now;
            MessageChangeMethod("Initializing...");
            long lenghtOfFile = 0;
            int bufferSize = 3145728; //3MB*10
            int round = 0;
            using (var fileo = File.OpenRead(this.inputFile))
            {
                lenghtOfFile = fileo.Length;
            }

            int allRoundNumber = (int)Math.Ceiling((double)((double)lenghtOfFile / (double)bufferSize));

            Task appender = new Task(() => 
            {
                AppendToOutputFileWhenRead(this.outputFile, allRoundNumber);
            }, this.cancellationToken, TaskCreationOptions.LongRunning);
            appender.Start();

            for (int i = 0; i < allRoundNumber && !cancellationToken.IsCancellationRequested; i++)
            {
                int bufSize = bufferSize;
                long offs = (long)round * (long)bufferSize;
                int ro = round;
                string inpf = this.inputFile;
                string ouf = this.outputFile;
                ProcessType ty = this.processType;
                List<byte[]> keys = new List<byte[]>();
                for (int z = 0; z < this.key.Length; z++)
                {
                    keys.Add(this.keyMatrix.GetNextRow());
                }
                Task t = new Task(() =>
                {
                    EncryptCurrentRound(bufSize, offs, ro, inpf, ouf, ty, keys);

                }, this.cancellationToken, TaskCreationOptions.LongRunning);
                if (!cancellationToken.IsCancellationRequested)
                {
                    t.Start();
                }
                round++;

                //if (i % (this.maxNumberOfTasks-1) == 0 && (i != 0))
                //{
                //    Thread.Sleep(100);
                //}
            }
        }

        private void EncryptCurrentRound(int bufferSize, long offset, int roundNumber, string inputFile, string outputFile, ProcessType type, List<byte[]> keys)
        {
            semaphoreSlim.Wait();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageChangeMethod(roundNumber + ". round process started...");
                Console.WriteLine(roundNumber + " is running");
                byte[] process = new byte[bufferSize];
                int read = 0;
                lock (lockObject)
                {
                    using (var fileo = File.OpenRead(inputFile))
                    {
                        fileo.Seek(offset, SeekOrigin.Begin);
                        read = fileo.Read(process, 0, bufferSize);
                    }
                }

                if (read < bufferSize)
                {
                    byte[] newOne = new byte[read];

                    for (int i = 0; i < read; i++)
                    {
                        newOne[i] = process[i];
                    }
                    process = newOne;
                }

                byte[] output;
                switch (type)
                {
                    case ProcessType.Encrypt:
                        output = EncryptData(process, keys);
                        break;
                    case ProcessType.Decrypt:
                        output = DecryptData(process, keys);
                        break;
                    default:
                        output = EncryptData(process, keys);
                        break;
                }
                string name = "";
                using (var file = File.Create(outputFile + roundNumber))
                {
                    name = file.Name;
                    file.Write(output, 0, output.Length);
                }

                //put the list to tell the other task that it can process this file
               // lock (this.lockObjectForList)
                //{
                    this.readyList.Add(name);
               // }
            }
            semaphoreSlim.Release();
        }
        private void AppendToOutputFileWhenRead(string outpputFile, int maxRounds)
        {
          //  this.semaphoreSlim.Wait();
            int nextToAppend = 0;
            while (nextToAppend < maxRounds && !cancellationToken.IsCancellationRequested)
            {
                // lock(this.lockObjectForList)
                // {

                List<string> actualListToProcess = new List<string>();
                foreach (var item in this.readyList)
                {
                    actualListToProcess.Add(item);
                }

                Console.WriteLine(this.semaphoreSlim.CurrentCount);

               while (actualListToProcess.Contains(outpputFile+nextToAppend.ToString()))
                {
                    MessageChangeMethod(nextToAppend + ". round WRITING to file...");
                    Append(outpputFile + nextToAppend.ToString(), outpputFile);
                    nextToAppend++;
                    PercentageChangedMethod((double)nextToAppend / ((double)((maxRounds - 1) / 100.0)));
                }

                //foreach (var item in actualListToProcess)
                //{
                //    string act;
                //    this.readyList.TryTake(out act);
                //}
                if (maxRounds > 100)
              {
                    Thread.Sleep(5000);
              }
                    //foreach (var item in this.readyList)
                    //{
                    //    if (item == outpputFile+nextToAppend.ToString())
                    //    {
                    //        MessageChangeMethod(nextToAppend + ". round WRITING to file...");
                    //        Append(item, outpputFile);
                    //        nextToAppend++;
                    //        PercentageChangedMethod((double)nextToAppend/((double)((maxRounds - 1) / 100.0)));
                            
                    //    }
                    //}
              //  }

                //Thread.Sleep(1000);

            }
            MessageChangeMethod("Complete.");
            PercentageChangedMethod(100);
          //  this.semaphoreSlim.Release();
        }

        private void Append(string item, string outpputFile)
        {
            byte[] toAppend = File.ReadAllBytes(item);
            using (var fs = new FileStream(outpputFile, FileMode.Append))
            {
                fs.Write(toAppend, 0, toAppend.Length);
            }
            File.Delete(item);
        }
    }
}
