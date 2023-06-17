using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace lab2
{
    public enum Availability
    {
        InStock,
        RunningOut,
        OutOfStock
    }

    interface INameAndCopy
    {
        string Name { get; set; }
        object DeepCopy();
    }
    public class Product
    {
        public int Quantity { get; set; }
        public Product(int code, Availability availability)
        {
            Code = code;
            Availability = availability;
            InputName();
            InputManufacturingDate();
        }

        public Product(string name, int code, Availability availability, DateTime manufacturingDate)
        {
            Name = name;
            Code = code;
            Availability = availability;
            ManufacturingDate = manufacturingDate;
        }

        public Product()
        {
            // Default values
            Name = "Default Product";
            Code = 0;
            Availability = Availability.OutOfStock;
            ManufacturingDate = DateTime.MinValue;
        }

        public virtual object DeepCopy()
        {
            return new Product(Name, Code, Availability, ManufacturingDate);
        }

        public string Name { get; set; }
        public int Code { get; set; }
        public Availability Availability { get; set; }
        public DateTime ManufacturingDate { get; set; }

        private void InputName()
        {
            var inputOk = false;
            while (!inputOk)
            {
                Console.WriteLine($"Enter the name of product {Code}:");
                var raw = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                Name = raw;
                inputOk = true;
            }
        }

        private void InputManufacturingDate()
        {
            DateTime manufacturingDate;
            var inputOk = false;
            while (!inputOk)
            {
                Console.WriteLine($"Enter the manufacturing date of product {Code} (dd.MM.yyyy):");
                var raw = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                if (!DateTime.TryParseExact(raw, "dd.MM.yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out manufacturingDate))
                    continue;

                ManufacturingDate = manufacturingDate;
                inputOk = true;
            }
        }

        public override string ToString()
        {
            return $"Name: {Name}, Code: {Code}, Availability: {Availability}, Manufacturing Date: {ManufacturingDate}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Product other = (Product)obj;
            return Name == other.Name &&
                   Code == other.Code &&
                   Availability == other.Availability &&
                   ManufacturingDate == other.ManufacturingDate;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 23 + Code.GetHashCode();
                hash = hash * 23 + Availability.GetHashCode();
                hash = hash * 23 + ManufacturingDate.GetHashCode();
                return hash;
            }
        }
    }

    public class Revision : INameAndCopy
    {
        private string revisionInfo;
        protected DateTime revisionDate;
        protected int revisionNumber;

        public string RevisionInfo
        {
            get { return revisionInfo; }
            set { revisionInfo = value; }
        }

        public DateTime RevisionDate
        {
            get { return revisionDate; }
            set { revisionDate = value; }
        }

        public int RevisionNumber
        {
            get { return revisionNumber; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("revisionNumber", "Номер ревізії не може бути від'ємним.");

                revisionNumber = value;
            }
        }

        public Revision(string revisionInfo, DateTime revisionDate, int revisionNumber)
        {
            RevisionInfo = revisionInfo;
            RevisionDate = revisionDate;
            RevisionNumber = revisionNumber;
        }

        public Revision()
        {
            revisionDate = DateTime.MinValue;
            revisionNumber = 0;
        }

        public override string ToString()
        {
            return $"Revision Info: {RevisionInfo}, Revision Date: {RevisionDate}, Revision Number: {RevisionNumber}";
        }

        public virtual bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Revision other = (Revision)obj;

            return RevisionInfo == other.RevisionInfo &&
                   RevisionDate == other.RevisionDate &&
                   RevisionNumber == other.RevisionNumber;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RevisionInfo, RevisionDate, RevisionNumber);
        }

        public static bool operator ==(Revision left, Revision right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Revision left, Revision right)
        {
            return !(left == right);
        }

        public virtual object DeepCopy()
        {
            return new Revision(RevisionInfo, RevisionDate, RevisionNumber);
        }

        public string Name
        {
            get { return RevisionInfo; }
            set { RevisionInfo = value; }
        }
    }


    public class WarehouseRevision : Revision, INameAndCopy
    {
        private List<Person> responsiblePersons;
        private List<Product> products;

        public string RevisionName { get; set; }

        public WarehouseRevision(string revisionInfo, DateTime revisionDate, int revisionNumber, List<Person> responsiblePersons, params Product[] products)
            : base(revisionInfo, revisionDate, revisionNumber)
        {
            ResponsiblePersons = responsiblePersons;
            Products = new List<Product>(products);
        }

        public WarehouseRevision()
        {
            RevisionInfo = "Default revision";
            RevisionDate = DateTime.UtcNow;
            RevisionNumber = 0;
            ResponsiblePersons = new List<Person>();
            Products = new List<Product>();
        }

        public new object DeepCopy()
        {
            WarehouseRevision copy = (WarehouseRevision)base.DeepCopy();
            copy.ResponsiblePersons = new List<Person>(ResponsiblePersons.Select(rp => (Person)rp.DeepCopy()));
            copy.Products = new List<Product>(Products.Select(p => (Product)p.DeepCopy()));
            return copy;
        }

        string INameAndCopy.Name
        {
            get { return RevisionName; }
            set { RevisionName = value; }
        }

        public List<Person> ResponsiblePersons
        {
            get { return responsiblePersons; }
            set { responsiblePersons = value; }
        }

        public List<Product> Products
        {
            get { return products; }
            set { products = value; }
        }

        public Product LatestProduct
        {
            get
            {
                return products.OrderBy(p => p.ManufacturingDate).FirstOrDefault();
            }
        }

        public Product BiggestCodeProduct
        {
            get
            {
                return products.OrderBy(p => p.Code).LastOrDefault();
            }
        }

        public void AddProducts(params Product[] newProducts)
        {
            products.AddRange(newProducts);
        }

        public void AddMembers(params Person[] newMembers)
        {
            responsiblePersons.AddRange(newMembers);
        }

        public Revision Revision
        {
            get { return base.DeepCopy() as Revision; }
            set
            {
                RevisionInfo = value.RevisionInfo;
                RevisionDate = value.RevisionDate;
                RevisionNumber = value.RevisionNumber;
            }
        }

        public override string ToString()
        {
            string productDetails = "";
            foreach (Product product in Products)
            {
                productDetails += product.ToString() + "\n";
            }

            return $"Revision Info: {RevisionInfo}\n" +
                   $"Revision Date: {RevisionDate}\n" +
                   $"Revision Number: {RevisionNumber}\n" +
                   $"Responsible Persons:\n{string.Join("\n", ResponsiblePersons)}\n" +
                   $"Products:\n{productDetails}";
        }

        public virtual string ToShortString()
        {
            return $"Revision Info: {RevisionInfo}\n" +
                   $"Revision Date: {RevisionDate}\n" +
                   $"Revision Number: {RevisionNumber}\n" +
                   $"Responsible Persons: {string.Join(", ", ResponsiblePersons.Select(rp => rp.ToString()))}";
        }

        public IEnumerable<Product> OutOfStockProducts()
        {
            return Products.Where(p => p.Quantity == 0);
        }

        public IEnumerable<Product> ProductsWithName(string name)
        {
            return Products.Where(p => p.Name.Contains(name));
        }

        public IEnumerable<Product> GetFinishedProducts()
        {
            return Products.Where(p => p.Availability == Availability.OutOfStock);
        }

        public IEnumerable<Product> GetProductsByName(string name)
        {
            return Products.Where(p => p.Name.Contains(name));
        }

    }

    public class Person : INameAndCopy
    {
        private string firstName;
        private string lastName;
        private DateTime birthDate;
        private int birthYear;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Person otherPerson = (Person)obj;
            return FirstName == otherPerson.FirstName &&
                   LastName == otherPerson.LastName;
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public DateTime BirthDate
        {
            get { return birthDate; }
            set { birthDate = value; birthYear = value.Year; }
        }

        public int BirthYear
        {
            get { return birthYear; }
            set { birthYear = value; }
        }

        public Person(string firstName, string lastName, DateTime birthDate)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
        }

        public Person()
        {
            FirstName = "Default";
            LastName = "Person";
            BirthDate = DateTime.MinValue;
        }

        public override string ToString()
        {
            return $"First Name: {FirstName}, Last Name: {LastName}, Birth Date: {BirthDate.ToShortDateString()}, Birth Year: {BirthYear}";
        }

        public virtual string ToShortString()
        {
            return $"First Name: {FirstName}, Last Name: {LastName}";
        }

        public virtual object DeepCopy()
        {
            return new Person(FirstName, LastName, BirthDate);
        }

        string INameAndCopy.Name
        {
            get { return $"{FirstName} {LastName}"; }
            set
            {
                string[] names = value.Split(' ');
                if (names.Length >= 2)
                {
                    FirstName = names[0];
                    LastName = names[1];
                }
            }
        }
    }

    class Program
    {
        public static readonly string CultureCode = "ua-UA";

        static void Main()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(CultureCode);
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.Unicode;

            // 1. Створити два об'єкти типу Revision з однаковими даними і перевірити, що
            //    посилання на об'єкти не рівні, а об'єкти рівні, вивести значення хеш-кодів
            Console.WriteLine("1. Створення двох об'єктів типу Revision з однаковими даними та перевірка їх рівності:");
            Revision revision1 = new Revision("Revision 1", DateTime.Now, 1);
            Revision revision2 = new Revision("Revision 1", DateTime.Now, 1);
            Console.WriteLine($"revision1: {revision1}");
            Console.WriteLine($"revision2: {revision2}");
            Console.WriteLine($"revision1 == revision2: {revision1 == revision2}");
            Console.WriteLine($"revision1.Equals(revision2): {revision1.Equals(revision2)}");
            Console.WriteLine($"revision1.GetHashCode(): {revision1.GetHashCode()}");
            Console.WriteLine($"revision2.GetHashCode(): {revision2.GetHashCode()}");

            // 2. Створити блок try/catch та у ньому привласнити некоректне значення властивостям об'єкта
            Console.WriteLine("\n2. Обробка виключення при некоректному присвоєнні значення властивостям об'єкта:");
            try
            {
                revision1.RevisionNumber = -1;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Exception message: {ex.Message}");
            }

            // 3. Створити об'єкт типу WarehouseRevision, додати елементи до списку товарів та
            //    списку відповідальних за ревізію і вивести дані об'єкта WarehouseRevision
            Console.WriteLine("\n3. Створення об'єкта WarehouseRevision, додавання елементів та виведення даних:");
            WarehouseRevision warehouseRevision = new WarehouseRevision();
            warehouseRevision.AddProducts(
                new Product(1, Availability.InStock),
                new Product(2, Availability.OutOfStock),
                new Product(3, Availability.RunningOut)
            );
            warehouseRevision.AddMembers(
                new Person("John", "Doe", DateTime.Now),
                new Person("Jane", "Smith", DateTime.Now)
            );
            Console.WriteLine(warehouseRevision);

            // 4. Вивести значення властивості Revision для об'єкта типу WarehouseRevision
            Console.WriteLine("\n4. Виведення значення властивості Revision для об'єкта WarehouseRevision:");
            Console.WriteLine($"WarehouseRevision.Revision: {warehouseRevision.Revision}");

            // 5. Створити повну копію об'єкта WarehouseRevision за допомогою методу DeepCopy()
            //    та змінити інформацію про ревізію та назву одного товару в оригіналі і вивести
            //    копію та оригінал
            Console.WriteLine("\n5. Повне копіювання об'єкта WarehouseRevision та зміна даних в оригіналі:");
            WarehouseRevision copy = (WarehouseRevision)warehouseRevision.DeepCopy();
            DateTime revisionDate = DateTime.Now; // Здесь можно использовать нужную вам дату ревизии
            warehouseRevision.Revision = new Revision("Revision 2", revisionDate, 1);
            warehouseRevision.Products[0].Name = "Product 1 (Updated)";
            Console.WriteLine("Copy:");
            Console.WriteLine(copy);
            Console.WriteLine("Original:");
            Console.WriteLine(warehouseRevision);

            // 6. Вивести список товарів, що закінчились, за допомогою оператора foreach та ітератора
            Console.WriteLine("\n6. Виведення списку товарів, що закінчились:");
            foreach (Product product in warehouseRevision.GetFinishedProducts())
            {
                Console.WriteLine(product);
            }

            // 7. Вивести список товарів, в назві яких є заданий рядок, за допомогою оператора foreach та ітератора з параметром
            string searchString = "2";
            Console.WriteLine($"\n7. Виведення списку товарів, в назві яких є рядок \"{searchString}\":");
            foreach (Product product in warehouseRevision.GetProductsByName(searchString))
            {
                Console.WriteLine(product);
            }

            // 8. Порівняння часу виконання операцій з різними типами масивів
            Console.WriteLine("\n8. Порівняння часу виконання операцій з різними типами масивів:");

            var inputOk = false;
            int nrow = 0;
            int ncol = 0;

            while (!inputOk)
            {
                Console.WriteLine("Введіть кількість рядків та колонок (наприклад \"100:200\")");
                var raw = Console.ReadLine().Split(':');
                if (raw.Length != 2)
                    continue;

                if (!int.TryParse(raw[0], out nrow))
                    continue;
                if (!int.TryParse(raw[1], out ncol))
                    continue;
                inputOk = true;
            }

            Console.WriteLine("Ініціалізація масивів, будь ласка, зачекайте...");
            Product[] productsArray = new Product[nrow];
            Product[,] productsMatrix = new Product[nrow, ncol];
            Product[][] productsJaggedArray = new Product[nrow][];
            for (int i = 0; i < nrow; i++)
            {
                productsArray[i] = new Product($"Product {i}", i, Availability.InStock, DateTime.UtcNow);

                for (int j = 0; j < ncol; j++)
                {
                    productsMatrix[i, j] = new Product($"Product {i}-{j}", i * j, Availability.InStock, DateTime.UtcNow);
                    if (productsJaggedArray[i] == null)
                        productsJaggedArray[i] = new Product[ncol];
                    productsJaggedArray[i][j] = new Product($"Product {i}-{j}", i * j, Availability.InStock, DateTime.UtcNow);
                }
            }

            // Вимірювання часу для одновимірного масиву
            Stopwatch watch = Stopwatch.StartNew();
            foreach (Product product in productsArray)
            {
                // Виконати дії з об'єктом
                product.Code++;
                product.Name += "1";
            }
            watch.Stop();
            Console.WriteLine($"Час виконання операцій з елементами одновимірного масиву: {watch.ElapsedMilliseconds} ms");

            // Вимірювання часу для двовимірного масиву
            watch = Stopwatch.StartNew();
            foreach (Product product in productsMatrix)
            {
                // Виконати дії з об'єктом
                product.Code++;
                product.Name += "1";
            }
            watch.Stop();
            Console.WriteLine($"Час виконання операцій з елементами двовимірного прямокутного масиву: {watch.ElapsedMilliseconds} ms");

            // Вимірювання часу для ступінчастого масиву
            watch = Stopwatch.StartNew();
            foreach (Product[] productRow in productsJaggedArray)
            {
                foreach (Product product in productRow)
                {
                    // Виконати дії з об'єктом
                    product.Code++;
                    product.Name += "1";
                }
            }
            watch.Stop();
            Console.WriteLine($"Час виконання операцій з елементами ступінчастого масиву: {watch.ElapsedMilliseconds} ms");

            Console.WriteLine("\nНатисніть будь-яку кнопку для закриття консолі...");
            Console.ReadKey();
        }
    }
}