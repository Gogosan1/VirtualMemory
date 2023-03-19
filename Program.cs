using VirtualMem;

Console.WriteLine($"Длина массива {VmStorage.DefaultSize} элементов, Название файла '{VmStorage.DefaultFilename}'");

ConsoleKeyInfo keyInfo;
do
{
    Console.WriteLine("Использовать значения по умолчанию для работы программы? (y/n)");
    keyInfo = Console.ReadKey();
}
while (keyInfo.KeyChar != 'y' && keyInfo.KeyChar != 'n');
Console.WriteLine();

int storageSize = VmStorage.DefaultSize;
String filename = VmStorage.DefaultFilename;
if (keyInfo.KeyChar == 'n')
{
    do
    {
        long? value = ConsoleHelper.ReadLong("Введите новую длину массива (>=10000) (Enter - использовать значение по умолчанию): ", VmStorage.DefaultSize);
        storageSize = (int)(value ?? 0);
        if (storageSize >= 10000) break;

        Console.WriteLine("Введенное значение не удовлетворяет условиям");
    }
    while (true);

    do
    {
        filename = ConsoleHelper.ReadString("Введите новое имя файла (Enter - использовать значение по умолчанию): ", VmStorage.DefaultFilename);
        if (!String.IsNullOrEmpty(filename)) break;

        Console.WriteLine("Введенное значение не удовлетворяет условиям");
    }
    while (true);
}

Console.WriteLine();
Console.WriteLine("================================================================");
Console.WriteLine("Используемые значения:");
Console.WriteLine($"  - Размер массива = {storageSize}");
Console.WriteLine($"  - Название файла = {filename}");
Console.WriteLine();



var storage = new VmStorage<int>(filename, storageSize);
Console.WriteLine("Файл открыт/создан");

Console.WriteLine();
Console.WriteLine();

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

            if (value >= 0 && value < storageSize)
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
