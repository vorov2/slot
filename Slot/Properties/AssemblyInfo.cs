using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Slot.Core;

// Управление общими сведениями о сборке осуществляется с помощью 
// набора атрибутов. Измените значения этих атрибутов, чтобы изменить сведения,
// связанные со сборкой.
[assembly: AssemblyTitle("Slot")]
[assembly: AssemblyDescription(App.Description)]
[assembly: AssemblyConfiguration(App.Configuration)]
[assembly: AssemblyCompany(App.Company)]
[assembly: AssemblyProduct(App.Product)]
[assembly: AssemblyCopyright(App.Copyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Параметр ComVisible со значением FALSE делает типы в сборке невидимыми 
// для COM-компонентов.  Если требуется обратиться к типу в этой сборке через 
// COM, задайте атрибуту ComVisible значение TRUE для этого типа.
[assembly: ComVisible(false)]

// Следующий GUID служит для идентификации библиотеки типов, если этот проект будет видимым для COM
[assembly: Guid("3a814cee-1a58-46d0-97e7-dcd93a86f726")]

// Сведения о версии сборки состоят из следующих четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии 
//   Номер сборки
//      Редакция
//
// Можно задать все значения или принять номера сборки и редакции по умолчанию 
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(App.Version)]
[assembly: AssemblyFileVersion(App.Version)]
[assembly: AssemblyCommit(App.Commit)]