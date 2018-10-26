using Hessian.Lite.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dubbo.Config
{
    public class ServiceConfig
    {
        public const string DubboVersion = "2.0.0";
        public const string DubboApplication = "application";
        public const string DubboGroup = "group";
        public const string DubboSide = "side";
        public const string DubboCategory = "category";
        public const string DubboConsumer = "consumer";
        public const string DubboProvider = "provider";
        public const string DubboPath = "interface";
        public const string DubboMethod = "methods";
        public const string ServiceVersion = "version";


        protected IDictionary<string, string> Parameters;

        public string Application
        {
            get => Parameters.TryGetValue(DubboApplication, out var val) ? val : null;
            set => Parameters[DubboApplication] = value;
        }

        public string Group
        {
            get => Parameters.TryGetValue(DubboGroup, out var val) ? val : null;
            set => Parameters[DubboGroup] = value;
        }


        public string Side
        {
            get => Parameters.TryGetValue(DubboSide, out var val) ? val : null;
            set => Parameters[DubboSide] = value;
        }

        public string Category
        {
            get => Parameters.TryGetValue(DubboCategory, out var val) ? val : null;
            set => Parameters[DubboCategory] = value;
        }

        public string ServiceName
        {
            get => Parameters.TryGetValue(DubboPath, out var val) ? val : null;
            set => Parameters[DubboPath] = value;
        }


        public string[] Methods
        {
            get => Parameters.TryGetValue(DubboMethod, out var val) ? val.Split(',') : null;
            set => Parameters[DubboMethod] = string.Join(",", value);
        }

        public string Version
        {
            get => Parameters.TryGetValue(ServiceVersion, out var val) ? val : null;
            set => Parameters[ServiceVersion] = value;
        }

        //地址 ip或ip:port
        public string Address { get; set; }
        public string Protocol { get; set; }
        public bool Check { get; set; }

        public ServiceConfig() : this(new Dictionary<string, string>())
        {
        }

        public ServiceConfig(IDictionary<string, string> parameters)
        {
            Parameters = new Dictionary<string, string>(parameters);
        }

        public string ToServiceUrl()
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.AppendFormat("{0}://{1}/{2}", Protocol, Address, ServiceName);
            urlBuilder.AppendFormat("?dubbo={1}&timestamp={0}&check={2}", DateTime.Now.TimeStamp().ToString(),
                DubboVersion, Check.ToString());
            if (Parameters.Count <= 0)
                return urlBuilder.ToString();
            foreach (var kv in Parameters)
            {
                if (kv.Value.IsNullOrEmpty())
                {
                    continue;
                }

                urlBuilder.AppendFormat("&{0}={1}", kv.Key, kv.Value);
            }

            return urlBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is ServiceConfig other)
            {
                return Address == other.Address && Protocol == other.Protocol && Check == other.Check &&
                       Parameters.Count == other.Parameters.Count && Parameters.All(
                           kv => other.Parameters.TryGetValue(kv.Value, out var val) && kv.Value == val);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToServiceUrl().GetHashCode();
        }

        public override string ToString()
        {
            return ToServiceUrl();
        }

        public static ServiceConfig ParseServiceUrl(string serviceUrl)
        {
            if (serviceUrl.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            var serviceInfo = Regex.Match(serviceUrl,
                "(?<protocol>.+?)://(?<address>.+?)/(?<service>.+?)\\?(?<parameter>.+)", RegexOptions.Compiled);
            if (!serviceInfo.Success)
            {
                throw new Exception();
            }

            var parameters = serviceInfo.Groups["parameter"].Value.Split('&').Select(line => line.Split('='))
                .ToDictionary(kv => kv[0], kv => kv[1]);
            return new ServiceConfig(parameters)
            {
                Address = serviceInfo.Groups["address"].Value,
                Protocol = serviceInfo.Groups["protocol"].Value,
                ServiceName = serviceInfo.Groups["service"].Value
            };
        }
    }
}