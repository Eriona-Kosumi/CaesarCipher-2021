using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaesarCipher
{
    class Program
    {
        static void Main(string[] args)
        {
            string plaintext, key;
            Console.WriteLine("Shenoni plaintextin");
            plaintext = Console.ReadLine();

            Console.WriteLine("Shenoni celesin");
            key = Console.ReadLine();

            string ciphertext = Encrypt(plaintext, int.Parse(key));
            Console.WriteLine("Ciphertexti i fituar: " + ciphertext);

            string decrypttext = Decrypt(ciphertext, int.Parse(key));
            Console.WriteLine("Teksti i dekriptuar: " + decrypttext);

            Console.ReadKey();
        }

        static string Encrypt(string plaintext, int key)
        {
            StringBuilder sbCiphertext = new StringBuilder(plaintext);
            for(int i=0; i < plaintext.Length; i++)
            {
                char ch = plaintext[i];

                int pozitach = ch - 'A';
                pozitach = (pozitach + key)%26;

                ch = (char)(pozitach + 'A');

                sbCiphertext[i] = ch;
            }

            return sbCiphertext.ToString();
        }

        static string Decrypt(string ciphertext, int key)
        {
            StringBuilder sbDecrypttext = new StringBuilder(ciphertext);
            for (int i = 0; i < ciphertext.Length; i++)
            {
                char ch = ciphertext[i];

                int pozitach = ch - 'A';
                pozitach = (pozitach - key +26) % 26;

                ch = (char)(pozitach + 'A');

                sbDecrypttext[i] = ch;
            }

            return sbDecrypttext.ToString();
        }
    }

}
