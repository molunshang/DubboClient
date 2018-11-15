using Dubbo.Remote;
using Dubbo.Utils;
using Hessian.Lite;
using Hessian.Lite.Exception;
using Hessian.Lite.IO;
using Hessian.Lite.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dubbo
{
    public class Codec
    {
        const int HeaderLength = 16;
        const ushort Magic = 0xdabb;
        const byte MagicFirst = 218;
        const byte MagicSecond = 187;
        const byte RequestFlag = 128;
        const byte TwowayFlag = 64;
        const byte EventFlag = 32;
        const int HessianSerialize = 2;


        public static void EncodeRequest(Request request, Stream outputStream)
        {
            var header = new byte[HeaderLength];
            header.WriteUShort(Magic);
            header[2] = RequestFlag | HessianSerialize;
            if (request.IsEvent)
            {
                header[2] |= EventFlag;
            }

            if (request.IsTwoWay)
            {
                header[2] |= TwowayFlag;
            }

            header.WriteLong(request.RequestId, 4);
            using (var dataStream = new PoolMemoryStream())
            {
                var output = new Hessian2Writer(dataStream);
                if (request.IsEvent)
                {
                    output.WriteNull();
                }
                else
                {
                    output.WriteString("2.0.0");
                    output.WriteObject(request.Service);
                    output.WriteObject(request.Version);
                    output.WriteObject(request.MethodName);
                    output.WriteString(request.ParameterTypeInfo);
                    if (request.Arguments != null && request.Arguments.Length > 0)
                    {
                        foreach (var arg in request.Arguments)
                        {
                            output.WriteObject(arg);
                        }
                    }
                    output.WriteObject(request.Attachments);
                }
                header.WriteInt((int)dataStream.Length, 12);
                outputStream.Write(header, 0, header.Length);
                dataStream.CopyTo(outputStream);
            }
        }

        public static Response DecodeResponse(Stream inputStream)
        {
            var resHeader = new byte[16];
            inputStream.ReadBytes(resHeader);

            if ((resHeader[2] & RequestFlag) != 0)
            {
                throw new ArgumentException("decode response fail. the stream is not response.");
            }

            var response = new Response
            {
                ResponseId = resHeader.ReadLong(4),
                Status = resHeader[3],
                IsEvent = (resHeader[2] & EventFlag) != 0,
                IsTwoWay = (resHeader[2] & TwowayFlag) != 0
            };
            var request = RequestTasks.GetRequestTask(response.ResponseId);
            var bodyLength = resHeader.ReadInt(12);
            var body = new byte[bodyLength];
            inputStream.ReadBytes(body);
            var reader = new Hessian2Reader(new MemoryStream(body));
            if (response.IsOk)
            {
                if (response.IsEvent)
                {
                    response.Result = reader.ReadObject();
                }
                else
                {
                    var resultType = request?.Request.ReturnType;
                    var flag = (byte)reader.ReadInt();
                    switch (flag)
                    {
                        case Response.Null:
                            break;
                        case Response.Value:
                            response.Result = reader.ReadObject(resultType);
                            break;
                        case Response.Exception:
                            response.Error = reader.ReadObject<JavaException>();
                            break;
                        case Response.NullWithAttachment:
                            response.Attachments = reader.ReadObject<Dictionary<string, string>>();
                            break;
                        case Response.ValueWithAttachment:
                            response.Result = reader.ReadObject(resultType);
                            response.Attachments = reader.ReadObject<Dictionary<string, string>>();
                            break;
                        case Response.ExceptionWithAttachment:
                            response.Error = reader.ReadObject<JavaException>();
                            response.Attachments = reader.ReadObject<Dictionary<string, string>>();
                            break;
                        default:
                            throw new IOException("Unknown result flag, expect '0' '1' '2', get " + flag);
                    }
                }
            }
            else
            {
                response.ErrorMessage = reader.ReadString();
                response.Error = new Exception(response.ErrorMessage);
            }

            request?.Task.TrySetResult(response);
            return response;
        }
    }
}