using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace XHD.Core.Common
{
    public class XHDData<T>
    {
        public int code { get; set; } = 0;

        public string msg { get; set; }

        public List<T> data { get; set; }

        public long count { get; set; } = 0;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        }
    }
}
