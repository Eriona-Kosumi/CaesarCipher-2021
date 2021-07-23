using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Shfrytezuesi
    {
        public string personi { get; set; }
        public string llojifatures { get; set; }
        public string viti { get; set; }
        public double vleraeuro { get; set; }
        public string muaji { get; set; }
        public string username { get; set; }

        public Shfrytezuesi(string personi,string llojifatures,string viti,double vleraeuro,string muaji,string username)
        {
            this.personi = personi;
            this.llojifatures = llojifatures;
            this.viti = viti;
            this.vleraeuro = vleraeuro;
            this.muaji = muaji;
            this.username = username;
        }

        public Shfrytezuesi()
        {

        }

    }
}
