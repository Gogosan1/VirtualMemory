using System;
namespace VirtualMem
{
	public class VmFile<TElement>
	{
		private readonly String Filename;
		private readonly int NumberOfElements;
		public int PageSize { get; init; }
		private const String signatureValue = "VM";
		private FileStream? vmFileStream;
		private BinaryReader? vmFileReader;
		private BinaryWriter? vmFileWriter;

		public VmFile(String filename, int numberOfElements, int PageSize = 512)
		{
			this.Filename = filename;
			this.NumberOfElements = numberOfElements;
			this.PageSize = PageSize;
		}

		public void OpenOrCreate()
		{
			if (File.Exists(Filename))
			{
				CheckSignature(Filename);
				CheckLength(Filename);

				InitStream(Filename, FileMode.Open);
				return;
			}

			if (!File.Exists(Filename))
			{
				InitStream(Filename, FileMode.Create);

				WriteSignature(vmFileWriter);

				int pagesNumber = GetPagesNumber();
				for (int i = 0; i < pagesNumber; i++)
				{
					WriteEmptyPage(vmFileWriter, GetElementsOnPage());
				}

				vmFileStream?.Seek(0, SeekOrigin.Begin);
			}
		}

		private void InitStream(String filename, FileMode mode)
		{
			vmFileStream = new FileStream(filename, mode, FileAccess.ReadWrite);
			vmFileStream.Seek(0, SeekOrigin.Begin);

			vmFileReader = new BinaryReader(vmFileStream);
			vmFileWriter = new BinaryWriter(vmFileStream);
		}

		public int GetElementsOnPage()
		{
			int elementSize = VmHelper.GetElementSize<TElement>();
			int elementsOnPage = (int)Math.Floor((decimal)PageSize / elementSize);
			return elementsOnPage;
		}

		public int GetPagesNumber()
		{
			int numberOfPages = (int)Math.Ceiling((decimal)NumberOfElements / GetElementsOnPage());
			return numberOfPages;
		}

		private void CheckSignature(String filename)
		{
			FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
			stream.Seek(0, SeekOrigin.Begin);
			BinaryReader reader = new BinaryReader(stream);

			char[] signature = new char[2];

			try
			{
				signature = reader.ReadChars(2);
				stream.Close();
			}
			catch (Exception)
			{
				throw new FormatException("Не удалось считать признак 'VM' в указанном файле");
			}

			if (new string(signature) != VmFile<TElement>.signatureValue)
			{
				throw new FormatException("При попытке считать признак 'VM' обнаружена другая последовательность");
			}
		}

		private void CheckLength(String filename)
		{
			FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
			long streamLength = stream.Length;
			stream.Close();

			long necessaryLength = CalculateVmFileLength();
			if (streamLength != necessaryLength)
			{
				throw new FormatException($"Длина файла ({streamLength} байт) не совпадает с необходимой длиной ({necessaryLength} байт)");
			}
		}

		private void WriteSignature(BinaryWriter? fileWriter)
		{
			fileWriter?.Write(VmFile<TElement>.signatureValue.ToCharArray());
		}

		private void WriteEmptyPage(BinaryWriter? fileWriter, int maxElementsOnPage)
		{
			long pageLength = GetPageLength();

			for (int i = 0; i < pageLength; i++)
			{
				fileWriter?.Write((byte)0);
			}
		}

		private long CalculateVmFileLength()
		{
			int pageLength = GetPageLength();
			return VmFile<TElement>.signatureValue.Length + pageLength * GetPagesNumber();
		}

		private int GetPageLength()
		{
			return GetBitMapLength() + PageSize;
		}

		private int GetBitMapLength()
		{
			return (int)Math.Ceiling(GetElementsOnPage() / 8d);
		}

		public VmPage<TElement> ReadPage(int pageIndex)
		{
			if (vmFileStream == null || vmFileReader == null)
			{
				throw new InvalidOperationException("ReadPage: Файл подкачки не открыт");
			}

			try
			{
				long offset = GetPageOffset(pageIndex);
				try
				{
					vmFileStream.Seek(offset, SeekOrigin.Begin);
				}
				catch (Exception)
				{
					throw new IOException("ReadPage: IO-Ошибка работы с файлом подкачки");
				}

				bool[] bitmap = ReadBitmap(vmFileReader, GetBitMapLength());
				TElement[] elements = ReadElements(vmFileReader, GetElementsOnPage());

                return VmPage<TElement>.create(pageIndex, bitmap, elements);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"ReadPage: Ошибка при считывании страницы #{pageIndex} (нумерация с 0)", e);
			}
		}

		private bool[] ReadBitmap(BinaryReader reader, int bitmapLength)
		{
			const int BYTE_SIZE = 8;
			bool[] buf = new bool[bitmapLength * BYTE_SIZE];

			try
			{
				for (int i = 0; i < bitmapLength; i++)
				{
					byte v = reader.ReadByte();
					for (int p = 0; p < BYTE_SIZE; p++)
					{
						byte bitmask = (byte)Math.Pow(2, p);
						buf[i * BYTE_SIZE + p] = ((v & bitmask) == 0) ? false : true;
					}
				}
			}
			catch (Exception)
			{
				throw new IOException("ReadBitmap: Ошибка считывания битовой карты при считывании страницы");
			}
			return buf;
		}

		private TElement[] ReadElements(BinaryReader reader, int maxElementsOnPage)
		{
			TElement[] elements = new TElement[maxElementsOnPage];
			int elementSizeBytes = VmHelper.GetElementSize<TElement>();
			byte[] elementBytes = new byte[elementSizeBytes];

			try
			{
                for (int i = 0; i < maxElementsOnPage; i++)
                {
                    elementBytes = reader.ReadBytes(elementSizeBytes);
                    elements[i] = VmHelper.DeserializeBytesToNumber<TElement>(elementBytes);
                }
            }
			catch (Exception)
			{
                throw new IOException("ReadElements: Ошибка считывания элементов при считывании страницы");
            }
			return elements;
		}

		private long GetPageOffset(int pageIndex)
		{
			return VmFile<TElement>.signatureValue.Length + pageIndex * GetPageLength();
        }

		public void WritePage(VmPage<TElement> vmPage)
		{
            if (vmFileStream == null || vmFileWriter == null)
            {
                throw new InvalidOperationException("WritePage: Файл подкачки не открыт");
            }

            try
            {
                long offset = GetPageOffset(vmPage.PageIndex);
                try
                {
                    vmFileStream.Seek(offset, SeekOrigin.Begin);
                }
                catch (Exception e)
                {
                    throw new IOException("WritePage: IO-Ошибка работы с файлом подкачки", e);
                }

                WriteBitmap(vmFileWriter, GetBitMapLength(), vmPage.Bitmap);
                WriteElements(vmFileWriter, GetElementsOnPage(), vmPage.Elements, vmPage.Bitmap);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"WritePage: Ошибка при записи страницы #{vmPage.PageIndex} (нумерация с 0)", e);
            }
        }

		private void WriteBitmap(BinaryWriter writer, int bitmapLength, bool[]? bitmap)
		{
			const int BYTE_SIZE = 8;
			byte[] buf = new byte[bitmapLength];

			try
			{
                
                int pos = 0;
				for (int i = 0; i < bitmapLength; i++)
				{
					byte v = 0;
					
					for (int p = 0; p < BYTE_SIZE; p++)
					{
						pos = i * BYTE_SIZE + p;
						if (bitmap?[pos] == true)
						{
							v = (byte)(v | (byte)Math.Pow(2, p));
						}
					}

					buf[i] = v;
				}
			}
			catch (Exception)
			{
				throw new InvalidOperationException("WriteBitmap: Ошибка при формировании битовой маски");
            }

            try
            {
				writer.Write(buf);
			}
			catch (Exception)
			{
				throw new IOException("WriteBitmap: ");
			}
		}

		private void WriteElements(BinaryWriter writer, int maxElementsOnPage, TElement[]? elements, bool[] bitmap)
		{
            int elementSizeBytes = VmHelper.GetElementSize<TElement>();

            for (int i = 0; i < maxElementsOnPage; i++)
			{
				if (elements == null || i + 1 > elements.Length || elements[i] == null || bitmap[i] == false)
				{
					for (int p = 0; p < elementSizeBytes; p++)
					{
						writer.Write((byte)0);
					}
				} else
				{
					byte[] buf = VmHelper.SerializeNumberToBytes<TElement>(elements[i]);
					writer.Write(buf);
                }
			}
		}

		public void Close()
		{
			vmFileStream?.Close();
		}
	}
}

