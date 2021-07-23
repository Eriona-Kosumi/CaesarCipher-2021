using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

using System.Security.Cryptography.Xml;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System.Data;
using System.Windows.Forms;

namespace Server
{
    class Program
    {
        
        
        public static XmlDocument objXml = new XmlDocument();


        public static XmlElement rootNode;
                                              
        public static XmlElement shfrytezuesiNode;
        public static XmlElement personiNode;
        public static XmlElement llojifaturesNode;
        public static XmlElement vitiNode;
        public static XmlElement vleraeuroNode;
        public static XmlElement muajiNode;
        public static XmlElement usernameNode;
        public static XmlElement saltedHashNode;
        public static XmlElement saltNode;



        public static DESCryptoServiceProvider objDes;
        public static RSACryptoServiceProvider objRsa;
        public static RSACryptoServiceProvider objRsaForSign;
        public static string strCelesiCipher = "";
        public static Socket serverSocket;
        public static byte[] desKey = new byte[8];
        public static byte[] desIV = new byte[8];

        

        public static SHA1CryptoServiceProvider objHash = new SHA1CryptoServiceProvider();
        public static SHA1Managed objHashManaged = new SHA1Managed();

        static void Main(string[] args)
        {

            
            createServerSocket();
            


            while (true)
            {
                Socket connSocket = serverSocket.Accept();
                Thread th = new Thread(() => handleConnection(ref connSocket));
                th.Start();
            }

        }

        private static void createServerSocket()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 12000);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(ip);
            serverSocket.Listen(10);
            Console.WriteLine("Serveri eshte duke pritur per lidhje ...");
        }

        private static void handleConnection(ref Socket connSocket)
        {
            IPEndPoint clientIp = (IPEndPoint)connSocket.RemoteEndPoint;
            Console.WriteLine("Serveri u lidh me hostin {0} ne portin {1}", clientIp.Address, clientIp.Port);


            
            connSocket.Receive(desKey);
            connSocket.Receive(desIV);
            if(objDes==null)
                createDes();




            while (true)
            {
                byte[] byteFromClient = new byte[1024];
                int length = connSocket.Receive(byteFromClient);
                string strFromClient = Encoding.UTF8.GetString(byteFromClient, 0, length);


                Console.WriteLine("Client: (Ciphertext) {0} \n", strFromClient);
                string fromClientDecrypted = dekriptoDes(strFromClient);
                Console.WriteLine("Client: (Plaintext) {0} \n", fromClientDecrypted);

                string[] tokens = fromClientDecrypted.Split(' ');
                Console.WriteLine(tokens.Length);

                string strFromServer = "";
                if (tokens.Length == 2)
                {
                    string username = tokens[0].Trim();
                    string password = tokens[1].Trim();

                    if (isValidLogin(username, password))
                    {
                        strFromServer = "You are logged in!";
                        DataSet dsShfrytezuesi = new DataSet();
                        Shfrytezuesi shfrytezuesi = getShfrytezuesi(username);
                        string jsonWebToken = generateJWT(shfrytezuesi.personi, shfrytezuesi.llojifatures,shfrytezuesi.viti,shfrytezuesi.vleraeuro,shfrytezuesi.muaji,shfrytezuesi.username);
                       

              
                        
                    }
                    else
                    {
                        strFromServer = "false";
                        
                    }

                }
                else
                {
                    string personi = tokens[0];
                    string llojifatures = tokens[1];
                    string muaji = tokens[2];
                    string viti = tokens[3];
                    double vleraeuro = Double.Parse(tokens[4]);
                    string username = tokens[5].Trim();
                    string password = tokens[6].Trim();
                    //string saltedHash = tokens[7];

                    try
                    {
                        addShfrytezuesi(personi, llojifatures, viti, vleraeuro, muaji, username, password);
                        strFromServer = "true";
                        //Console.WriteLine(strFromServer);
                    }
                    catch (Exception ex)
                    {
                        strFromServer = "User has not been added!! \n" + ex.Message;
                        //Console.WriteLine(strFromServer);
                    }
                }

                
                //
                // Serveri kthen diqka ...
                //


                Console.WriteLine("Server: (Plaintext) {0} \n", strFromServer);
                string strFromServerEncrypted = enkriptoDes(strFromServer);
                connSocket.Send(Encoding.UTF8.GetBytes(strFromServerEncrypted));
                Console.WriteLine("Server: (Ciphertext) {0} \n", strFromServerEncrypted);
            }
        }

        private static void createDes()
        {
            objDes = new DESCryptoServiceProvider();
            objDes.IV = desIV;
            objDes.Padding = PaddingMode.Zeros;
            objDes.Mode = CipherMode.CBC;
            objDes.Key = desKey;
        }

        private static string enkriptoDes(string strFromServer)
        {
            byte[] byteFromServer = Encoding.UTF8.GetBytes(strFromServer);

            MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, objDes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(byteFromServer, 0, byteFromServer.Length);
            cs.Close();

            byte[] byteFromServerEncrypted = ms.ToArray();
            string strFromServerEncrypted = Convert.ToBase64String(byteFromServerEncrypted);
            return strFromServerEncrypted;
        }

        private static string dekriptoDes(string strFromClient)
        {
            byte[] byteFromClient = Convert.FromBase64String(strFromClient);

            MemoryStream ms = new MemoryStream(byteFromClient);
            CryptoStream cs = new CryptoStream(ms, objDes.CreateDecryptor(), CryptoStreamMode.Read);
            byte[] byteFromClientDecrypted = new byte[ms.Length];
            cs.Read(byteFromClientDecrypted, 0, byteFromClientDecrypted.Length);
            cs.Close();
            ms.Close();

            string strFromClientDecrypted = Encoding.UTF8.GetString(byteFromClientDecrypted);
            return strFromClientDecrypted;

        }

        private static void createRsa()
        {
            objRsa = new RSACryptoServiceProvider();
            

            File.WriteAllText("public-key.xml", objRsa.ToXmlString(false));
            File.WriteAllText("private-key.xml", objRsa.ToXmlString(true));

            // Duhet me ja jep celsin publik
        }

        public static void addShfrytezuesi(string personi, string llojifatures, string viti, double vleraeuro,string muaji,string username,string password)
        {
            if (File.Exists("shfrytezuesi.xml") == false)
            {
                XmlTextWriter xmlTw = new XmlTextWriter("shfrytezuesi.xml", Encoding.UTF8);
                xmlTw.WriteStartElement("shfrytezuesi");
                xmlTw.Close();

            }

            objXml.Load("shfrytezuesi.xml");

            rootNode = objXml.DocumentElement;

            shfrytezuesiNode = objXml.CreateElement("shfrytezuesi");
            personiNode = objXml.CreateElement("personi");
            llojifaturesNode = objXml.CreateElement("llojifatures");
            vitiNode = objXml.CreateElement("viti");
            vleraeuroNode = objXml.CreateElement("vleraeuro");
            muajiNode = objXml.CreateElement("muaji");
            usernameNode = objXml.CreateElement("username");
            saltedHashNode = objXml.CreateElement("saltedHashPassword");
            saltNode = objXml.CreateElement("salt");


            personiNode.InnerText = personi;
            llojifaturesNode.InnerText = llojifatures;
            vitiNode.InnerText = viti;
            vleraeuroNode.InnerText = vleraeuro+"";
            muajiNode.InnerText = muaji;
            usernameNode.InnerText = username;
            Random random = new Random(DateTime.Now.Millisecond);
            string salt = random.Next(100000, 1000000).ToString();
            saltNode.InnerText = salt;
            saltedHashNode.InnerText = getSaltedHash(salt,password);


            shfrytezuesiNode.AppendChild(personiNode);
            shfrytezuesiNode.AppendChild(llojifaturesNode);
            shfrytezuesiNode.AppendChild(vitiNode);
            shfrytezuesiNode.AppendChild(vleraeuroNode);
            shfrytezuesiNode.AppendChild(muajiNode);
            shfrytezuesiNode.AppendChild(usernameNode);
            shfrytezuesiNode.AppendChild(saltNode);
            shfrytezuesiNode.AppendChild(saltedHashNode);


            rootNode.AppendChild(shfrytezuesiNode);

            objXml.Save("shfrytezuesi.xml");
        }

        private static string getSaltedHash(string salt, string password)
        {
            string saltedPassword = password+salt;
             

            byte[] byteSaltedPassword = Encoding.UTF8.GetBytes(saltedPassword);
            byte[] byteSaltedHash = objHashManaged.ComputeHash(byteSaltedPassword);

            return Convert.ToBase64String(byteSaltedHash);
            
        }

        public static bool isValidLogin(string username,string password)
        {
            objXml.Load("shfrytezuesi.xml");

            XmlNodeList shfrytezuesiElements = objXml.GetElementsByTagName("shfrytezuesi");


            for (int i = 0; i < shfrytezuesiElements.Count; i++)
            {
                string usernameXml = shfrytezuesiElements[i].SelectSingleNode("username").InnerText;
                string saltedHashXml = shfrytezuesiElements[i].SelectSingleNode("saltedHashPassword").InnerText;
                string saltXml = shfrytezuesiElements[i].SelectSingleNode("salt").InnerText;



                string saltedPasswordLogin = saltXml + password;


                string saltedHashLogin = getSaltedHash(saltXml, password);

                Console.WriteLine("=================================");
                Console.WriteLine("Username(xml): " + usernameXml);
                Console.WriteLine("SaltedHash(xml): " + saltedHashXml);

                Console.WriteLine("Username(login): " + username);
                Console.WriteLine("SaltedHash(login): " + saltedHashLogin);
                Console.WriteLine("====================================");

                if (usernameXml.Equals(username) && saltedHashXml.Equals(saltedHashLogin))
                {
                    return true;
                }
            }

            return false; 
        }

        private static Shfrytezuesi getShfrytezuesi(string username)
        {
            objXml.Load("shfrytezuesi" +".xml");
            XmlNodeList shfrytezuesiElements = objXml.GetElementsByTagName("shfrytezuesi");

            Shfrytezuesi shfrytezuesi = new Shfrytezuesi();

            for(int i = 0; i < shfrytezuesiElements.Count; i++)
            {
                string usernameXml = shfrytezuesiElements[i].SelectSingleNode("username").InnerText;
                string personiXml = shfrytezuesiElements[i].SelectSingleNode("personi").InnerText;
                string llojifaturesXml = shfrytezuesiElements[i].SelectSingleNode("llojifatures").InnerText;
                string vitiXml = shfrytezuesiElements[i].SelectSingleNode("viti").InnerText;
                double vleraeuroXml = Double.Parse(shfrytezuesiElements[i].SelectSingleNode("vleraeuro").InnerText);
                string muajiXml = shfrytezuesiElements[i].SelectSingleNode("muaji").InnerText;

                if (username.Equals(usernameXml))
                {
                    shfrytezuesi = new Shfrytezuesi(personiXml, llojifaturesXml, vitiXml, vleraeuroXml, muajiXml, usernameXml);
                }
            }



            return shfrytezuesi;
        }

        private static byte[] decryptKey()
        {
            byte[] byteCelesiDekriptuar = objRsa.Decrypt(desKey,true);

            return byteCelesiDekriptuar;
        }




        

        private static string generateJWT(string personi,string llojifatures,string viti,double vleraeuro,string muaji,string username)
        {
            var payload = new Dictionary<string, object>
                {
                    { "personi", personi },
                    { "llojifatures", llojifatures },
                    {"viti", viti},
                    {"vleraeuro",vleraeuro },
                    {"muaji",muaji },
                    {"username",username }
                };
            const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);
            Console.WriteLine("JsonWebToken: "+token+"\n");

            //rtResult.Text = token;
            return token;
        }
    }
}
