﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Dubbo.Remote
{
    public class Request
    {
        private static long _requestId;
        private string _service;
        private string _version;
        private IDictionary<string, string> _attachments;
        public Request()
        {
            RequestId = Interlocked.Increment(ref _requestId);
        }

        public long RequestId { get; }
        public bool IsTwoWay { get; set; }
        public bool IsEvent { get; set; }
        public string Service
        {
            get => _service;
            set
            {
                _service = value;
                Attachments["path"] = Attachments["interface"] = value;
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                Attachments["version"] = value;
            }
        }

        public string MethodName { get; set; }
        public string ParameterTypeInfo { get; set; }
        public object[] Arguments { get; set; }

        public IDictionary<string, string> Attachments
        {
            get => _attachments ?? (_attachments = new Dictionary<string, string>());
            set
            {
                if (value == null)
                {
                    return;
                }
                _attachments = value;
            }
        }

        public Type ReturnType { get; set; }
    }
}