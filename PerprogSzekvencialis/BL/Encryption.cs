using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PerprogSzekvencialis.BL
{
    public enum ProcessType { Encrypt, Decrypt };
    public class Encryption
    {
        public event ProcessHandler PercentageChange;
        public event ProcessHandlerMessage MessageChange;
        public delegate void ProcessHandler(double percent);
        public delegate void ProcessHandlerMessage(string message);

        protected string inputFile;
        protected string outputFile;
        protected CancellationToken cancellationToken;
        protected byte[] key;
        protected ProcessType processType;
        protected KeyMatrix keyMatrix;
        protected Random random;

        protected DateTime baseTime;

        public Encryption(string inputFile, string outputFile, CancellationToken cancellationToken, string key, ProcessType processType)
        {
            random = new Random();
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.cancellationToken = cancellationToken;
            this.processType = processType;
            this.key = Encoding.UTF8.GetBytes(key);
        }

        public virtual void Start() { }

        protected byte[] EncryptData(byte[] byteToProcess)
        {
            for (int i = 0; i < this.key.Length; i++)
            {
                byteToProcess = EncryptData2(byteToProcess);
            }
            return byteToProcess;
        }
        protected byte[] EncryptData(byte[] byteToProcess, List<byte[]> keys)
        {
            foreach (var item in keys)
            {
                byteToProcess = EncryptData2(byteToProcess, item);
            }
            return byteToProcess;
        }
        protected byte[] DecryptData(byte[] byteToProcess, List<byte[]> keys)
        {
            foreach (var item in keys)
            {
                byteToProcess = DecryptData2(byteToProcess, item);
            }
            return byteToProcess;
        }
        protected byte[] DecryptData(byte[] byteToProcess)
        {
            for (int i = 0; i < this.key.Length; i++)
            {
                byteToProcess = DecryptData2(byteToProcess);
            }
            return byteToProcess;
        }
        protected byte[] EncryptData2(byte[] byteToProcess)
        {
            byte[] actualKey = this.keyMatrix.GetNextRow();
            int maxIndexForKey = actualKey.Length - 1;
            if (byteToProcess.Length < actualKey.Length)
            {
                maxIndexForKey = byteToProcess.Length - 1;
            }

            byte[] reBytes = new byte[byteToProcess.Length];

            int actualIndexForKey = 0;
            for (int i = 0; i < byteToProcess.Length; i++)
            {
                byte currentKey = actualKey[actualIndexForKey];
                int sh = byteToProcess[i] + currentKey;
                if (sh <= 255)
                {
                    reBytes[i] = (byte)sh;
                }
                else
                {
                    sh = sh - 256;
                    reBytes[i] = (byte)sh;
                }

                actualIndexForKey++;
                if (actualIndexForKey > maxIndexForKey)
                {
                    actualIndexForKey = 0;
                }
            }

            return reBytes;

        }
        protected byte[] EncryptData2(byte[] byteToProcess, byte[] key)
        {
            byte[] actualKey = key;
            int maxIndexForKey = actualKey.Length - 1;
            if (byteToProcess.Length < actualKey.Length)
            {
                maxIndexForKey = byteToProcess.Length - 1;
            }

            byte[] reBytes = new byte[byteToProcess.Length];

            int actualIndexForKey = 0;
            for (int i = 0; i < byteToProcess.Length; i++)
            {
                byte currentKey = actualKey[actualIndexForKey];
                int sh = byteToProcess[i] + currentKey;
                if (sh <= 255)
                {
                    reBytes[i] = (byte)sh;
                }
                else
                {
                    sh = sh - 256;
                    reBytes[i] = (byte)sh;
                }

                actualIndexForKey++;
                if (actualIndexForKey > maxIndexForKey)
                {
                    actualIndexForKey = 0;
                }
            }

            return reBytes;

        }
        protected byte[] DecryptData2(byte[] byteToProcess)
        {
            byte[] actualKey = this.keyMatrix.GetNextRow();
            int maxIndexForKey = actualKey.Length - 1;
            if (byteToProcess.Length < actualKey.Length)
            {
                maxIndexForKey = byteToProcess.Length - 1;
            }

            byte[] reBytes = new byte[byteToProcess.Length];

            int actualIndexForKey = 0;
            for (int i = 0; i < byteToProcess.Length; i++)
            {
                byte currentKey = actualKey[actualIndexForKey];
                int sh = byteToProcess[i] - currentKey;
                if (sh >= 0)
                {
                    reBytes[i] = (byte)sh;
                }
                else
                {
                    sh = sh + 256;
                    reBytes[i] = (byte)sh;
                }

                actualIndexForKey++;
                if (actualIndexForKey > maxIndexForKey)
                {
                    actualIndexForKey = 0;
                }
            }

            return reBytes;

        }
        protected byte[] DecryptData2(byte[] byteToProcess, byte[] key)
        {
            byte[] actualKey = key;
            int maxIndexForKey = actualKey.Length - 1;
            if (byteToProcess.Length < actualKey.Length)
            {
                maxIndexForKey = byteToProcess.Length - 1;
            }

            byte[] reBytes = new byte[byteToProcess.Length];

            int actualIndexForKey = 0;
            for (int i = 0; i < byteToProcess.Length; i++)
            {
                byte currentKey = actualKey[actualIndexForKey];
                int sh = byteToProcess[i] - currentKey;
                if (sh >= 0)
                {
                    reBytes[i] = (byte)sh;
                }
                else
                {
                    sh = sh + 256;
                    reBytes[i] = (byte)sh;
                }

                actualIndexForKey++;
                if (actualIndexForKey > maxIndexForKey)
                {
                    actualIndexForKey = 0;
                }
            }

            return reBytes;

        }
        protected void InitializeKey(byte[] key)
        {
            this.keyMatrix = new KeyMatrix(key);
        }

        protected void PercentageChangedMethod(double percent)
        {
            PercentageChange(percent);
        }
        protected void MessageChangeMethod(string message)
        {
            var dif = DateTime.Now - this.baseTime;
            MessageChange(dif.ToString(@"hh\:mm\:ss") + "        " + message);
        }
    }
}
