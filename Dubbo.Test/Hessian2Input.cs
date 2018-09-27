using hessiancsharp.io;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Dubbo.Test
{
    class Hessian2Input : CHessianInput
    {
        private Type superType = typeof(CHessianInput);
        public Hessian2Input(Stream srInput) : base(srInput)
        {
        }

        public override string ReadString()
        {
            int tag = Read();
            var str = new StringBuilder();

            switch (tag)
            {
                case 'N':
                    return null;
                case 'T':
                    return "true";
                case 'F':
                    return "false";

                // direct integer
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:

                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:

                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:

                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    return (tag - 0x90).ToString();

                /* byte int */
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7:
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf:
                    return (((tag - 0xc8) << 8) + Read()).ToString();

                /* short int */
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:
                    return (((tag - 0xd4) << 16)
                            + 256 * Read() + Read()).ToString();

                case 'I':
                case 0x59:

                    return ParseInt().ToString();

                // direct long
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf:

                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                    return (tag - 0xe0).ToString();

                /* byte long */
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:
                    return (((tag - 0xf8) << 8) + Read()).ToString();

                /* short long */
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                    return (((tag - 0x3c) << 16)
                            + 256 * Read() + Read()).ToString();

                case 'L':
                    return ParseLong().ToString();

                case 0x5b:
                    return "0.0";

                case 0x5c:
                    return "1.0";

                case 0x5d:
                    return Read().ToString();

                case 0x5e:
                    return ((short)(256 * Read() + Read())).ToString();

                case 0x5f:
                    {
                        int mills = ParseInt();

                        return (0.001 * mills).ToString();
                    }

                case 'D':
                    var method = superType.GetMethod("ParseDouble", BindingFlags.Instance | BindingFlags.NonPublic);
                    return method.Invoke(this, new object[0]).ToString();

                case 'S':
                case 'R':
                    SetChunkInfo(tag == 'S', (Read() << 8) + Read());
                    str.Clear();
                    int ch;

                    while ((ch = ParseChar()) >= 0)
                        str.Append((char)ch);

                    return str.ToString();

                // 0-byte string
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x0e:
                case 0x0f:

                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                    SetChunkInfo(true, tag);
                    str.Clear();
                    while ((ch = ParseChar()) >= 0)
                        str.Append((char)ch);
                    return str.ToString();
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                    SetChunkInfo(true, (tag - 0x30) * 256 + Read());
                    str.Clear();
                    while ((ch = ParseChar()) >= 0)
                        str.Append((char)ch);

                    return str.ToString();

                default:
                    throw new Exception();
            }
        }

        public void SetChunkInfo(bool flag, int length)
        {
            var isLast = superType.GetField("m_blnIsLastChunk", BindingFlags.Instance | BindingFlags.NonPublic);
            isLast.SetValue(this, flag);
            var chunkLength = superType.GetField("m_intChunkLength", BindingFlags.Instance | BindingFlags.NonPublic);
            chunkLength.SetValue(this, length);
        }

        public int ParseChar()
        {
            var parseChar = superType.GetMethod("ParseChar", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)parseChar.Invoke(this, new object[0]);
        }

        public int ParseInt()
        {
            var parseChar = superType.GetMethod("ParseInt", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)parseChar.Invoke(this, new object[0]);
        }

        public long ParseLong()
        {
            var parseChar = superType.GetMethod("ParseLong", BindingFlags.Instance | BindingFlags.NonPublic);
            return (long)parseChar.Invoke(this, new object[0]);
        }

        public int Read()
        {
            var method = superType.GetMethod("Read", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
            return (int)method.Invoke(this, new object[0]);
        }

    }
}
