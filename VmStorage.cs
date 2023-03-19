using System;
namespace VirtualMem
{
	public class VmStorage<TElement>
	{
        public String Filename { get; init; }
        public int Size { get; init; }
		private VmFile<TElement>? vmFile;
		private bool isOpened = false;
		private VmPage<TElement>[] inMemoryPages;


		public VmStorage(String filename = "pagefile.dat", int size = 15000, int numberOfPagesInMemory = 3, int pageSize = 512)
		{
			inMemoryPages = new VmPage<TElement>[numberOfPagesInMemory];
			Filename = filename;
			Size = size;
            vmFile = OpenOrCreateVmFile(pageSize);
            isOpened = true;

        }

		public int GetNumberOfPages() { return inMemoryPages.Length; }
		public int GetPageSize() { return vmFile.PageSize; }

		private VmFile<TElement> OpenOrCreateVmFile(int pageSize)
		{
            VmFile<TElement> vmf = new VmFile<TElement>(Filename, Size, pageSize);
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

            if (elementIndex + 1 > Size || elementIndex < 0)
			{
                throw new InvalidOperationException("ReadElement: Индекс вышел за пределы массива");
            }

			if (vmFile == null)
			{
				throw new InvalidOperationException("ReadElement: VmFile не инициализирован");
            }

			int elementPageIndex = GetPageIndexByElementIndex(elementIndex); 
			int inPageElementIndex = (int)(elementIndex - elementPageIndex * vmFile.GetElementsOnPage());

            int choosenPageIndexToLoad;
			bool pageNotFound = true;
			int pageLocalIndex = -1; 

			for (int i = 0; i < inMemoryPages.Length; i++)
			{				
				if (inMemoryPages[i] != null && inMemoryPages[i].PageIndex == elementPageIndex)
				{
					pageNotFound = false;
					pageLocalIndex = i;
					break;
                }
			}

			if (pageNotFound)
			{
                choosenPageIndexToLoad = FindNullPage(); 

                if (choosenPageIndexToLoad == -1)
				{
                    choosenPageIndexToLoad = FindNotModifiedPage();
				}

                if (choosenPageIndexToLoad == -1)
                {

                    choosenPageIndexToLoad = FindEarlierPage();
                }
                
				if (inMemoryPages[choosenPageIndexToLoad] != null && inMemoryPages[choosenPageIndexToLoad].isModified == true)
				{
					vmFile.WritePage(inMemoryPages[choosenPageIndexToLoad]);
				}
                var page = vmFile.ReadPage(elementPageIndex); 
                inMemoryPages[choosenPageIndexToLoad] = page;
				pageLocalIndex = choosenPageIndexToLoad;
			}

			
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

            if (elementIndex + 1 > Size || elementIndex < 0)
			{
				throw new InvalidOperationException("WriteElement: Индекс вышел за пределы массива");
			}

			if (vmFile == null)
			{
				throw new InvalidOperationException("WriteElement: VmFile не инициализирован");
			}

			int elementPageIndex = GetPageIndexByElementIndex(elementIndex);
			int inPageElementIndex = (int)(elementIndex - elementPageIndex * vmFile.GetElementsOnPage());

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
