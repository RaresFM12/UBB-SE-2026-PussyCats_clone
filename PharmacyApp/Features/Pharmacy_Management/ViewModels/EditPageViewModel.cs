using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using PharmacyApp.Common.Services;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Pharmacy_Management.ViewModels
{
    public class EditPageViewModel : INotifyPropertyChanged
    {
        private readonly IAdminService adminService;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>();

        public ObservableCollection<Substance> Substances { get; } = new ObservableCollection<Substance>();

        private Visibility itemListButtonsVisibility = Visibility.Visible;

        public Visibility ItemListButtonsVisibility
        {
            get => this.itemListButtonsVisibility;
            private set
            {
                this.itemListButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility itemBottomButtonsVisibility = Visibility.Visible;

        public Visibility ItemBottomButtonsVisibility
        {
            get => this.itemBottomButtonsVisibility;
            private set
            {
                this.itemBottomButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility showExpiredItemsToggleVisibility = Visibility.Visible;

        public Visibility ShowExpiredItemsToggleVisibility
        {
            get => this.showExpiredItemsToggleVisibility;
            private set
            {
                this.showExpiredItemsToggleVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility substanceListButtonsVisibility = Visibility.Collapsed;

        public Visibility SubstanceListButtonsVisibility
        {
            get => this.substanceListButtonsVisibility;
            private set
            {
                this.substanceListButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility substanceBottomButtonsVisibility = Visibility.Collapsed;

        public Visibility SubstanceBottomButtonsVisibility
        {
            get => this.substanceBottomButtonsVisibility;
            private set
            {
                this.substanceBottomButtonsVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility addSubstanceGridVisibility = Visibility.Collapsed;

        public Visibility AddSubstanceGridVisibility
        {
            get => this.addSubstanceGridVisibility;
            set
            {
                this.addSubstanceGridVisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility updateSubstanceGridVisibility = Visibility.Collapsed;

        public Visibility UpdateSubstanceGridVisibility
        {
            get => this.updateSubstanceGridVisibility;
            set
            {
                this.updateSubstanceGridVisibility = value;
                this.OnPropertyChanged();
            }
        }

        public EditPageViewModel()
            : this(new AdminService())
        {
        }

        public EditPageViewModel(IAdminService adminService)
        {
            this.adminService = adminService;
            this.RefreshItems();
            this.RefreshSubstances();
        }

        public void RefreshItems()
        {
            this.Items.Clear();
            foreach (Item item in this.adminService.GetAllItems())
            {
                this.Items.Add(item);
            }
        }

        public void RefreshSubstances()
        {
            this.Substances.Clear();
            foreach (Substance substance in this.adminService.GetAllSubstances())
            {
                this.Substances.Add(substance);
            }
        }

        public void SearchItems(string query)
        {
            this.Items.Clear();
            foreach (Item item in this.adminService.SearchItemsByName(query))
            {
                this.Items.Add(item);
            }
        }

        public void ShowExpiredItems()
        {
            this.Items.Clear();
            foreach (Item item in this.adminService.GetExpiredItems())
            {
                this.Items.Add(item);
            }
        }

        public Item GetItemById(int id) => this.adminService.GetItemById(id);

        public Substance GetSubstanceByName(string name) => this.adminService.GetSubstanceByName(name);

        public bool SubstanceExists(string name) => this.adminService.SubstanceExists(name);

        public void AddItemWithQuantity(Item item) => this.adminService.AddItemWithQuantity(item);

        public void AddItemFromDetails(
            string name,
            string producer,
            string category,
            float price,
            int numberOfPills,
            string label,
            string description,
            string imagePath,
            float discount,
            Dictionary<string, float> activeSubstances,
            Dictionary<DateOnly, int> batches)
        {
            this.adminService.AddItemFromDetails(
                name,
                producer,
                category,
                price,
                numberOfPills,
                label,
                description,
                imagePath,
                discount,
                activeSubstances,
                batches);
        }

        public void UpdateItemById(int id, Item item) => this.adminService.UpdateItemById(id, item);

        public void UpdateItemFromDetails(
            int id,
            string name,
            string producer,
            string category,
            float price,
            int numberOfPills,
            string label,
            string description,
            string imagePath,
            float discount,
            Dictionary<string, float> activeSubstances,
            Dictionary<DateOnly, int> batches)
        {
            this.adminService.UpdateItemFromDetails(
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
                activeSubstances,
                batches);
        }

        public void RemoveItemById(int id) => this.adminService.RemoveItemById(id);

        public void AddSubstance(Substance substance) => this.adminService.AddSubstance(substance);

        public void UpdateSubstanceByName(string name, Substance substance) => this.adminService.UpdateSubstanceByName(name, substance);

        public void RemoveSubstanceByName(Substance substance) => this.adminService.RemoveSubstanceByName(substance);

        public bool TryValidateActiveSubstance(
            string substanceName,
            float concentration,
            IReadOnlyDictionary<string, float> activeSubstances,
            out string errorMessage)
        {
            return this.adminService.TryValidateActiveSubstance(substanceName, concentration, activeSubstances, out errorMessage);
        }

        public bool TryValidateBatch(
            DateOnly expirationDate,
            int packs,
            DateOnly currentDate,
            out string errorMessage)
        {
            return this.adminService.TryValidateBatch(expirationDate, packs, currentDate, out errorMessage);
        }

        public void ActivateItemsSection()
        {
            this.ItemListButtonsVisibility = Visibility.Visible;
            this.ItemBottomButtonsVisibility = Visibility.Visible;
            this.ShowExpiredItemsToggleVisibility = Visibility.Visible;

            this.SubstanceListButtonsVisibility = Visibility.Collapsed;
            this.SubstanceBottomButtonsVisibility = Visibility.Collapsed;
            this.AddSubstanceGridVisibility = Visibility.Collapsed;
            this.UpdateSubstanceGridVisibility = Visibility.Collapsed;
        }

        public void ActivateSubstancesSection()
        {
            this.ItemListButtonsVisibility = Visibility.Collapsed;
            this.ItemBottomButtonsVisibility = Visibility.Collapsed;
            this.ShowExpiredItemsToggleVisibility = Visibility.Collapsed;

            this.SubstanceListButtonsVisibility = Visibility.Visible;
            this.SubstanceBottomButtonsVisibility = Visibility.Visible;
        }

        public void ToggleAddSubstanceGrid()
        {
            if (this.AddSubstanceGridVisibility == Visibility.Visible)
            {
                this.AddSubstanceGridVisibility = Visibility.Collapsed;
            }
            else
            {
                this.AddSubstanceGridVisibility = Visibility.Visible;
                this.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            }
        }

        public void ToggleUpdateSubstanceGrid()
        {
            if (this.UpdateSubstanceGridVisibility == Visibility.Visible)
            {
                this.UpdateSubstanceGridVisibility = Visibility.Collapsed;
            }
            else
            {
                this.UpdateSubstanceGridVisibility = Visibility.Visible;
                this.AddSubstanceGridVisibility = Visibility.Collapsed;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}