using System.Reflection;
using UUIDNext;

namespace uuidnextTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            for (int i = 0; i < 100; i++)
            {
                Guid newid = Uuid.NewSequential();
                Guid newid1 = Uuid.NewDatabaseFriendly(Database.SqlServer);
                var ids = newid.ToString().Split("-");
                var sn = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}{ids[2]}{ids[3]}";


                Console.WriteLine($"{i} => { newid } { newid1 } { sn }");

                
            }
            Console.ReadLine();
        }
    }
}