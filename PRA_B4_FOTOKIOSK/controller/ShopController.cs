using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home? Window { get; set; }
        private decimal total = 0;

        public void Start()
        {
            // Initialize products with prices and descriptions
            ShopManager.Products.Add(new KioskProduct() 
            { 
                Name = "Foto 10x15", 
                Price = 2.55m,
                Description = "Standaard formaat foto"
            });
            
            ShopManager.Products.Add(new KioskProduct() 
            { 
                Name = "Foto 13x18", 
                Price = 3.95m,
                Description = "Groot formaat foto"
            });
            
            ShopManager.Products.Add(new KioskProduct() 
            { 
                Name = "Foto 20x30", 
                Price = 7.50m,
                Description = "Poster formaat foto"
            });

            // Initialize receipt
            ShopManager.SetShopReceipt("Bon:\n");
            
            // Set price list
            StringBuilder priceList = new StringBuilder("Prijslijst:\n\n");
            foreach (KioskProduct product in ShopManager.Products)
            {
                priceList.AppendLine($"{product.Name}");
                priceList.AppendLine($"Prijs: €{product.Price:F2}");
                priceList.AppendLine($"{product.Description}\n");
            }
            ShopManager.SetShopPriceList(priceList.ToString());

            // Update product dropdown
            ShopManager.UpdateDropDownProducts();
        }

        public void AddButtonClick()
        {
            // Get values from form
            var selectedProduct = ShopManager.GetSelectedProduct();
            var fotoId = ShopManager.GetFotoId();
            var amount = ShopManager.GetAmount();

            // Validate input
            if (selectedProduct == null)
            {
                MessageBox.Show("Selecteer een product");
                return;
            }
            if (!fotoId.HasValue)
            {
                MessageBox.Show("Vul een geldig foto ID in");
                return;
            }
            if (!amount.HasValue || amount.Value <= 0)
            {
                MessageBox.Show("Vul een geldig aantal in");
                return;
            }

            // Calculate totals
            decimal itemTotal = selectedProduct.Price * amount.Value;
            total += itemTotal;

            // Format receipt
            string receiptLine = $"\n{selectedProduct.Name}\n" +
                               $"Foto ID: {fotoId.Value}\n" +
                               $"Aantal: {amount.Value} x €{selectedProduct.Price:F2}\n" +
                               $"Subtotaal: €{itemTotal:F2}\n" +
                               $"----------------------------------------\n" +
                               $"TOTAAL: €{total:F2}";

            ShopManager.SetShopReceipt(ShopManager.GetShopReceipt() + receiptLine);
        }

        public void ResetButtonClick()
        {
            total = 0;
            ShopManager.SetShopReceipt("Bon:\n");
        }

        public void SaveButtonClick()
        {
            if (total == 0)
            {
                MessageBox.Show("Er zijn geen items om op te slaan");
                return;
            }

            string fileName = $"bon_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            File.WriteAllText(fileName, ShopManager.GetShopReceipt());
            MessageBox.Show($"Bon opgeslagen als {fileName}");
        }
    }
}
