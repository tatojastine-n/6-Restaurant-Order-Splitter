using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant_Order_Splitter
{
    public class MenuItem
    {
        public string Name { get; }
        public decimal Price { get; }
        public bool IsShared { get; }

        public MenuItem(string name, decimal price, bool isShared = false)
        {
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));

            Name = name;
            Price = price;
            IsShared = isShared;
        }
    }
    public class Diner
    {
        public string Name { get; }
        public List<MenuItem> PersonalItems { get; } = new List<MenuItem>();
        public decimal TipPercentage { get; set; }
        public decimal TotalAmount { get; private set; }

        public Diner(string name, decimal tipPercentage = 0)
        {
            Name = name;
            TipPercentage = tipPercentage;
        }

        public void AddItem(MenuItem item)
        {
            PersonalItems.Add(item);
        }

        public void AddSharedItemContribution(decimal amount)
        {
            TotalAmount += amount;
        }

        public void CalculateTotal(decimal serviceChargePercentage, decimal sharedItemsTotal, int totalDiners)
        {
            decimal personalTotal = PersonalItems.Sum(item => item.Price);

            decimal sharedPortion = sharedItemsTotal / totalDiners;

            decimal subtotal = personalTotal + sharedPortion;

            decimal serviceCharge = subtotal * (serviceChargePercentage / 100m);

            decimal tip = subtotal * (TipPercentage / 100m);

            TotalAmount = subtotal + serviceCharge + tip;
        }
    }
    public class BillSplitter
    {
        private readonly List<MenuItem> _allItems;
        private readonly List<Diner> _diners;
        private readonly decimal _serviceChargePercentage;

        public BillSplitter(List<MenuItem> items, List<Diner> diners, decimal serviceChargePercentage)
        {
            _allItems = items ?? throw new ArgumentNullException(nameof(items));
            _diners = diners ?? throw new ArgumentNullException(nameof(diners));
            _serviceChargePercentage = serviceChargePercentage;

            ValidateItems();
            AssignItemsToDiners();
        }

        private void ValidateItems()
        {
            if (_allItems.Any(item => item.Price < 0))
                throw new ArgumentException("All item prices must be non-negative");

            var unassignedItems = _allItems.Where(item => !_diners.Any(d => d.PersonalItems.Contains(item)) && !item.IsShared).ToList();
            if (unassignedItems.Any())
                Console.WriteLine($"Warning: {unassignedItems.Count} items not assigned to any diner and not marked as shared");
        }

        private void AssignItemsToDiners()
        {          
        }

        public void CalculateBill()
        {
            var personalItems = _diners.SelectMany(d => d.PersonalItems).ToList();
            var sharedItems = _allItems.Except(personalItems).Where(i => i.IsShared).ToList();
            decimal sharedItemsTotal = sharedItems.Sum(item => item.Price);

            foreach (var diner in _diners)
            {
                diner.CalculateTotal(_serviceChargePercentage, sharedItemsTotal, _diners.Count);
            }
        }

        public void PrintBill()
        {
            Console.WriteLine("\nBill Summary:");
            Console.WriteLine($"Service Charge: {_serviceChargePercentage}%");
            Console.WriteLine("\nItem Breakdown:");

            var sharedItems = _allItems.Where(i => i.IsShared).ToList();
            if (sharedItems.Any())
            {
                Console.WriteLine("\nShared Items:");
                foreach (var item in sharedItems)
                {
                    Console.WriteLine($"- {item.Name}: {item.Price:C}");
                }
                Console.WriteLine($"Total Shared: {sharedItems.Sum(i => i.Price):C}");
                Console.WriteLine($"Per Diner Share: {sharedItems.Sum(i => i.Price) / _diners.Count:C}");
            }

            foreach (var diner in _diners.OrderBy(d => d.Name))
            {
                Console.WriteLine($"\n{diner.Name}'s Items:");
                if (!diner.PersonalItems.Any() && !sharedItems.Any())
                {
                    Console.WriteLine("No items assigned");
                    continue;
                }

                foreach (var item in diner.PersonalItems)
                {
                    Console.WriteLine($"- {item.Name}: {item.Price:C}");
                }

                Console.WriteLine($"\n{diner.Name}'s Total Breakdown:");
                decimal personalTotal = diner.PersonalItems.Sum(item => item.Price);
                decimal sharedPortion = sharedItems.Sum(item => item.Price) / _diners.Count;
                decimal subtotal = personalTotal + sharedPortion;
                decimal service = subtotal * (_serviceChargePercentage / 100m);
                decimal tip = subtotal * (diner.TipPercentage / 100m);

                Console.WriteLine($"Personal Items: {personalTotal:C}");
                Console.WriteLine($"Shared Items Portion: {sharedPortion:C}");
                Console.WriteLine($"Subtotal: {subtotal:C}");
                Console.WriteLine($"Service Charge: {service:C}");
                Console.WriteLine($"Tip ({diner.TipPercentage}%): {tip:C}");
                Console.WriteLine($"Total: {diner.TotalAmount:C}");
            }

            decimal grandTotal = _diners.Sum(d => d.TotalAmount);
            Console.WriteLine($"\nGrand Total for All Diners: {grandTotal:C}");
        }
    }

    internal class Program
    {
       
        public static object PackingStrategy { get; private set; }
        static void Main(string[] args)
        {
            try
            {
                var items = new List<MenuItem>
            {
                new MenuItem("Steak", 25.00m),
                new MenuItem("Salad", 8.50m),
                new MenuItem("Wine", 30.00m, true),  
                new MenuItem("Soup", 6.50m),
                new MenuItem("Dessert", 12.00m, true) 
            };

                var diners = new List<Diner>
            {
                new Diner("Alice", 15),  
                new Diner("Bob", 10),    
                new Diner("Charlie", 20) 
            };
              
                diners[0].AddItem(items[0]); 
                diners[0].AddItem(items[1]); 
                diners[1].AddItem(items[3]); 
               
                decimal serviceCharge = 10m;

                var billSplitter = new BillSplitter(items, diners, serviceCharge);
                billSplitter.CalculateBill();
                billSplitter.PrintBill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
