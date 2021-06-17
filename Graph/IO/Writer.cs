using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Materia.Graph.IO
{
    public class Writer : IDisposable
    {
        MemoryStream buffer = new MemoryStream();

        public int Length => (int)buffer.Length;

        public ArraySegment<byte> Buffer
        {
            get
            {
                buffer?.Seek(0, SeekOrigin.Begin);
                ArraySegment<byte> result = new ArraySegment<byte>();
                buffer?.TryGetBuffer(out result);
                return result;
            }
        }

        public void Write(byte b)
        {
            buffer.WriteByte(b);
        }

        public void Write(byte[] data, int offset, int count)
        {
            buffer.Write(data, offset, count);
        }

        public void Write(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            Write(bytes.Length);
            buffer.Write(bytes, 0, bytes.Length);
        }

        public void Write(ArraySegment<byte> data)
        {
            buffer.Write(data.Array, data.Offset, data.Count);
        }

        public void Write(float v)
        {
            byte[] bytes = v.GetBytes();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            Write(bytes, 0, bytes.Length);
        }

        public void Write(double v)
        {
            byte[] bytes = BitConverter.GetBytes(v);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            Write(bytes, 0, bytes.Length);
        }

        public void Write(uint v)
        {
            Write((byte)(v >> 24));
            Write((byte)(v >> 16));
            Write((byte)(v >> 8));
            Write((byte)(v & 0xFF));
        }

        public void Write(ulong v)
        {
            Write((byte)(v >> 56));
            Write((byte)(v >> 48));
            Write((byte)(v >> 40));
            Write((byte)(v >> 32));
            Write((byte)(v >> 24));
            Write((byte)(v >> 16));
            Write((byte)(v >> 8));
            Write((byte)(v & 0xFF));
        }

        public void Write(ushort v)
        {
            Write((byte)(v >> 8));
            Write((byte)(v & 0xFF));
        }

        public void Write(int v) => Write((uint)v);

        public void Write(long v) => Write((ulong)v);

        public void Write(short v) => Write((ushort)v);

        public void Write(bool v) => Write((byte)(v ? 1 : 0));

        public void Write(string v)
        {
            if (string.IsNullOrEmpty(v))
            {
                Write(0);
                return;
            }

            byte[] d = Encoding.UTF8.GetBytes(v);
            Write(d.Length);
            Write(d, 0, d.Length);
        }

        public void WriteSegment(ArraySegment<byte> data)
        {
            Write(data.Count);
            Write(data);
        }

        public void WriteObjectList<T>(T[] data) where T : struct
        {
            int len = data.Length;
            Write(len);
            for(int i = 0; i < data.Length; ++i)
            {
                WriteObject(data[i]);
            }
        }

        public void WriteStringList(string[] data)
        {
            int len = data.Length;
            Write(len);
            for (int i = 0; i < data.Length; ++i)
            {
                Write(data[i]);
            }
        }

        public void WriteObject<T>(T data) where T : struct
        {
            var info = data.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < info.Length; ++i)
            {
                Type t = info[i].FieldType;
                object o = info[i].GetValue(data);
                if (t == typeof(bool))
                {
                    Write(Convert.ToBoolean(o));
                }
                else if (t == typeof(float))
                {
                    Write(Convert.ToSingle(o));
                }
                else if(t == typeof(double))
                {
                    Write(Convert.ToDouble(o));
                }
                else if(t == typeof(string))
                {
                    Write(Convert.ToString(o));
                }
                else if(t == typeof(long))
                {
                    Write(Convert.ToInt64(o));
                }
                else if(t == typeof(ulong))
                {
                    Write(Convert.ToUInt64(o));
                }
                else if(t == typeof(int))
                {
                    Write(Convert.ToInt32(o));
                }
                else if(t == typeof(uint))
                {
                    Write(Convert.ToUInt32(o));
                }
                else if(t == typeof(short))
                {
                    Write(Convert.ToInt16(o));
                }
                else if(t == typeof(ushort))
                {
                    Write(Convert.ToUInt16(o));
                }
                else if(t == typeof(sbyte) || t == typeof(byte))
                {
                    Write(Convert.ToByte(o));
                }
                else if(t.IsEnum)
                {
                    Write(Convert.ToInt32(o));
                }
                else if(t.IsArray)
                {
                    MethodInfo method = typeof(Writer).GetMethod(nameof(Writer.WriteObjectList));
                    MethodInfo genericMethod = method.MakeGenericMethod(t);
                    genericMethod.Invoke(this, new object[] { o });
                }
            }
        }

        public void Clear()
        {
            try
            {
                if (buffer != null)
                {
                    buffer.SetLength(0);
                    buffer.Position = 0;
                }
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                buffer?.SetLength(0);
                buffer?.Close();
                buffer = null;
            }
            catch { }
        }
    }
}
