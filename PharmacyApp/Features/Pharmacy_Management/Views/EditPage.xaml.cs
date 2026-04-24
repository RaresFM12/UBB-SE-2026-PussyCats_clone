using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Pharmacy_Management.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Pharmacy_Management
{
    public sealed partial class EditPage : Page
    {
        public EditPageViewModel ViewModel { get; }

        private bool isGetItemDataClicked = false;

        public Dictionary<string, float> ActiveSubstancesDict { get; set; } = new Dictionary<string, float>();

        public Dictionary<DateOnly, int> BatchesDict { get; set; } = new Dictionary<DateOnly, int>();

        public EditPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.ViewModel = new EditPageViewModel();
            this.DataContext = this.ViewModel;
            this.ApplyUiStateFromViewModel();
            this.ResetUiValidationState();
        }

        public class ActiveSubstance
        {
            public string Name { get; set; }

            public float Concentration { get; set; }
        }

        public class BatchItem
        {
            public DateOnly Date { get; set; }

            public int Packs { get; set; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ResetUiValidationState();
        }

        private void ApplyUiStateFromViewModel()
        {
            this.ItemListButtons.Visibility = this.ViewModel.ItemListButtonsVisibility;
            this.ItemBottomButtons.Visibility = this.ViewModel.ItemBottomButtonsVisibility;
            this.ShowExpiredItemsToggle.Visibility = this.ViewModel.ShowExpiredItemsToggleVisibility;

            this.SubstanceListButtons.Visibility = this.ViewModel.SubstanceListButtonsVisibility;
            this.SubstanceBottomButtons.Visibility = this.ViewModel.SubstanceBottomButtonsVisibility;
            this.AddSubstanceGrid.Visibility = this.ViewModel.AddSubstanceGridVisibility;
            this.UpdateSubstanceGrid.Visibility = this.ViewModel.UpdateSubstanceGridVisibility;
        }

        private void ResetUiValidationState()
        {
            this.isGetItemDataClicked = false;

            this.ViewModel.ActivateItemsSection();
            this.ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            this.ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            this.ApplyUiStateFromViewModel();

            this.AddItemGrid.Visibility = Visibility.Collapsed;
            this.UpdateItemGrid.Visibility = Visibility.Collapsed;

            this.SearchBox.Text = string.Empty;
            this.ShowExpiredToggle.IsOn = false;

            this.ClearItemAddBoxes();
            this.ClearItemUpdateBoxes();
            this.ClearSubstanceBoxes();
            this.ClearSubstanceUpdateBoxes();

            this.ActiveSubstancesDict.Clear();
            this.BatchesDict.Clear();
            this.RefreshActiveSubstancesList();
            this.RefreshActiveSubstancesListUpdate();
            this.RefreshBatchesList();
            this.RefreshBatchesListUpdate();

            this.ItemList.SelectedItem = null;
            this.SubstanceList.SelectedItem = null;

            this.RemoveItemError.Visibility = Visibility.Collapsed;
            this.RemoveSubstanceError.Visibility = Visibility.Collapsed;

            this.ResetAddItemErrors();
            this.ResetUpdateItemErrors();
            this.ResetSubstanceErrors();
            this.ResetActiveSubstanceErrors();
            this.ResetAddBatchErrors();
        }

        private void GoToStatisticsClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StatisticsPage));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.SearchItems(this.SearchBox.Text);
            this.ItemList.ItemsSource = this.ViewModel.Items;
        }

        private void OnItemClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActivateItemsSection();
            this.ApplyUiStateFromViewModel();
            this.ResetSubstanceErrors();
        }

        private void OnSubstancesClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ActivateSubstancesSection();
            this.ApplyUiStateFromViewModel();

            this.AddItemGrid.Visibility = Visibility.Collapsed;
            this.UpdateItemGrid.Visibility = Visibility.Collapsed;
            this.RemoveItemError.Visibility = Visibility.Collapsed;
        }

        private void OnOrdersClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Features.Orders.Views.OrderManagementPage), new OrderService());
        }

        private void ShowExpiredToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (this.ShowExpiredToggle.IsOn)
            {
                this.ViewModel.ShowExpiredItems();
                this.ItemList.ItemsSource = this.ViewModel.Items;
            }
            else
            {
                this.ViewModel.RefreshItems();
                this.ItemList.ItemsSource = this.ViewModel.Items;
            }
        }

        private void ClearItemAddBoxes()
        {
            this.NameBox.Text = string.Empty;
            this.ProducerBox.Text = string.Empty;
            this.PriceBox.Text = string.Empty;
            this.CategoryBox.Text = string.Empty;
            this.ImagePathBox.Text = string.Empty;
            this.NumberOfPillsBox.Text = string.Empty;
            this.LabelBox.Text = string.Empty;
            this.DescriptionBox.Text = string.Empty;
            this.DiscountBox.Text = string.Empty;
            this.SubstanceNameBox.Text = string.Empty;
            this.PacksBox.Text = string.Empty;
        }

        private void ClearItemUpdateBoxes()
        {
            this.IdBox.Text = string.Empty;
            this.NameBoxUpdate.Text = string.Empty;
            this.ProducerBoxUpdate.Text = string.Empty;
            this.PriceBoxUpdate.Text = string.Empty;
            this.CategoryBoxUpdate.Text = string.Empty;
            this.ImagePathBoxUpdate.Text = string.Empty;
            this.NumberOfPillsBoxUpdate.Text = string.Empty;
            this.LabelBoxUpdate.Text = string.Empty;
            this.DescriptionBoxUpdate.Text = string.Empty;
            this.DiscountBoxUpdate.Text = string.Empty;
            this.SubstanceNameBoxUpdate.Text = string.Empty;
            this.PacksBoxUpdate.Text = string.Empty;
        }

        private void ClearSubstanceBoxes()
        {
            this.NameBoxSubstance.Text = string.Empty;
            this.LethalDoseBoxSubstance.Text = string.Empty;
            this.DescriptionBoxSubstance.Text = string.Empty;
        }

        private void ClearSubstanceUpdateBoxes()
        {
            this.NameBoxSubstanceUpdate.Text = string.Empty;
            this.LethalDoseBoxSubstanceUpdate.Text = string.Empty;
            this.DescriptionBoxSubstanceUpdate.Text = string.Empty;
        }

        private void OnItemAddClick(object sender, RoutedEventArgs e)
        {
            this.RemoveUpdateActiveSubstanceError.Visibility = Visibility.Collapsed;

            if (this.AddItemGrid.Visibility == Visibility.Visible)
            {
                this.AddItemGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.AddItemGrid.Visibility = Visibility.Visible;
                this.UpdateItemGrid.Visibility = Visibility.Collapsed;
                this.ResetAddItemErrors();
                this.ResetActiveSubstanceErrors();
                this.ResetAddBatchErrors();
            }
        }

        private void OnAddItemClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddItem())
            {
                return;
            }

            float discount = 0f;
            string name = this.NameBox.Text;
            string producer = this.ProducerBox.Text;
            string category = this.CategoryBox.Text;
            string imagePath = this.ImagePathBox.Text;
            float price = float.Parse(this.PriceBox.Text);
            int numberOfPills = int.Parse(this.NumberOfPillsBox.Text);

            if (this.DiscountBox.Text != string.Empty)
            {
                discount = float.Parse(this.DiscountBox.Text);
            }

            string label = this.LabelBox.Text;
            string description = this.DescriptionBox.Text;

            this.ViewModel.AddItemFromDetails(
                name,
                producer,
                category,
                price,
                numberOfPills,
                label,
                description,
                imagePath,
                discount,
                this.ActiveSubstancesDict,
                this.BatchesDict);

            this.ViewModel.RefreshItems();
            this.ItemList.ItemsSource = this.ViewModel.Items;

            System.Diagnostics.Debug.WriteLine("Added item");

            this.ClearItemAddBoxes();
            this.ActiveSubstancesDict.Clear();
            this.RefreshActiveSubstancesList();
            this.BatchesDict.Clear();
            this.RefreshBatchesList();
            this.ResetAddItemErrors();
        }

        private bool ValidateAddItem()
        {
            bool isValid = true;

            if (this.NameBox.Text == string.Empty)
            {
                this.NameBox.Background = new SolidColorBrush(Colors.LightPink);
                this.NameBox.Text = string.Empty;
                this.AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.ProducerBox.Text == string.Empty)
            {
                this.ProducerBox.Background = new SolidColorBrush(Colors.LightPink);
                this.ProducerBox.Text = string.Empty;
                this.AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.CategoryBox.Text == string.Empty)
            {
                this.CategoryBox.Background = new SolidColorBrush(Colors.LightPink);
                this.CategoryBox.Text = string.Empty;
                this.AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.PriceBox.Text == string.Empty)
            {
                this.PriceBox.Background = new SolidColorBrush(Colors.LightPink);
                this.PriceBox.Text = string.Empty;
                this.AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.NumberOfPillsBox.Text == string.Empty)
            {
                this.NumberOfPillsBox.Background = new SolidColorBrush(Colors.LightPink);
                this.NumberOfPillsBox.Text = string.Empty;
                this.AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(this.PriceBox.Text, out float price))
            {
                this.PriceBox.Background = new SolidColorBrush(Colors.LightPink);
                this.PriceBox.Text = string.Empty;
                this.AddItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(this.NumberOfPillsBox.Text, out int numberOfPills))
            {
                this.NumberOfPillsBox.Background = new SolidColorBrush(Colors.LightPink);
                this.NumberOfPillsBox.Text = string.Empty;
                this.AddItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(this.DiscountBox.Text, out float discount) && this.DiscountBox.Text != string.Empty)
            {
                this.DiscountBox.Background = new SolidColorBrush(Colors.LightPink);
                this.DiscountBox.Text = string.Empty;
                this.AddItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.ActiveSubstancesDict.Count == 0)
            {
                this.SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                this.SubstanceNameBox.Text = string.Empty;
                this.ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                this.ConcentrationBox.Text = string.Empty;
                this.AddItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void ResetAddItemErrors()
        {
            this.NameBox.Background = new SolidColorBrush(Colors.White);
            this.ProducerBox.Background = new SolidColorBrush(Colors.White);
            this.CategoryBox.Background = new SolidColorBrush(Colors.White);
            this.PriceBox.Background = new SolidColorBrush(Colors.White);
            this.NumberOfPillsBox.Background = new SolidColorBrush(Colors.White);
            this.DiscountBox.Background = new SolidColorBrush(Colors.White);
            this.SubstanceNameBox.Background = new SolidColorBrush(Colors.White);
            this.ConcentrationBox.Background = new SolidColorBrush(Colors.White);
            this.AddItemMandatoryError.Visibility = Visibility.Collapsed;
            this.AddItemFormatError.Visibility = Visibility.Collapsed;
        }

        private void RefreshActiveSubstancesList()
        {
            var list = this.ActiveSubstancesDict
                .Select(kvp => new ActiveSubstance
                {
                    Name = kvp.Key,
                    Concentration = kvp.Value,
                })
                .ToList();

            this.ActiveSubstancesList.ItemsSource = list;
        }

        private void AddSubstance_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddItemSubstance())
            {
                return;
            }

            string name = this.SubstanceNameBox.Text;
            float concentration = float.Parse(this.ConcentrationBox.Text);

            this.ActiveSubstancesDict[name] = concentration;

            this.RefreshActiveSubstancesList();

            this.SubstanceNameBox.Text = string.Empty;
            this.ConcentrationBox.Text = string.Empty;
            this.ResetActiveSubstanceErrors();
        }

        private bool ValidateAddItemSubstance()
        {
            bool isValid = true;
            this.AddActiveSubstanceToItemInvalidError.Text = "Invalid input!";

            if (this.SubstanceNameBox.Text == string.Empty)
            {
                this.SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                this.SubstanceNameBox.Text = string.Empty;
                this.AddActiveSubstanceToItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.ConcentrationBox.Text == string.Empty)
            {
                this.ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                this.ConcentrationBox.Text = string.Empty;
                this.AddActiveSubstanceToItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(this.ConcentrationBox.Text, out float concentration))
            {
                this.ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                this.ConcentrationBox.Text = string.Empty;
                this.AddActiveSubstanceToItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (isValid)
            {
                if (!this.ViewModel.TryValidateActiveSubstance(this.SubstanceNameBox.Text, concentration, this.ActiveSubstancesDict, out string errorMessage))
                {
                    this.SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                    this.SubstanceNameBox.Text = string.Empty;
                    this.ConcentrationBox.Background = new SolidColorBrush(Colors.LightPink);
                    this.ConcentrationBox.Text = string.Empty;
                    this.AddActiveSubstanceToItemInvalidError.Text = errorMessage;
                    this.AddActiveSubstanceToItemInvalidError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void RemoveSubstance_Click(object sender, RoutedEventArgs e)
        {
            string name = this.SubstanceNameBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                this.SubstanceNameBox.Background = new SolidColorBrush(Colors.LightPink);
                this.RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            if (this.ActiveSubstancesDict.ContainsKey(name))
            {
                this.ActiveSubstancesDict.Remove(name);
            }
            else
            {
                this.RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            this.RefreshActiveSubstancesListUpdate();

            this.SubstanceNameBox.Text = string.Empty;
            this.ConcentrationBox.Text = string.Empty;
            this.ResetActiveSubstanceErrors();
        }

        private void ResetActiveSubstanceErrors()
        {
            this.SubstanceNameBox.Background = new SolidColorBrush(Colors.White);
            this.ConcentrationBox.Background = new SolidColorBrush(Colors.White);
            this.AddActiveSubstanceToItemMandatoryError.Visibility = Visibility.Collapsed;
            this.AddActiveSubstanceToItemFormatError.Visibility = Visibility.Collapsed;
            this.RemoveActiveSubstanceFromItemError.Visibility = Visibility.Collapsed;
            this.AddActiveSubstanceToItemInvalidError.Visibility = Visibility.Collapsed;
        }

        private void RefreshBatchesList()
        {
            var list = this.BatchesDict
                .Select(kvp => new BatchItem
                {
                    Date = kvp.Key,
                    Packs = kvp.Value,
                })
                .OrderBy(x => x.Date)
                .ToList();

            this.BatchesList.ItemsSource = list;
        }

        private void AddBatch_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddBatch())
            {
                return;
            }

            int packs = int.Parse(this.PacksBox.Text);
            DateOnly date = DateOnly.FromDateTime(this.BatchDatePicker.Date.Date);

            this.BatchesDict[date] = packs;

            this.RefreshBatchesList();

            this.PacksBox.Text = string.Empty;
            this.ResetAddBatchErrors();
        }

        private bool ValidateAddBatch()
        {
            bool isValid = true;
            this.AddBatchFormatError.Text = "Wrong Format!";

            DateOnly selectedDate = DateOnly.FromDateTime(this.BatchDatePicker.Date.Date);
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (this.PacksBox.Text == string.Empty)
            {
                this.PacksBox.Background = new SolidColorBrush(Colors.LightPink);
                this.AddBatchMandatoryError.Visibility = Visibility.Visible;
                this.PacksBox.Text = string.Empty;
                isValid = false;
            }

            if (!int.TryParse(this.PacksBox.Text, out int packs))
            {
                this.PacksBox.Background = new SolidColorBrush(Colors.LightPink);
                this.AddBatchFormatError.Text = "invalid quantity";
                this.AddBatchFormatError.Visibility = Visibility.Visible;
                this.PacksBox.Text = string.Empty;
                isValid = false;
            }

            if (isValid)
            {
                if (!this.ViewModel.TryValidateBatch(selectedDate, packs, currentDate, out string errorMessage))
                {
                    this.AddBatchFormatError.Text = errorMessage;
                    this.AddBatchFormatError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        public void ResetAddBatchErrors()
        {
            this.BatchDatePicker.Background = new SolidColorBrush(Colors.White);
            this.PacksBox.Background = new SolidColorBrush(Colors.White);
            this.AddBatchMandatoryError.Visibility = Visibility.Collapsed;
            this.AddBatchFormatError.Visibility = Visibility.Collapsed;
            this.RemoveBatchFromItemError.Visibility = Visibility.Collapsed;
        }

        private void RemoveBatchFromItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedBatch = this.BatchesList.SelectedItem as BatchItem;

            if (selectedBatch != null)
            {
                if (this.BatchesDict.ContainsKey(selectedBatch.Date))
                {
                    this.BatchesDict.Remove(selectedBatch.Date);
                    this.RefreshBatchesList();
                }
            }
            else
            {
                this.RemoveBatchFromItemError.Visibility = Visibility.Visible;
            }
        }

        private void OnItemCancelClick(object sender, RoutedEventArgs e)
        {
            this.ClearItemAddBoxes();
            this.ClearItemUpdateBoxes();
            this.ActiveSubstancesDict.Clear();
            this.BatchesDict.Clear();
            this.RefreshActiveSubstancesList();
            this.RefreshActiveSubstancesListUpdate();
            this.RefreshBatchesList();
            this.RefreshBatchesListUpdate();
            this.isGetItemDataClicked = false;

            this.ResetAddItemErrors();
            this.ResetUpdateItemErrors();
            this.ResetActiveSubstanceErrors();
            this.ResetAddBatchErrors();

            this.AddItemGrid.Visibility = Visibility.Collapsed;
            this.UpdateItemGrid.Visibility = Visibility.Collapsed;
        }

        private void OnItemRemoveClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.ItemList.SelectedItem as Item;

            if (selectedItem == null)
            {
                this.RemoveItemError.Visibility = Visibility.Visible;
                return;
            }

            int id = selectedItem.Id;

            this.ViewModel.RemoveItemById(id);
            this.ViewModel.RefreshItems();
            this.ItemList.ItemsSource = this.ViewModel.Items;
            this.RemoveItemError.Visibility = Visibility.Collapsed;
        }

        private void OnItemUpdateClick(object sender, RoutedEventArgs e)
        {
            this.RemoveUpdateActiveSubstanceError.Visibility = Visibility.Collapsed;

            if (this.UpdateItemGrid.Visibility == Visibility.Visible)
            {
                this.UpdateItemGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.UpdateItemGrid.Visibility = Visibility.Visible;
                this.AddItemGrid.Visibility = Visibility.Collapsed;
                this.ResetUpdateItemErrors();
            }
        }

        private void OnGetItemDataClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateGetItemData())
            {
                return;
            }

            Item item = this.ViewModel.GetItemById(int.Parse(this.IdBox.Text));
            this.NameBoxUpdate.Text = item.Name;
            this.ProducerBoxUpdate.Text = item.Producer;
            this.PriceBoxUpdate.Text = item.Price.ToString();
            this.CategoryBoxUpdate.Text = item.Category;
            this.ImagePathBoxUpdate.Text = item.ImagePath;
            this.NumberOfPillsBoxUpdate.Text = item.NumberOfPills.ToString();
            this.LabelBoxUpdate.Text = item.Label;
            this.DescriptionBoxUpdate.Text = item.Description;
            this.DiscountBoxUpdate.Text = item.DiscountPercentage.ToString();

            this.ActiveSubstancesDict = item.ActiveSubstances;
            this.RefreshActiveSubstancesListUpdate();
            this.BatchesDict = item.Batches;
            this.RefreshBatchesListUpdate();

            this.ResetUpdateItemErrors();
            this.isGetItemDataClicked = true;
        }

        private bool ValidateGetItemData()
        {
            bool isValid = true;

            if (this.IdBox.Text == string.Empty)
            {
                this.IdBox.Background = new SolidColorBrush(Colors.LightPink);
                this.IdBox.Text = string.Empty;
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!int.TryParse(this.IdBox.Text, out int id))
            {
                this.IdBox.Background = new SolidColorBrush(Colors.LightPink);
                this.IdBox.Text = string.Empty;
                this.UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                try
                {
                    this.ViewModel.GetItemById(id);
                }
                catch (Exception)
                {
                    this.IdBox.Background = new SolidColorBrush(Colors.LightPink);
                    this.IdBox.Text = string.Empty;
                    this.UpdateInvalidIdError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void OnUpdateItemClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateUpdateItem())
            {
                return;
            }

            int id = int.Parse(this.IdBox.Text);

            string name = this.NameBoxUpdate.Text;
            string producer = this.ProducerBoxUpdate.Text;
            string category = this.CategoryBoxUpdate.Text;
            string imagePath = this.ImagePathBoxUpdate.Text;
            string label = this.LabelBoxUpdate.Text;
            string description = this.DescriptionBoxUpdate.Text;
            float price = float.Parse(this.PriceBoxUpdate.Text);
            float discount = 0f;

            if (this.DiscountBoxUpdate.Text != string.Empty)
            {
                discount = float.Parse(this.DiscountBoxUpdate.Text);
            }

            int numberOfPills = int.Parse(this.NumberOfPillsBoxUpdate.Text);

            this.ViewModel.UpdateItemFromDetails(
                id,
                name,
                producer,
                category,
                price,
                numberOfPills,
                label,
                description,
                imagePath,
                discount,
                this.ActiveSubstancesDict,
                this.BatchesDict);

            this.ViewModel.RefreshItems();
            this.ItemList.ItemsSource = this.ViewModel.Items;
            this.ClearItemUpdateBoxes();
            this.ActiveSubstancesDict.Clear();
            this.RefreshActiveSubstancesListUpdate();
            this.BatchesDict.Clear();
            this.RefreshBatchesListUpdate();
            this.ResetUpdateItemErrors();
        }

        private bool ValidateUpdateItem()
        {
            bool isValid = true;

            if (!this.isGetItemDataClicked)
            {
                this.UpdateItemGetDataError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.NameBoxUpdate.Text == string.Empty)
            {
                this.NameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.NameBoxUpdate.Text = string.Empty;
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.ProducerBoxUpdate.Text == string.Empty)
            {
                this.ProducerBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.ProducerBoxUpdate.Text = string.Empty;
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.CategoryBoxUpdate.Text == string.Empty)
            {
                this.CategoryBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.CategoryBoxUpdate.Text = string.Empty;
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.IdBox.Text == string.Empty)
            {
                this.IdBox.Background = new SolidColorBrush(Colors.LightPink);
                this.IdBox.Text = string.Empty;
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.PriceBoxUpdate.Text == string.Empty)
            {
                this.PriceBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
            }

            if (!float.TryParse(this.PriceBoxUpdate.Text, out float price))
            {
                this.PriceBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.PriceBoxUpdate.Text = string.Empty;
                this.UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.NumberOfPillsBoxUpdate.Text == string.Empty)
            {
                this.NumberOfPillsBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(this.NumberOfPillsBoxUpdate.Text, out int numberOfPills))
            {
                this.NumberOfPillsBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.NumberOfPillsBoxUpdate.Text = string.Empty;
                this.UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(this.DiscountBoxUpdate.Text, out float discount) && this.DiscountBoxUpdate.Text != string.Empty)
            {
                this.DiscountBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.DiscountBoxUpdate.Text = string.Empty;
                this.UpdateItemFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.ActiveSubstancesDict.Count == 0)
            {
                this.SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.SubstanceNameBoxUpdate.Text = string.Empty;
                this.ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.ConcentrationBoxUpdate.Text = string.Empty;
                this.UpdateItemMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void ResetUpdateItemErrors()
        {
            this.IdBox.Background = new SolidColorBrush(Colors.White);
            this.NameBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.ProducerBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.CategoryBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.PriceBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.NumberOfPillsBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.DiscountBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.UpdateItemMandatoryError.Visibility = Visibility.Collapsed;
            this.UpdateItemFormatError.Visibility = Visibility.Collapsed;
            this.UpdateInvalidIdError.Visibility = Visibility.Collapsed;
            this.UpdateItemGetDataError.Visibility = Visibility.Collapsed;
            this.RemoveUpdateActiveSubstanceError.Visibility = Visibility.Collapsed;
            this.UpdateActiveSubstanceMandatoryError.Visibility = Visibility.Collapsed;
            this.UpdateActiveSubstanceFormatError.Visibility = Visibility.Collapsed;
            this.UpdateActiveSubstanceInvalidError.Visibility = Visibility.Collapsed;
            this.BatchDatePickerUpdate.Background = new SolidColorBrush(Colors.White);
            this.PacksBoxUpdate.Background = new SolidColorBrush(Colors.White);
            this.UpdateBatchMandatoryError.Visibility = Visibility.Collapsed;
            this.UpdateBatchFormatError.Visibility = Visibility.Collapsed;
            this.RemoveUpdateBatchError.Visibility = Visibility.Collapsed;
        }

        private void OnSubstanceAddClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ToggleAddSubstanceGrid();
            this.ResetSubstanceErrors();
            this.ApplyUiStateFromViewModel();
        }

        private void OnSubstanceUpdateClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ToggleUpdateSubstanceGrid();
            this.ResetSubstanceErrors();
            this.ApplyUiStateFromViewModel();
        }

        private void RefreshActiveSubstancesListUpdate()
        {
            var list = this.ActiveSubstancesDict
                .Select(kvp => new ActiveSubstance
                {
                    Name = kvp.Key,
                    Concentration = kvp.Value,
                })
                .ToList();

            this.ActiveSubstancesListUpdate.ItemsSource = list;
        }

        private void AddSubstanceUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateUpdateItemSubstance())
            {
                return;
            }

            string name = this.SubstanceNameBoxUpdate.Text;
            float concentration = float.Parse(this.ConcentrationBoxUpdate.Text);

            this.ActiveSubstancesDict[name] = concentration;

            this.RefreshActiveSubstancesListUpdate();

            this.SubstanceNameBoxUpdate.Text = string.Empty;
            this.ConcentrationBoxUpdate.Text = string.Empty;
            this.ResetUpdateItemErrors();
        }

        private bool ValidateUpdateItemSubstance()
        {
            bool isValid = true;
            this.UpdateActiveSubstanceInvalidError.Text = "Invalid input!";

            if (this.SubstanceNameBoxUpdate.Text == string.Empty)
            {
                this.SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.SubstanceNameBoxUpdate.Text = string.Empty;
                this.UpdateActiveSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.ConcentrationBoxUpdate.Text == string.Empty)
            {
                this.ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.ConcentrationBoxUpdate.Text = string.Empty;
                this.UpdateActiveSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!float.TryParse(this.ConcentrationBoxUpdate.Text, out float concentration))
            {
                this.ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.ConcentrationBoxUpdate.Text = string.Empty;
                this.UpdateActiveSubstanceFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (isValid)
            {
                if (!this.ViewModel.TryValidateActiveSubstance(this.SubstanceNameBoxUpdate.Text, concentration, this.ActiveSubstancesDict, out string errorMessage))
                {
                    this.SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                    this.SubstanceNameBoxUpdate.Text = string.Empty;
                    this.ConcentrationBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                    this.ConcentrationBoxUpdate.Text = string.Empty;
                    this.UpdateActiveSubstanceInvalidError.Text = errorMessage;
                    this.UpdateActiveSubstanceInvalidError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void RemoveSubstanceUpdate_Click(object sender, RoutedEventArgs e)
        {
            string name = this.SubstanceNameBoxUpdate.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                this.SubstanceNameBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            if (this.ActiveSubstancesDict.ContainsKey(name))
            {
                this.ActiveSubstancesDict.Remove(name);
            }
            else
            {
                this.RemoveUpdateActiveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            this.RefreshActiveSubstancesListUpdate();

            this.SubstanceNameBoxUpdate.Text = string.Empty;
            this.ConcentrationBoxUpdate.Text = string.Empty;
            this.ResetUpdateItemErrors();
        }

        private void RefreshBatchesListUpdate()
        {
            var list = this.BatchesDict
                .Select(kvp => new BatchItem
                {
                    Date = kvp.Key,
                    Packs = kvp.Value,
                })
                .OrderBy(x => x.Date)
                .ToList();

            this.BatchesListUpdate.ItemsSource = list;
        }

        private void AddBatchUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateUpdateBatch())
            {
                return;
            }

            int packs = int.Parse(this.PacksBoxUpdate.Text);
            DateOnly date = DateOnly.FromDateTime(this.BatchDatePickerUpdate.Date.Date);

            this.BatchesDict[date] = packs;

            this.RefreshBatchesListUpdate();

            this.PacksBoxUpdate.Text = string.Empty;
            this.ResetUpdateItemErrors();
        }

        private bool ValidateUpdateBatch()
        {
            bool isValid = true;
            this.UpdateBatchFormatError.Text = "Wrong Format!";

            DateOnly selectedDate = DateOnly.FromDateTime(this.BatchDatePickerUpdate.Date.Date);
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (this.PacksBoxUpdate.Text == string.Empty)
            {
                this.PacksBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.UpdateBatchMandatoryError.Visibility = Visibility.Visible;
                this.PacksBoxUpdate.Text = string.Empty;
                isValid = false;
            }

            if (!int.TryParse(this.PacksBoxUpdate.Text, out int packs))
            {
                this.PacksBoxUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.UpdateBatchFormatError.Text = "invalid quantity";
                this.UpdateBatchFormatError.Visibility = Visibility.Visible;
                this.PacksBoxUpdate.Text = string.Empty;
                isValid = false;
            }

            if (isValid)
            {
                if (!this.ViewModel.TryValidateBatch(selectedDate, packs, currentDate, out string errorMessage))
                {
                    this.UpdateBatchFormatError.Text = errorMessage;
                    this.UpdateBatchFormatError.Visibility = Visibility.Visible;
                    isValid = false;
                }
            }

            return isValid;
        }

        private void RemoveBatchFromItemUpdate_Click(object sender, RoutedEventArgs e)
        {
            var selectedBatch = this.BatchesListUpdate.SelectedItem as BatchItem;

            if (selectedBatch != null)
            {
                if (this.BatchesDict.ContainsKey(selectedBatch.Date))
                {
                    this.BatchesDict.Remove(selectedBatch.Date);
                    this.RefreshBatchesListUpdate();
                }
            }
            else
            {
                this.RemoveUpdateBatchError.Visibility = Visibility.Visible;
            }
        }

        private void ResetSubstanceErrors()
        {
            this.NameBoxSubstance.Background = new SolidColorBrush(Colors.White);
            this.LethalDoseBoxSubstance.Background = new SolidColorBrush(Colors.White);
            this.DescriptionBoxSubstance.Background = new SolidColorBrush(Colors.White);
            this.NameBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.White);
            this.LethalDoseBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.White);
            this.DescriptionBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.White);
            this.RemoveSubstanceError.Visibility = Visibility.Collapsed;
            this.AddSubstanceFormatError.Visibility = Visibility.Collapsed;
            this.AddSubstanceMandatoryError.Visibility = Visibility.Collapsed;
            this.UpdateSubstanceFormatError.Visibility = Visibility.Collapsed;
            this.UpdateSubstanceMandatoryError.Visibility = Visibility.Collapsed;
            this.UpdateSubstanceInvalidError.Visibility = Visibility.Collapsed;
        }

        private void OnSubstanceRemoveClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.SubstanceList.SelectedItem as Substance;

            if (selectedItem == null)
            {
                this.RemoveSubstanceError.Visibility = Visibility.Visible;
                return;
            }

            this.ViewModel.RemoveSubstanceByName(selectedItem);
            this.ViewModel.RefreshSubstances();
            this.SubstanceList.ItemsSource = this.ViewModel.Substances;
            this.ResetSubstanceErrors();
        }

        private void OnSubstanceCancelClick(object sender, RoutedEventArgs e)
        {
            this.ClearSubstanceBoxes();
            this.ClearSubstanceUpdateBoxes();
            this.ViewModel.AddSubstanceGridVisibility = Visibility.Collapsed;
            this.ViewModel.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            this.ApplyUiStateFromViewModel();
            this.ResetSubstanceErrors();
        }

        private void OnAddSubstanceClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateAddSubstance())
            {
                return;
            }

            string name = this.NameBoxSubstance.Text;
            int lethalDose = int.Parse(this.LethalDoseBoxSubstance.Text);
            string description = this.DescriptionBoxSubstance.Text;

            Substance newSubstance = new Substance(name, lethalDose, description);

            this.ViewModel.AddSubstance(newSubstance);
            System.Diagnostics.Debug.WriteLine("Added substance");

            this.ClearSubstanceBoxes();
            this.ViewModel.RefreshSubstances();
            this.SubstanceList.ItemsSource = this.ViewModel.Substances;
            this.ResetSubstanceErrors();
        }

        private bool ValidateAddSubstance()
        {
            bool isValid = true;

            if (this.NameBoxSubstance.Text == string.Empty)
            {
                this.NameBoxSubstance.Background = new SolidColorBrush(Colors.LightPink);
                this.NameBoxSubstance.Text = string.Empty;
                this.AddSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.LethalDoseBoxSubstance.Text == string.Empty)
            {
                this.LethalDoseBoxSubstance.Background = new SolidColorBrush(Colors.LightPink);
                this.LethalDoseBoxSubstance.Text = string.Empty;
                this.AddSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(this.LethalDoseBoxSubstance.Text, out int lethalDose))
            {
                this.LethalDoseBoxSubstance.Background = new SolidColorBrush(Colors.LightPink);
                this.LethalDoseBoxSubstance.Text = string.Empty;
                this.AddSubstanceFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }

        private void OnUpdateSubstanceClick(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateUpdateSubstance())
            {
                return;
            }

            string name = this.NameBoxSubstanceUpdate.Text;
            int lethalDose = int.Parse(this.LethalDoseBoxSubstanceUpdate.Text);
            string description = this.DescriptionBoxSubstanceUpdate.Text;

            Substance updatedSubstance = new Substance(name, lethalDose, description);

            this.ViewModel.UpdateSubstanceByName(name, updatedSubstance);

            System.Diagnostics.Debug.WriteLine("Updated substance");

            this.ClearSubstanceUpdateBoxes();
            this.ViewModel.RefreshSubstances();
            this.SubstanceList.ItemsSource = this.ViewModel.Substances;
            this.ResetSubstanceErrors();
        }

        private bool ValidateUpdateSubstance()
        {
            bool isValid = true;

            if (this.NameBoxSubstanceUpdate.Text == string.Empty)
            {
                this.NameBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.NameBoxSubstanceUpdate.Text = string.Empty;
                this.UpdateSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!this.ViewModel.SubstanceExists(this.NameBoxSubstanceUpdate.Text))
            {
                this.NameBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.NameBoxSubstanceUpdate.Text = string.Empty;
                this.UpdateSubstanceInvalidError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (this.LethalDoseBoxSubstanceUpdate.Text == string.Empty)
            {
                this.LethalDoseBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.LethalDoseBoxSubstanceUpdate.Text = string.Empty;
                this.UpdateSubstanceMandatoryError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(this.LethalDoseBoxSubstanceUpdate.Text, out int lethalDose))
            {
                this.LethalDoseBoxSubstanceUpdate.Background = new SolidColorBrush(Colors.LightPink);
                this.LethalDoseBoxSubstanceUpdate.Text = string.Empty;
                this.UpdateSubstanceFormatError.Visibility = Visibility.Visible;
                isValid = false;
            }

            return isValid;
        }
    }
}