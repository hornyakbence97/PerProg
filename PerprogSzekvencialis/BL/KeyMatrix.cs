using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerprogSzekvencialis.BL
{
    public class KeyMatrix
    {
        private byte[,] key;
        private int lastRow;

        public KeyMatrix(byte[] key)
        {
            this.lastRow = -1;
            CreateKey(key);
        }

        public KeyMatrix(string key)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(key);
            CreateKey(byteKey);
            
        }
        private void CreateKey(byte[] key)
        {
            int length = key.Length;
            this.key = new byte[length, length];

            //minden elemhez hozzáadom a következő elemet majd modulo 255, kivéve az utolsónál
            int maxI = this.key.GetLength(0);
            int maxJ = this.key.GetLength(1);
            for (int i = 0; i < maxI; i++)
            {
                for (int j = 0; j < maxJ; j++)
                {
                    int biggerI = i + 1;
                    int biggerJ = j + 1;
                    
                    if (biggerJ < maxJ)
                    {
                        this.key[i, j] = (byte)((key[j] + key[biggerJ])%255);
                    }
                    else
                    {
                        this.key[i, j] = (byte)((key[j] + key[j-2]) % 255);
                    }
                    if (i > 0)
                    {
                        this.key[i, j] = (byte)((this.key[i-1,j] + key[j] + this.key[i,j])%255);
                    }
                }
            }
        }

        public byte[] GetNextRow()
        {
            this.lastRow++;
            if (this.lastRow >= this.key.GetLength(0))
            {
                this.lastRow = 0;
            }
            byte[] output = new byte[key.GetLength(0)];

            for (int i = 0; i < this.key.GetLength(0); i++)
            {
                output[i] = this.key[this.lastRow, i];
            }

            return output;
        }
    }
}
