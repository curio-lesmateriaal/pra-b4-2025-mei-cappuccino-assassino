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
        public static Home Window { get; set; }
        private List<OrderedProduct> orderedProducts = new List<OrderedProduct>();
        private decimal totalAmount = 0.00m;

        public void Start()
        {
            // Stel de prijslijst in aan de rechter kant.
            ShopManager.SetShopPriceList("Prijzen:\nFoto 10x15: €2.55");

            // Stel de bon in onderaan het scherm
            ShopManager.SetShopReceipt("Eindbedrag\n€0.00");

            // Vul de productlijst met producten
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 10x15", Price = 2.55m, Description = "Standaard fotoformaat 10x15 cm" });
            // Placeholders
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 13x18", Price = 3.25m, Description = "Middelgroot fotoformaat 13x18 cm" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 20x30", Price = 5.95m, Description = "Groot fotoformaat 20x30 cm" });

            // Update prijslijst met alle producten
            UpdatePriceList();

            // Update dropdown met producten
            ShopManager.UpdateDropDownProducts();

            // Alternative: Direct ComboBox population if ShopManager doesn't work properly
            if (Window?.cbProducts != null)
            {
                Window.cbProducts.Items.Clear();
                foreach (KioskProduct product in ShopManager.Products)
                {
                    Window.cbProducts.Items.Add(product);
                }
            }

            // Set default selection - FIX FOR THE BUG
            if (ShopManager.Products.Count > 0 && Window?.cbProducts != null)
            {
                Window.cbProducts.SelectedIndex = 0;
            }
        }

        private void UpdatePriceList()
        {
            // Begin met een header voor de prijslijst
            StringBuilder priceList = new StringBuilder("Prijzen:\n");

            // Loop door alle producten en voeg ze toe aan de prijslijst
            foreach (KioskProduct product in ShopManager.Products)
            {
                priceList.AppendLine($"{product.Name}: €{product.Price:F2}");
            }

            // Stel de prijslijst in
            ShopManager.SetShopPriceList(priceList.ToString());
        }

        // Wordt uitgevoerd wanneer er op de Toevoegen knop is geklikt
        public void AddButtonClick()
        {
            try
            {
                // Controleer of er producten in de ComboBox staan
                if (Window.cbProducts.Items.Count == 0)
                {
                    MessageBox.Show("Er zijn geen producten beschikbaar om te selecteren.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if something is selected - IMPROVED ERROR CHECKING
                if (Window.cbProducts.SelectedIndex == -1)
                {
                    MessageBox.Show("Selecteer eerst een product uit de lijst.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Haal de geselecteerde product op
                KioskProduct selectedProduct = Window.cbProducts.SelectedItem as KioskProduct;
                if (selectedProduct == null)
                {
                    MessageBox.Show("Het geselecteerde item is geen geldig product.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Haal het aantal op uit het tekstveld
                if (!int.TryParse(Window.tbAmount.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Voer een geldig aantal in (moet een positief getal zijn).", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Haal de foto ID op
                string photoId = Window.tbFotoId.Text.Trim();
                if (string.IsNullOrEmpty(photoId))
                {
                    MessageBox.Show("Voer een foto ID in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Maak een nieuw besteld product aan met jouw bestaande klasse structuur
                OrderedProduct orderedProduct = new OrderedProduct()
                {
                    PhotoId = photoId,
                    ProductName = selectedProduct.Name,
                    Quantity = quantity,
                    TotalPrice = selectedProduct.Price * quantity
                };

                // Voeg het product toe aan de lijst
                orderedProducts.Add(orderedProduct);

                // Update het totaalbedrag
                totalAmount += orderedProduct.TotalPrice;

                // Update de kassabon
                UpdateReceipt();

                // Reset de invoervelden voor het volgende product
                Window.tbFotoId.Text = "";
                Window.tbAmount.Text = "1";

                MessageBox.Show($"{quantity}x {selectedProduct.Name} toegevoegd aan de bestelling.", "Toegevoegd", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReceipt()
        {
            StringBuilder receipt = new StringBuilder();

            // Voeg het eindbedrag toe
            receipt.AppendLine($"Eindbedrag");
            receipt.AppendLine($"€{totalAmount:F2}");
            receipt.AppendLine();

            // Voeg een scheidingslijn toe
            receipt.AppendLine("------------------------");

            // Voeg alle bestelde producten toe
            receipt.AppendLine("Kassabon:");
            foreach (OrderedProduct product in orderedProducts)
            {
                receipt.AppendLine(product.ToString());
            }

            // Update de kassabon in de UI
            ShopManager.SetShopReceipt(receipt.ToString());
        }

        // Wordt uitgevoerd wanneer er op de Resetten knop is geklikt
        public void ResetButtonClick()
        {
            // Reset de lijst met bestelde producten
            orderedProducts.Clear();

            // Reset het totaalbedrag
            totalAmount = 0.00m;

            // Reset de bon
            ShopManager.SetShopReceipt("Eindbedrag\n€0.00");

            // Reset de invoervelden
            if (Window != null)
            {
                Window.tbFotoId.Text = "";
                Window.tbAmount.Text = "1";
                if (Window.cbProducts.Items.Count > 0)
                {
                    Window.cbProducts.SelectedIndex = 0;
                }
            }

            MessageBox.Show("Bestelling is gereset.", "Reset", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Wordt uitgevoerd wanneer er op de Save knop is geklikt
        public void SaveButtonClick()
        {
            try
            {
                if (orderedProducts.Count == 0)
                {
                    MessageBox.Show("Er zijn geen producten om op te slaan.", "Geen producten", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Genereer een unieke bestandsnaam met datum en tijd
                string fileName = $"Kassabon_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                // Maak de kassabon content
                StringBuilder receiptContent = new StringBuilder();
                receiptContent.AppendLine("=== FOTO KIOSK KASSABON ===");
                receiptContent.AppendLine($"Datum: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
                receiptContent.AppendLine();

                receiptContent.AppendLine("BESTELDE PRODUCTEN:");
                receiptContent.AppendLine("------------------------");

                foreach (OrderedProduct product in orderedProducts)
                {
                    receiptContent.AppendLine(product.ToString());
                }

                receiptContent.AppendLine("------------------------");
                receiptContent.AppendLine($"TOTAALBEDRAG: €{totalAmount:F2}");
                receiptContent.AppendLine();
                receiptContent.AppendLine("Bedankt voor uw bestelling!");

                // Sla het bestand op
                File.WriteAllText(filePath, receiptContent.ToString(), Encoding.UTF8);

                MessageBox.Show($"Kassabon opgeslagen als: {fileName}\nLocatie: {filePath}", "Opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan van de kassabon: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Extra hulpmethode om producten te zoeken
        public KioskProduct GetProductByName(string productName)
        {
            return ShopManager.Products.FirstOrDefault(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
        }

        // Hulpmethode om het totaalbedrag op te halen
        public decimal GetTotalAmount()
        {
            return totalAmount;
        }

        // Hulpmethode om het aantal bestelde producten op te halen
        public int GetOrderedProductsCount()
        {
            return orderedProducts.Count;
        }

        // Hulpmethode om een specifiek besteld product te verwijderen
        public void RemoveOrderedProduct(int index)
        {
            if (index >= 0 && index < orderedProducts.Count)
            {
                OrderedProduct productToRemove = orderedProducts[index];
                totalAmount -= productToRemove.TotalPrice;
                orderedProducts.RemoveAt(index);
                UpdateReceipt();
            }
        }
    }
}