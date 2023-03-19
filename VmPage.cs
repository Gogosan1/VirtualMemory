using System;
namespace VirtualMem
{
	public class VmPage<TElement>
	{
		public int PageIndex { get; private set; }
		public bool[] Bitmap { get; private set; }
        public TElement[] Elements { get; private set; }
		public bool isModified { get; private set; }
		public DateTime loadedTime { get; private set; }

        private VmPage()
		{
			Elements = new TElement[0];
			Bitmap = new bool[0];
		}

		public static VmPage<TElement> create(int pageIndex, bool[] bitmap, TElement[] elements)
		{
			VmPage<TElement> page = new VmPage<TElement>();
			page.PageIndex = pageIndex;
			page.Bitmap = bitmap;
			page.Elements = elements;
			page.isModified = false;
			page.loadedTime = DateTime.Now;
			return page;
        }

        public TElement ReadElement(int elementLocalIndex)
		{
			return Elements[elementLocalIndex];
		}

		public void WriteElement(int elementLocalIndex, TElement element)
		{
			Elements[elementLocalIndex] = element;
			Bitmap[elementLocalIndex] = true;
			isModified = true;
		}
    }
}

