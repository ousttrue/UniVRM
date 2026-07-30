using System;
using System.Linq;


namespace UniJSON
{
    public struct Utf8String : IComparable<Utf8String>
    {
        public static readonly System.Text.Encoding Encoding = new System.Text.UTF8Encoding(false);

        public readonly ReadOnlyMemory<byte> Bytes;

        public int ByteLength => Bytes.Length;

        public Utf8Iterator GetIterator()
        {
            return new Utf8Iterator(Bytes);
        }

        public int CompareTo(Utf8String other)
        {
            int i = 0;
            for (; i < ByteLength && i < other.ByteLength; ++i)
            {
                if (this[i] < other[i])
                {
                    return 1;
                }
                else if (this[i] > other[i])
                {
                    return -1;
                }
            }
            if (i < ByteLength)
            {
                return -1;
            }
            else if (i < other.ByteLength)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public Byte this[int i]
        {
            get { return Bytes.Span[i]; }
        }

        public Utf8String(ReadOnlyMemory<Byte> bytes)
        {
            Bytes = bytes;
        }

        public Utf8String(ArraySegment<Byte> bytes)
        {
            Bytes = bytes;
        }

        public Utf8String(Byte[] bytes, int offset, int count)
        {
            Bytes = bytes.AsMemory(offset,count);
        }

        public Utf8String(Byte[] bytes)
        {
            Bytes = bytes;
        }

        public static Utf8String From(string src)
        {
            return new Utf8String(Encoding.GetBytes(src));
        }

        public static Utf8String From(string src, Byte[] bytes)
        {
            var required = src.Sum(c => Utf8Iterator.ByteLengthFromChar(c));
            if (required > bytes.Length)
            {
                throw new OverflowException();
            }
            int pos = 0;
            foreach (var c in src)
            {
                if (c <= Utf8Iterator.Mask7)
                {
                    // 1bit
                    bytes[pos++] = (byte)c;
                }
                else if (c <= Utf8Iterator.Mask11)
                {
                    // 2bit
                    bytes[pos++] = (byte)(Utf8Iterator.Head2 | Utf8Iterator.Mask5 & (c >> 6));
                    bytes[pos++] = (byte)(Utf8Iterator.Head1 | Utf8Iterator.Mask6 & (c));
                }
                else
                {
                    // 3bit
                    bytes[pos++] = (byte)(Utf8Iterator.Head3 | Utf8Iterator.Mask4 & (c >> 12));
                    bytes[pos++] = (byte)(Utf8Iterator.Head1 | Utf8Iterator.Mask6 & (c >> 6));
                    bytes[pos++] = (byte)(Utf8Iterator.Head1 | Utf8Iterator.Mask6 & (c));
                }
            }
            return new Utf8String(bytes, 0, pos);
        }

        public Utf8String Concat(Utf8String rhs)
        {
            var bytes = new Byte[ByteLength + rhs.ByteLength];
            Bytes.Span.CopyTo(bytes);
            rhs.Bytes.Span.CopyTo(bytes.AsSpan(ByteLength));
            return new Utf8String(bytes);
        }

        public ReadOnlySpan<byte> AsSpan() => Bytes.Span;

        public override string ToString()
        {
            if (ByteLength == 0) return "";
            return Encoding.GetString(Bytes.Span);
        }

        public string ToAscii()
        {
            if (ByteLength == 0) return "";
            return System.Text.Encoding.ASCII.GetString(Bytes.Span);
        }

        public bool IsEmpty
        {
            get
            {
                return ByteLength == 0;
            }
        }

        public bool StartsWith(Utf8String rhs)
        {
            return Bytes.Span.StartsWith(rhs.Bytes.Span);
        }

        public bool EndsWith(Utf8String rhs)
        {
            return Bytes.Span.EndsWith(rhs.Bytes.Span);
        }

        public int IndexOf(Byte code)
        {
            return IndexOf(0, code);
        }

        public int IndexOf(int offset, Byte code)
        {
            var span = Bytes.Span;
            for (int i = offset; i < span.Length; ++i)
            {
                if (span[i] == code)
                {
                    return i;
                }
            }
            return -1;
        }

        public Utf8String Subbytes(int offset)
        {
            return Subbytes(offset, ByteLength - offset);
        }

        public Utf8String Subbytes(int offset, int count)
        {
            return new Utf8String(Bytes.Slice(offset, count));
        }

        static bool IsSpace(Byte b)
        {
            switch (b)
            {
                case 0x20:
                case 0x0a:
                case 0x0b:
                case 0x0c:
                case 0x0d:
                case 0x09:
                    return true;
            }

            return false;
        }

        public Utf8String TrimStart()
        {
            var span = Bytes.Span;
            var i = 0;
            for (; i < span.Length; ++i)
            {
                if (!IsSpace(span[i]))
                {
                    break;
                }
            }
            return Subbytes(i);
        }

        public Utf8String TrimEnd()
        {
            var span = Bytes.Span;
            var i = span.Length - 1;
            for (; i >= 0; --i)
            {
                if (!IsSpace(span[i]))
                {
                    break;
                }
            }
            return Subbytes(0, i+1);
        }

        public Utf8String Trim()
        {
            return TrimStart().TrimEnd();
        }

        public override bool Equals(Object obj)
        {
            return obj is Utf8String && Equals((Utf8String)obj);
        }

        public static bool operator ==(Utf8String x, Utf8String y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Utf8String x, Utf8String y)
        {
            return !(x == y);
        }

        public bool Equals(Utf8String other)
        {
            return Bytes.Span.SequenceEqual(other.Bytes.Span);
        }

        public override int GetHashCode()
        {
            return ByteLength.GetHashCode();
        }

        public static Utf8String operator +(Utf8String l, Utf8String r)
        {
            return l.Concat(r);
        }

        public bool IsInt
        {
            get
            {
                //bool isInt = false;
                var span = Bytes.Span;
                for (int i = 0; i < span.Length; ++i)
                {
                    var c = span[i];
                    if (c == '0'
                        || c == '1'
                        || c == '2'
                        || c == '3'
                        || c == '4'
                        || c == '5'
                        || c == '6'
                        || c == '7'
                        || c == '8'
                        || c == '9'
                        )
                    {
                        // ok
                        //isInt = true;
                    }
                    else if (i == 0 && c == '-')
                    {
                        // ok
                    }
                    else if (c == '.' || c == 'e')
                    {
                        return false;
                    }
                    else
                    {
                        break;
                    }
                }
                return true;
            }
        }
    }
}