using System;
using System.IO;
using System.Reflection;
using System.Text;
using Materia.Rendering.Extensions;

namespace Materia.Graph.IO
{
    public class Reader
    {
        ArraySegment<byte> buffer;
        int position = 0;

        byte[] floatBuffer = new byte[4];
        byte[] doubleBuffer = new byte[8];

        public int Length { get; private set; } = 0;

        public int Position
        {
            get => position;
            set => position = value.Clamp(0, Length - 1);
        }

        public ArraySegment<byte> Buffer
        {
            get => buffer;
            set
            {
                buffer = value;
                position = 0;
                Length = buffer.Count;
            }
        }

        public bool HasFloat => position + 3 < Length;
        public bool HasDouble => position + 7 < Length;
        public bool HasShort => position + 1 < Length;
        public bool HasByte => position < Length;

        public Reader() { }

        public Reader(byte[] data)
        {
            buffer = new ArraySegment<byte>(data);
            position = 0;
            Length = buffer != null ? buffer.Count : 0;
        }

        public Reader(ArraySegment<byte> data)
        {
            buffer = data;
            position = 0;
            Length = buffer != null ? buffer.Count : 0;
        }

        public void Clear()
        {
            Length = 0;
            position = 0;
        }

        public bool CanRead(int len)
        {
            return position + (len - 1) < Length;
        }

        public byte[] ReadBytes(int len)
        {
            if (position + (len - 1) >= Length)
            {
                throw new EndOfStreamException("ReadBytes out of range");
            }

            byte[] outBytes = new byte[len];
            Array.Copy(buffer.Array, buffer.Offset + position, outBytes, 0, len);
            position += len;
            return outBytes;
        }

        public ArraySegment<byte> ReadSegment(int len)
        {
            if (position + (len -1) >= Length)
            {
                throw new EndOfStreamException("ReadSegment out of range");
            }

            var segment = new ArraySegment<byte>(buffer.Array, buffer.Offset + position, len);
            position += len;
            return segment;
        }

        public Guid NextGuid()
        {
            if (!HasFloat)
            {
                throw new EndOfStreamException("NextGuid out of range");
            }

            int len = NextInt();
            byte[] data = ReadBytes(len);
            return new Guid(data);
        }

        public ArraySegment<byte> NextSegment()
        {
            if (!HasFloat)
            {
                throw new EndOfStreamException("NextSegment out of range");
            }

            int len = NextInt();
            return ReadSegment(len);
        }

        public float NextFloat()
        {
            if (!HasFloat)
            {
                throw new EndOfStreamException("NextFloat out of range");
            }

            Array.Copy(buffer.Array, buffer.Offset + position, floatBuffer, 0, floatBuffer.Length);
            position += floatBuffer.Length;

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(floatBuffer);
            }

            return floatBuffer.ToSingle();
        }

        public double NextDouble()
        {
            if (!HasDouble)
            {
                throw new EndOfStreamException("NextDouble out of range");
            }

            Array.Copy(buffer.Array, buffer.Offset + position, doubleBuffer, 0, doubleBuffer.Length);
            position += doubleBuffer.Length;

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(doubleBuffer);
            }

            return BitConverter.ToDouble(doubleBuffer);
        }

        public uint NextUInt()
        {
            if (!HasFloat)
            {
                throw new EndOfStreamException("NextUInt out of range");
            }

            uint v = 0;
            v |= (uint)(buffer.Array[buffer.Offset + position] << 24);
            ++position;
            v |= (uint)(buffer.Array[buffer.Offset + position] << 16);
            ++position;
            v |= (uint)(buffer.Array[buffer.Offset + position] << 8);
            ++position;
            v |= buffer.Array[buffer.Offset + position];
            ++position;

            return v;
        }

        public ulong NextULong()
        {
            if (!HasDouble)
            {
                throw new EndOfStreamException("NextULong out of range");
            }

            ulong v = 0;
            v |= (ulong)(buffer.Array[buffer.Offset + position] << 56);
            ++position;
            v |= (ulong)(buffer.Array[buffer.Offset + position] << 48);
            ++position;
            v |= (ulong)(buffer.Array[buffer.Offset + position] << 40);
            ++position;
            v |= (ulong)(buffer.Array[buffer.Offset + position] << 32);
            ++position;
            v |= (ulong)(buffer.Array[buffer.Offset + position] << 24);
            ++position;
            v |= (ulong)(buffer.Array[buffer.Offset + position] << 16);
            ++position;
            v |= (ulong)(buffer.Array[buffer.Offset + position] << 8);
            ++position;
            v |= buffer.Array[buffer.Offset + position];
            ++position;

            return v;
        }

        public ushort NextUShort()
        {
            if (!HasShort)
            {
                throw new EndOfStreamException("NextUShort out of range");
            }

            ushort v = 0;
            v |= (ushort)(buffer.Array[buffer.Offset + position] << 8);
            ++position;
            v |= buffer.Array[buffer.Offset + position];
            ++position;

            return v;
        }

        public short NextShort() => (short)NextUShort();

        public long NextLong() => (long)NextULong();

        public int NextInt() => (int)NextUInt();

        public bool NextBool() => NextByte() > 0;

        public byte NextByte()
        {
            if (!HasByte)
            {
                throw new EndOfStreamException("NextByte out of range");
            }

            byte b = buffer.Array[buffer.Offset + position];
            ++position;
            return b;
        }

        public sbyte NextSByte() => (sbyte)NextByte();

        public string NextString()
        {
            if (!HasFloat)
            {
                throw new EndOfStreamException("NextString out of range cannot read length");
            }

            int len = NextInt();

            if (position + (len - 1) >= Length)
            {
                throw new EndOfStreamException("NextString out of range cannot read string");
            }

            string s = Encoding.UTF8.GetString(buffer.Array, buffer.Offset + position, len);
            position += len;

            return s;
        }

        public string[] NextStringList()
        {
            if (!HasFloat)
            {
                throw new EndOfStreamException("NextStringList out of range");
            }

            int len = NextInt();
            string[] data = new string[len];
            for (int i = 0; i < len; ++i)
            {
                data[i] = NextString();
            }
            return data;
        }

        public T[] NextList<T>() where T : struct
        {
            if (!HasFloat)
            {
                throw new EndOfStreamException("NextList out of range");
            }

            int len = NextInt();
            T[] data = new T[len];
            for (int i = 0; i < len; ++i)
            {
                T v = default;
                Next(ref v);
                data[i] = v;
            }
            return data;
        }

        public void Next<T>(ref T v) where T : struct
        {
            TypedReference tv = __makeref(v);
            var info = v.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < info.Length; ++i)
            {
                Type t = info[i].FieldType;
                if (t == typeof(bool))
                {
                    info[i].SetValueDirect(tv, NextByte() == 1);
                }
                else if (t == typeof(float))
                {
                    info[i].SetValueDirect(tv, NextFloat());      
                }
                else if (t == typeof(double))
                {
                    info[i].SetValueDirect(tv, NextDouble());
                }
                else if (t == typeof(string))
                {
                    info[i].SetValueDirect(tv, NextString());
                }
                else if (t == typeof(long))
                {
                    info[i].SetValueDirect(tv, NextLong());
                }
                else if (t == typeof(ulong))
                {
                    info[i].SetValueDirect(tv, NextULong());
                }
                else if (t == typeof(int))
                {
                    info[i].SetValueDirect(tv, NextInt());
                }
                else if (t == typeof(uint))
                {
                    info[i].SetValueDirect(tv, NextUInt());
                }
                else if (t == typeof(short))
                {
                    info[i].SetValueDirect(tv, NextShort());
                }
                else if (t == typeof(ushort))
                {
                    info[i].SetValueDirect(tv, NextUShort());
                }
                else if (t == typeof(byte))
                {
                    info[i].SetValueDirect(tv, NextByte());
                }
                else if (t == typeof(sbyte))
                {
                    info[i].SetValueDirect(tv, NextSByte());
                }
                else if (t.IsEnum)
                {
                    info[i].SetValueDirect(tv, NextInt());
                }
                else if(t.IsArray)
                {
                    MethodInfo method = typeof(Reader).GetMethod(nameof(Reader.NextList));
                    MethodInfo generic = method.MakeGenericMethod(t);
                    info[i].SetValueDirect(tv, generic.Invoke(this, null));
                }
            }
        }
    }
}
