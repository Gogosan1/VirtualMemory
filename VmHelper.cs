using System;
namespace VirtualMem
{
	public static class VmHelper
	{
		public static int GetElementSize<TElement>()
		{
            if (typeof(TElement) == typeof(byte)) return sizeof(byte);
            if (typeof(TElement) == typeof(short)) return sizeof(short);
            if (typeof(TElement) == typeof(int)) return sizeof(int);
            if (typeof(TElement) == typeof(long)) return sizeof(long);
            throw new InvalidOperationException($"DeserializeBytesToNumber: Не поддерживаемый тип {typeof(TElement).Name}");
        }

        public static TElement DeserializeBytesToNumber<TElement>(byte[] bytes)
        {
            if (typeof(TElement) == typeof(byte))
            {
                return (TElement)Convert.ChangeType(bytes[0], typeof(TElement));
            }
            if (typeof(TElement) == typeof(short))
            {                
                short v = BitConverter.ToInt16(bytes, 0);
                return (TElement)Convert.ChangeType(v, typeof(TElement));
            }
            if (typeof(TElement) == typeof(int))
            {
                int v = BitConverter.ToInt32(bytes, 0);
                return (TElement)Convert.ChangeType(v, typeof(TElement));
            }
            if (typeof(TElement) == typeof(long))
            {
                long v = BitConverter.ToInt64(bytes, 0);
                return (TElement)Convert.ChangeType(v, typeof(TElement));
            }
            throw new InvalidOperationException($"DeserializeBytesToNumber: Не поддерживаемый тип {typeof(TElement).Name}");
        }

        public static byte[] SerializeNumberToBytes<TElement>(TElement element)
        {
            byte[] buf = new byte[GetElementSize<TElement>()];
            if (element == null)
            {                
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = 0;
                }
                return buf;
            }
            if (typeof(TElement) == typeof(byte))
            {
                buf[0] = (byte)Convert.ChangeType(element, typeof(byte));
                return buf;
            }
            if (typeof(TElement) == typeof(short))
            {
                short v = (short)Convert.ChangeType(element, typeof(short));
                return BitConverter.GetBytes(v);
            }
            if (typeof(TElement) == typeof(int))
            {
                int v = (int)Convert.ChangeType(element, typeof(int));
                return BitConverter.GetBytes(v);
            }
            if (typeof(TElement) == typeof(long))
            {
                long v = (long)Convert.ChangeType(element, typeof(long));
                return BitConverter.GetBytes(v);
            }
            throw new InvalidOperationException($"SerializeNumberToBytes: Не поддерживаемый тип {typeof(TElement).Name}");
        }
    }
}

