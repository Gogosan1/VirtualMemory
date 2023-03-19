using System;
namespace VirtualMem
{
	public class VmStorage<TElement>
	{
        private String filename;
        private int size;
		private VmFile<TElement>? vmFile;
		private bool isOpened = false;
		private VmPage<TElement>[] inMemoryPages = new VmPage<TElement>[3];

        public VmStorage()
		{			
			Create(VmStorage.DefaultFilename, VmStorage.DefaultSize);
		}

		public VmStorage(String filename, int? size)
		{			
			Create(filename, size);
		}

		private void Create(String? filename, int? size)
		{
			if (!string.IsNullOrEmpty(filename))
			{
				this.filename = filename ?? this.filename;
			}
			if (size != null)
			{
				this.size = size.Value;
			}
			vmFile = OpenOrCreateVmFile();
			isOpened = true;
		}

		private VmFile<TElement> OpenOrCreateVmFile()
		{
            VmFile<TElement> vmf = new VmFile<TElement>(filename, size);
            vmf.OpenOrCreate();
			return vmf;
        }

		public void Flush() // сбрасывает все загруженные страницы на диск
		{
			CheckIfAlreadyClosed();

			for (int i = 0; i < inMemoryPages.Length; i++)
			{
				if (inMemoryPages[i] == null) continue;
				vmFile?.WritePage(inMemoryPages[i]);
			}
		}

		public void Close()
		{
            CheckIfAlreadyClosed();
			Flush();
			vmFile?.Close();

			isOpened = false;
        }

		private void CheckIfAlreadyClosed()
		{
			if (!isOpened)
			{
				throw new InvalidOperationException("Ошибка: VmStorage уже закрыт, поэтому выполнение операции невозможно");
			}
		}

		public TElement ReadElement(long elementIndex)
		{
			CheckIfAlreadyClosed();

            if (elementIndex + 1 > size || elementIndex < 0)
			{
                throw new InvalidOperationException("ReadElement: Индекс вышел за пределы массива");
            }

			if (vmFile == null)
			{
				throw new InvalidOperationException("ReadElement: VmFile не инициализирован");
            }

			int elementPageIndex = GetPageIndexByElementIndex(elementIndex); // индекс искомой страницы
			int inPageElementIndex = (int)(elementIndex - elementPageIndex * vmFile.GetElementsOnPage());

            int choosenPageIndexToLoad;
			bool pageNotFound = true;
			int pageLocalIndex = -1; // индекс найденной страницы, которую надо использовать

			// ищем страницу с таким индексом - может быть, она уже загружена
			for (int i = 0; i < inMemoryPages.Length; i++)
			{				
				if (inMemoryPages[i] != null && inMemoryPages[i].PageIndex == elementPageIndex)
				{
					// нашли страницу
					pageNotFound = false;
					pageLocalIndex = i;
					break;
                }
			}

			if (pageNotFound)
			{
                // искомая страница не найдена среди имеющихся уже в памяти
                choosenPageIndexToLoad = FindNullPage(); // ищем пустой слот

                if (choosenPageIndexToLoad == -1) // значит пустого слота не было
				{
                    choosenPageIndexToLoad = FindNotModifiedPage(); // ищем не модифицированную
				}

                if (choosenPageIndexToLoad == -1) // значит все страницы были модифицированы
                {
					// ищем самую раннюю, которая раньше всех была загружена в память
                    choosenPageIndexToLoad = FindEarlierPage();
                }
                
				if (inMemoryPages[choosenPageIndexToLoad] != null && inMemoryPages[choosenPageIndexToLoad].isModified == true)
				{
					// страница была модифицирована, следовательно сохраняем на диск перед замещением
					vmFile.WritePage(inMemoryPages[choosenPageIndexToLoad]);
				}
                var page = vmFile.ReadPage(elementPageIndex); // загружаем с диска искомую страницу
                inMemoryPages[choosenPageIndexToLoad] = page; // операция замещения (теряется страница, которая была ранее в этом индексе)
				pageLocalIndex = choosenPageIndexToLoad;
			}

			// искомая страница загружена в память
			// индекс элемента на странице известен
			
			TElement element = inMemoryPages[pageLocalIndex].ReadElement(inPageElementIndex);
            return element;
		}

		private int FindEarlierPage()
		{
			DateTime dtMin = DateTime.MaxValue;
			int index = -1;
            for (int i = 0; i < inMemoryPages.Length; i++)
			{
				if (inMemoryPages[i] != null && inMemoryPages[i].loadedTime < dtMin)
				{
					index = i;
					dtMin = inMemoryPages[i].loadedTime;
                }
			}
			return index;
        }

        private int FindNullPage()
        {
            for (int i = 0; i < inMemoryPages.Length; i++)
            {
                if (inMemoryPages[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindNotModifiedPage()
		{
			for (int i = 0; i < inMemoryPages.Length; i++)
			{
				if (inMemoryPages[i] != null && inMemoryPages[i].isModified == false)
				{
					return i;
				}
			}
			return -1;
		}


        public void WriteElement(long elementIndex, TElement element)
		{
			CheckIfAlreadyClosed();

            if (elementIndex + 1 > size || elementIndex < 0)
			{
				throw new InvalidOperationException("WriteElement: Индекс вышел за пределы массива");
			}

			if (vmFile == null)
			{
				throw new InvalidOperationException("WriteElement: VmFile не инициализирован");
			}

			int elementPageIndex = GetPageIndexByElementIndex(elementIndex);
			int inPageElementIndex = (int)(elementIndex - elementPageIndex * vmFile.GetElementsOnPage());

			// считываем элемент, чтобы гарантировать, что искомая страница
			// оказалась в памяти
			ReadElement(elementIndex);

            int pageLocalIndex = -1;

            for (int i = 0; i < inMemoryPages.Length; i++)
			{
				if (inMemoryPages[i] != null && inMemoryPages[i].PageIndex == elementPageIndex)
				{
					pageLocalIndex = i;
				}
			}

			if (pageLocalIndex == -1)
			{
				throw new InvalidOperationException("WriteElement: Страница не найдена после считывания элемента");
            }

			inMemoryPages[pageLocalIndex].WriteElement(inPageElementIndex, element);
		}

		private int GetPageIndexByElementIndex(long elementIndex)
		{
			if (vmFile == null) return -1;

            return (int)Math.Floor((decimal)elementIndex / vmFile.GetElementsOnPage());
        }
    }
}

public static class VmStorage
{
    public const String DefaultFilename = "pagefile.dat";
    public const int DefaultSize = 15000;
}