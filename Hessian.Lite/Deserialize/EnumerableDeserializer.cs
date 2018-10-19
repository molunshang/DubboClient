using Hessian.Lite.Exception;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Hessian.Lite.Deserialize
{
    public class EnumerableDeserializer : AbstractDeserializer
    {
        private static readonly Type SelfType = typeof(EnumerableDeserializer);
        private static readonly MethodInfo BaseMethod = SelfType.GetMethod("Read", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly Func<Hessian2Reader, int, IEnumerable> _converter;

        public EnumerableDeserializer(Type type)
        {
            Type = type;
            var methodInfo = type.IsGenericType ? BaseMethod.MakeGenericMethod(type.GenericTypeArguments) : BaseMethod.MakeGenericMethod(typeof(object));
            var argExp = new[] { Expression.Parameter(typeof(Hessian2Reader), "reader"), Expression.Parameter(typeof(int), "length") };
            var callExp = Expression.Call(methodInfo, argExp);
            _converter = Expression.Lambda<Func<Hessian2Reader, int, IEnumerable>>(callExp, argExp).Compile();

        }

        private static IEnumerable<T> Read<T>(Hessian2Reader reader, int length)
        {
            var list = new List<T>();
            reader.AddRef(list);
            if (length >= 0)
            {
                for (int i = 0; i < length; i++)
                {
                    list.Add(reader.ReadObject<T>());
                }
            }
            else
            {
                while (!reader.HasEnd())
                {
                    list.Add(reader.ReadObject<T>());
                }
                reader.ReadToEnd();
            }
            return list;
        }

        public override object ReadObject(Hessian2Reader reader)
        {
            var tag = reader.ReadListStart();
            switch (tag)
            {
                case Constants.Null:
                    return null;
                case Constants.VariableList:
                    return ReadList(reader, -1);
                case Constants.VariableUnTypeList:
                    return ReadList(reader, -1);
                case Constants.FixedList:
                    reader.ReadType();
                    return ReadList(reader, reader.ReadInt());
                case Constants.FixedUnTypeList:
                    return ReadList(reader, reader.ReadInt());
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77:
                    reader.ReadType();
                    return ReadList(reader, tag - 0x70);
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7e:
                case 0x7f:
                    return ReadList(reader, tag - 0x78);
                default:
                    throw new HessianException($"unknown code {(char)tag} where read type {Type.FullName}");
            }
        }

        public override object ReadList(Hessian2Reader reader, int length)
        {
            return _converter(reader, length);
        }
    }
}