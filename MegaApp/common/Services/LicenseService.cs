﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Storage;
using Microsoft.Phone.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Converters;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Views;

// Use mocking library in DEBUG mode
// And the real Windows Store library in RELEASE Mode
#if DEBUG
    using MockIAPLib;
    using Store = MockIAPLib;
#else
    using Windows.ApplicationModel.Store;
#endif

namespace MegaApp.Services
{
    /// <summary>
    /// Class that handles in app purchases and MEGA licence validation
    /// </summary>
    public static class LicenseService
    {
        /// <summary>
        /// Check to see if the Windows Store is available to retrieve information
        /// </summary>
        /// <returns>True if the Windows Store information is available else it returns False</returns>
        public static async Task<bool> IsAvailable()
        {
            try
            {
                var products= await GetProducts();
                // If no products are available. We need not to show any information so return False
                return products != null && products.Any();
            }
            catch
            {
                // If listing information can not be loaded. No internet is available or the 
                // Windows Store is unavailable for retrieving data
                return false;
            }
        }

        /// <summary>
        /// Get all available products (in app purchases) for this app from the Windows Store
        /// </summary>
        /// <returns>List of products available in the Windows Store</returns>
        private static async Task<IReadOnlyDictionary<string, ProductListing>> GetProducts()
        {
            var listingInformation = await CurrentApp.LoadListingInformationAsync();
            return listingInformation == null ? null : listingInformation.ProductListings;
        }

        /// <summary>
        /// Get Windows Store product matching a MEGA product
        /// </summary>
        /// <param name="megaProductId">MEGA product identifier to match</param>
        /// <returns>Windows Store product or NULL if none available</returns>
        public static async Task<ProductListing> GetProductAsync(string megaProductId)
        {
            try
            {
                if (string.IsNullOrEmpty(megaProductId)) return null;

                var listing = await CurrentApp.LoadListingInformationAsync();
                if (listing == null || !listing.ProductListings.Any()) return null;

                var result = listing.ProductListings.First(
                    p => p.Key.ToLower().Equals(megaProductId.ToLower()));

                return result.Value;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                    string.Format("Failure retrieving store product for {0}", megaProductId), e);
                return null;
            }
        }

        /// <summary>
        /// Purchase a product by product id
        /// </summary>
        /// <param name="productId">The id of the product the user want to purchase</param>
        private static async Task PurchaseProduct(string productId)
        {
            if(string.IsNullOrEmpty(productId)) throw new ArgumentNullException("productId");

            // Check to see if the user already owns a product license
            // Do not try to purchase again if the users already owns it
            if (CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(productId) &&
                CurrentApp.LicenseInformation.ProductLicenses[productId].IsActive)
            {
                new CustomMessageDialog(
                    AppMessages.AM_AlreadyPurchased_Title,
                    AppMessages.AM_AlreadyPurchased,
                        App.AppInformation).ShowDialog();
                return;
            }

            // Kick off purchase; ask for a receipt when it returns
            // It will raise an error if the user cancels the purchase or something goes wrong
            var result = await CurrentApp.RequestProductPurchaseAsync(productId, true);
            
            // If we do not get a receipt raise an error to inform the user
            if(string.IsNullOrEmpty(result)) throw new Exception("Purchase failed");

            // Now that purchase is done, give the user the goods they paid for by sending receipt to the MEGA license server
            SendLicenseToMega(result);
        }

        /// <summary>
        /// Purchase a MEGA product/subscription
        /// </summary>
        /// <param name="product">MEGA product/subscription/plan</param>
        public static async Task PurchaseProduct(Product product)
        {
            try
            {
                if (product == null) throw new ArgumentNullException("product");

                var products = await GetProducts();
                if (products == null || !products.Any()) throw new Exception("Failed to retrieve products");
                
                switch (product.AccountType)
                {
                    case MAccountType.ACCOUNT_TYPE_PROI:
                        switch (product.Months)
                        {
                            case 1:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.Pro1Month)).Value.ProductId);
                                break;
                            case 12:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.Pro1Year)).Value.ProductId);
                                break;
                        }
                        break;
                    case MAccountType.ACCOUNT_TYPE_PROII:
                        switch (product.Months)
                        {
                            case 1:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.Pro2Month)).Value.ProductId);
                                break;
                            case 12:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.Pro2Year)).Value.ProductId);
                                break;
                        }
                        break;
                    case MAccountType.ACCOUNT_TYPE_PROIII:
                        switch (product.Months)
                        {
                            case 1:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.Pro3Month)).Value.ProductId);
                                break;
                            case 12:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.Pro3Year)).Value.ProductId);
                                break;
                        }
                        break;
                    case MAccountType.ACCOUNT_TYPE_LITE:
                        switch (product.Months)
                        {
                            case 1:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.ProLiteMonth)).Value.ProductId);
                                break;
                            case 12:
                                await PurchaseProduct(products.First(p => p.Key.Equals(AppResources.ProLiteYear)).Value.ProductId);
                                break;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch
            {
                new CustomMessageDialog(
                    AppMessages.AM_PurchaseFailed_Title,
                    AppMessages.AM_PurchaseFailed,
                        App.AppInformation).ShowDialog();
            }
        }

        /// <summary>
        /// Send the receipt to MEGA License Server to verify and validate the purchase
        /// </summary>
        /// <param name="receipt">In app purchase receipt received from the Windows Store</param>
        private static void SendLicenseToMega(string receipt)
        {
            // Validate and activate the MEGA Windows Store (int 13) subscription on the MEGA license server
            SdkService.MegaSdk.submitPurchaseReceipt((int)MPaymentMethod.PAYMENT_METHOD_WINDOWS_STORE, 
                receipt, new PurchaseReceiptRequestListener(receipt));
        }

        /// <summary>
        /// Validate if purchased subscription products are activated for the account of the user
        /// </summary>
        public static void ValidateLicenses()
        {
            try
            {
                // Only try to validate if network connection is available
                if (!NetworkService.IsNetworkAvailable()) return;
                // First retrieve all active licenses from the Windows Store
                var licenses = CurrentApp.LicenseInformation.ProductLicenses;
                // If no licenses available. Stop validation process
                if (!licenses.Any()) return;

                // Validate the licenses
                foreach (var productLicense in licenses)
                {
                    // only validate active licenses
                    if(!productLicense.Value.IsActive) continue;
                    // Check active license to current account license status
                    LicenseCheck(productLicense.Value.ProductId);
                }
            }
            catch
            {
                // if an error occurs, ignore. App will try again on restart
            }
            
        }

        /// <summary>
        /// Check if the license for the selected product is already active. If not revalidate the receipt at MEGA license server
        /// </summary>
        /// <param name="productId">Produc identifier to check license validation status</param>
        private static async void LicenseCheck(string productId)
        {
            try
            {
                // Retrieve the original Windows Store receipt
                var receipt = await CurrentApp.GetProductReceiptAsync(productId);
                if (string.IsNullOrEmpty(receipt)) return;

                var xDoc = XDocument.Parse(receipt, LoadOptions.None);
                var uniqueId = xDoc.Root.Descendants().First().Attribute("Id").Value;

                var currentIds = SettingsService.LoadSetting(SettingsResources.Receipts, string.Empty);
                var currentIdsList = currentIds.Split(';');
                var isValidated = currentIdsList.Any(id => id.Equals(uniqueId));
                if (!isValidated) SendLicenseToMega(receipt);
            }
            catch
            {
                // if an error occurs, ignore. App will try again on restart
            }
        }

#if DEBUG
        /// <summary>
        /// Setup the Mocking Library for in app purchases
        /// </summary>
        public static void SetupMockIap()
        {
            MockIAP.Init();
            MockIAP.ClearCache();
            MockIAP.RunInMockMode(true);

            // Default listing information to test iap
            // Use the current culture to retrieve products for the current mobile phone
            MockIAP.SetListingInformation(1, CultureInfo.CurrentUICulture.Name, "Test description", "1", "TestApp");

            // Needed 1 product to check products in license service is available
            var p = new ProductListing
            {
                Name = "Test",
                ProductId = "Test",
                ImageUri = null,
                ProductType = Windows.ApplicationModel.Store.ProductType.Durable,
                Keywords = new[] { "Test" },
                Description = "Test",
                FormattedPrice = "0.99",
                Tag = string.Empty
            };

            MockIAP.AddProductListing("Test", p);
        }

        /// <summary>
        /// Add a MEGA product as test product to the Mock Iap library
        /// </summary>
        /// <param name="product">MEGA product to add to Mock Iap</param>
        public static void AddProductToMockIap(Product product)
        {
            if (product == null) return;

            string productId = product.MicrosoftStoreId;
            
            // Only add the products once
            if (!MockIAP.allProducts.ContainsKey(productId))
            {
                var p = new ProductListing
                {
                    Name = product.Name + " (" + product.Months + " months)",
                    ProductId = productId,
                    ImageUri = null,
                    ProductType = Windows.ApplicationModel.Store.ProductType.Durable,
                    Keywords = new[] { productId },
                    Description = product.Name,
                    FormattedPrice = product.FormattedPrice,
                    Tag = string.Empty
                };

                MockIAP.AddProductListing(productId, p);
            }
        }
#endif

    }
}
