using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerprogSzekvencialis.BL
{
    class SerialEncrypt : Encryption
    {
        public SerialEncrypt(string inputFile, string outputFile, CancellationToken cancellationToken, string key, ProcessType processType)
            : base(inputFile,outputFile,cancellationToken,key,processType)
        {
        }

        public override void Start()
        {
            InitializeKey(this.key);
            StartProcessing();
        }

        private void StartProcessing()
        {
            this.baseTime = DateTime.Now;
            using (var fi = File.OpenRead(this.inputFile))
            {
                long allBytes = fi.Length;
                long round = 0;
                int byteToProcessLengthDefault = 3145728*10;
                byte[] byteToProcess = new byte[byteToProcessLengthDefault]; //3MB*15
                int read = 0;
                while (((read = fi.Read(byteToProcess, 0, byteToProcess.Length)) > 0) && !cancellationToken.IsCancellationRequested) 
                {
                    MessageChangeMethod(round.ToString() + ". round process start");
                    if (read < byteToProcess.Length)
                    {
                        byte[] b = new byte[read];
                        for (int p = 0; p < read; p++)
                        {
                            b[p] = byteToProcess[p];
                        }
                        byteToProcess = b;
                    }
                    byte[] encrypted = null;
                    switch (this.processType)
                    {
                        case ProcessType.Encrypt:
                            encrypted = EncryptData(byteToProcess);
                            break;
                        case ProcessType.Decrypt:
                            encrypted = DecryptData(byteToProcess);
                            break;
                        default:
                            encrypted = EncryptData(byteToProcess);
                            break;
                    }
                    MessageChangeMethod(round.ToString() + ". round write to disk..");
                    WriteToDiskData(encrypted);
                    round++;
                    try
                    {
                        PercentageChangedMethod((double)((double)round * (double)byteToProcessLengthDefault) / ((double)allBytes / 100.0));
                    }
                    catch(Exception)
                    {

                    }
                }
                MessageChangeMethod(round.ToString() + ". round process done");
                MessageChangeMethod("Complete.");
                PercentageChangedMethod(100);
            }

            
        }

        

        private void WriteToDiskData(byte[] encrypted)
        {
            using (var os = new FileStream(this.outputFile, FileMode.Append))
            {
                try
                {
                    os.Write(encrypted, 0, encrypted.Length);
                }
                catch(Exception e)
                {
                    MessageChangeMethod(e.Message);
                }
            }
        }
    }
}
