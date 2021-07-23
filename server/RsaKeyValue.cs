using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Server
{
    internal class RsaKeyValue : KeyInfoClause
    {
        private RSACryptoServiceProvider objRsa;

        public RsaKeyValue(RSACryptoServiceProvider objRsa)
        {
            this.objRsa = objRsa;
        }

        public override XmlElement GetXml()
        {
            throw new System.NotImplementedException();
        }

        public override void LoadXml(XmlElement element)
        {
            throw new System.NotImplementedException();
        }
    }
}