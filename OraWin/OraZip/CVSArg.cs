using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace OraZip
{
    class CVSArg
    {
        public CVSArg( string Product, int ProductId, int BaseVersion, int LastVersion, int PackVersion)
        {
            this.Product = Product;
            this.ProductId = ProductId;
            this.BaseVersion = BaseVersion;
            this.LastVersion = LastVersion;
            this.PackVersion = PackVersion;
        }

        public string Product { get; private set; } // 06版 融资融券 UF2.0
        public int ProductId { get; private set; } // 1-06版 2-融资融券 UF2.0
        public int BaseVersion { get; private set; } // V6.1.4 的 4 基线 也就是 BL2011
        public int LastVersion { get; private set; } // SP1 的 1
        public int PackVersion { get; private set; } // PACK4 的 4
        public int BuildVersion { get; private set; } // 暂时不用

        public string Sversion {
            get
            {
                return ProductId.ToString() + "." + BaseVersion.ToString() + "." +
                    LastVersion.ToString() + "." + PackVersion.ToString(); 
            }
        }
    }
}
