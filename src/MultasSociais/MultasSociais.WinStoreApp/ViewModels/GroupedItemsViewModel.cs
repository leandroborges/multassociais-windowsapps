﻿using System.Collections.Generic;
using Caliburn.Micro;
using MultasSociais.WinStoreApp.DataModel;
using MultasSociais.WinStoreApp.Views;
using Windows.UI.Xaml.Controls;

namespace MultasSociais.WinStoreApp.ViewModels
{
    public class GroupedItemsViewModel : ViewModelBase
    {
        public GroupedItemsViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        protected override void OnInitialize()
        {
            Groups = new BindableCollection<SampleDataGroup>(SampleDataSource.GetGroups(Parameter));
            base.OnInitialize();
        }

        private BindableCollection<SampleDataGroup> groups;

        public string Parameter { get; set; }

        public BindableCollection<SampleDataGroup> Groups
        {
            set
            {
                groups = value;
                NotifyOfPropertyChange();
            }
            get
            {
                return groups;
            }
        }

        public void GoToHeader(SampleDataGroup sampleDataGroup)
        {
            navigationService.Navigate<GroupDetailView>(sampleDataGroup.UniqueId);
        }
        public void GoToItem(SampleDataItem sampleDataItem)
        {
            navigationService.Navigate<ItemDetailView>(sampleDataItem.UniqueId);
        }
    }
}
