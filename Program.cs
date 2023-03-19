using VirtualMem;

ConsoleKeyInfo keyInfo;
do
{
    Console.WriteLine("Использовать значения по умолчанию для работы программы? (y/n)");
    keyInfo = Console.ReadKey();
}
while (keyInfo.KeyChar != 'y' && keyInfo.KeyChar != 'n');
Console.WriteLine();

VmStorage<int> storage;


if (keyInfo.KeyChar == 'n')
{
    int storageSize, numberOfPages, sizeOfPage;
    String filename;
    do
    {
        Console.Write("Введите новую длину массива (>=10000): ");
        string? s = Console.ReadLine();
        int val;
        if (!String.IsNullOrEmpty(s) && Int32.TryParse(s, out val))
        {
            if (val >= 10000)
            {
                storageSize = val;
                break;
            }
        }
 
        Console.WriteLine("Введенное значение не удовлетворяет условиям");
    }
    while (true);

    do
    {

        Console.Write("Введите новое имя файла: ");
        string? s = Console.ReadLine();
        if (!String.IsNullOrEmpty(s))
        {
            filename = s;
            break;
        }

        Console.WriteLine("Введенное значение не удовлетворяет условиям");
    }
    while (true);

    do
    {
        Console.Write("Введите новое количество страниц: ");
        string? s = Console.ReadLine();
        int val;
        if (!String.IsNullOrEmpty(s) && Int32.TryParse(s, out val))
        {
            numberOfPages = val;
            break;
            
        }

        Console.WriteLine("Введенное значение не удовлетворяет условиям");
    }
    while (true);

    do
    {
        Console.Write("Введите новый размер страницы: ");
        string? s = Console.ReadLine();
        int val;
        if (!String.IsNullOrEmpty(s) && Int32.TryParse(s, out val))
        {
            sizeOfPage = val;
            break;

        }

        Console.WriteLine("Введенное значение не удовлетворяет условиям");
    }
    while (true);

    storage = new VmStorage<int>(filename,storageSize,numberOfPages, sizeOfPage);
}
else
{
storage = new VmStorage<int>();
}
Console.WriteLine("================================================================");
Console.WriteLine("Файл открыт/создан");
Console.WriteLine();

Console.WriteLine("================================================================");
Console.WriteLine("Используемые значения:");
Console.WriteLine($"  - Размер хранилища = {storage.Size}");
Console.WriteLine($"  - Название файла = {storage.Filename}");
Console.WriteLine($"  - Количество страниц = {storage.GetNumberOfPages()}");
Console.WriteLine($"  - Pазмер страницы = {storage.GetPageSize()}");


bool isExit = false;

try
{
    do
    {
        Console.WriteLine();
        Console.WriteLine("================================================================");
        Console.WriteLine("Операция записи в выбранный элемент");
        Console.WriteLine("-----------");

        long elementIndex;
        do
        {
            long? value = ConsoleHelper.ReadLong("Введите индекс элемента (Enter - завершение работы): ", null);
            if (value == null)
            {
                isExit = true;
                throw new Exception();
            }

            if (value >= 0 && value < storage.Size)
            {
                elementIndex = value.Value;
                break;
            }

            Console.WriteLine("Введенное значение не удовлетворяет условиям");
        }
        while (true);

        int elementValue;
        do
        {
            long? value = ConsoleHelper.ReadLong("Введите значение элемента (Enter - завершение работы): ", null);
            if (value == null)
            {
                isExit = true;
                throw new Exception();
            }

            if (value >= int.MinValue && value <= int.MaxValue)
            {
                elementValue = (int)value.Value;
                break;
            }

            Console.WriteLine("Введенное значение не удовлетворяет условиям");
        }
        while (true);

        int prevValue = storage.ReadElement(elementIndex);
        Console.WriteLine();
        Console.WriteLine($"Значение до записи: {prevValue}");

        storage.WriteElement(elementIndex, elementValue);
        int actualValue = storage.ReadElement(elementIndex);
        Console.WriteLine($"Значение после считывания: {actualValue}");
        Console.WriteLine();

    } while (true);

}
catch (Exception e)
{
    if (!isExit)
    {
        Console.WriteLine(e.ToString());
    }
}
storage.Close();

Console.WriteLine();
Console.WriteLine("Работа программы завершена");
