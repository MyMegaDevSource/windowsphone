﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;
using Microsoft.Phone.Shell;
using IApplicationBar = MegaApp.Interfaces.IApplicationBar;

namespace MegaApp.Models
{
    public class BaseAppInfoAwareViewModel : BaseSdkViewModel, IHamburgerMenu, IApplicationBar
    {
        public BaseAppInfoAwareViewModel(MegaSDK megaSdk, AppInformation appInformation): base(megaSdk)
        {
            this.AppInformation = appInformation;
            this.MenuItems = new List<HamburgerMenuItem>();            
        }

        protected void UpdateUserData()
        {
            if (Convert.ToBoolean(App.MegaSdk.isLoggedIn()))
            {
                if (App.UserData == null)
                    App.UserData = new UserDataViewModel { UserEmail = App.MegaSdk.getMyEmail() };

                if (String.IsNullOrEmpty(App.UserData.AvatarPath) || App.UserData.AvatarUri == null)
                    App.MegaSdk.getUserAvatar(App.MegaSdk.getContact(App.MegaSdk.getMyEmail()), App.UserData.AvatarPath, new GetUserAvatarRequestListener(App.UserData));

                if (String.IsNullOrEmpty(App.UserData.UserEmail))
                    App.UserData.UserEmail = App.MegaSdk.getMyEmail();
                if (String.IsNullOrEmpty(App.UserData.UserName))
                    App.MegaSdk.getUserData(new GetUserDataRequestListener(App.UserData));

                UserData = App.UserData;
            }
            else
            {
                if (UserData == null)
                    UserData = new UserDataViewModel();
                
                Deployment.Current.Dispatcher.BeginInvoke(() => UserData.UserName = UiResources.MyAccount);
            }
        }

        #region Properties

        public AppInformation AppInformation { get; private set; }

        private UserDataViewModel _userData;
        public UserDataViewModel UserData
        {
            get { return _userData; }
            set
            {
                _userData = value;
                OnPropertyChanged("UserData");
            }
        }        

        #endregion

        #region IHamburgerMenu

        public IList<HamburgerMenuItem> MenuItems { get; set; }

        protected void InitializeMenu(HamburgerMenuItemType activeItem)
        {
            bool[] activeItemsArray = new bool[6];
            
            switch(activeItem)
            {
                case HamburgerMenuItemType.CloudDrive:
                    activeItemsArray[0] = true;
                    break;
                case HamburgerMenuItemType.CameraUploads:
                    activeItemsArray[1] = true;
                    break;
                case HamburgerMenuItemType.SharedItems:
                    activeItemsArray[2] = true;
                    break;
                case HamburgerMenuItemType.Contacts:
                    activeItemsArray[3] = true;
                    break;
                case HamburgerMenuItemType.Transfers:
                    activeItemsArray[4] = true;
                    break;
                case HamburgerMenuItemType.Settings:
                    activeItemsArray[5] = true;
                    break;
                default:
                    break;
            }

            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.CloudDrive,
                DisplayName = UiResources.CloudDriveName.ToLower(),
                IconPathData = VisualResources.CloudDriveMenuPathData,
                IconWidth = 48,
                IconHeight = 34,
                Margin = new Thickness(36, 0, 35, 0),
                TapAction = () => { NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal); },
                IsActive = activeItemsArray[0]
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.CameraUploads,
                DisplayName = UiResources.CameraUploads.ToLower(),
                IconPathData = VisualResources.CameraUploadsPathData,
                IconWidth = 46,
                IconHeight = 36,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () => { },
                IsActive = activeItemsArray[1]
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.SharedItems,
                DisplayName = UiResources.SharedItems.ToLower(),
                IconPathData = VisualResources.SharedItemsPathData,
                IconWidth = 45,
                IconHeight = 36,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () => { },
                IsActive = activeItemsArray[2]
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.Contacts,
                DisplayName = UiResources.Contacts.ToLower(),
                IconPathData = VisualResources.ContactsPathData,
                IconWidth = 45,
                IconHeight = 33,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () => { },
                IsActive = activeItemsArray[3]
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.Transfers,
                DisplayName = UiResources.Transfers.ToLower(),
                IconPathData = VisualResources.TransfersPathData,
                IconWidth = 44,
                IconHeight = 44,
                Margin = new Thickness(38, 0, 36, 0),
                TapAction = () => { NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal); },
                IsActive = activeItemsArray[4]
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.Settings,
                DisplayName = UiResources.Settings.ToLower(),
                IconPathData = VisualResources.SettingsPathData,
                IconWidth = 45,
                IconHeight = 45,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () => { NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal); },
                IsActive = activeItemsArray[5]
            });
        }

        public bool CheckHamburgerMenu(DrawerLayout MainDrawerLayout, bool isCancel)
        {
            if (isCancel) return true;
            if (!MainDrawerLayout.IsDrawerOpen) return false;
            MainDrawerLayout.CloseDrawer();
            return true;
        }

        #endregion

        #region IApplicationBar

        public void TranslateAppBarItems(IList<ApplicationBarIconButton> iconButtons, 
            IList<ApplicationBarMenuItem> menuItems, IList<string> iconStrings, IList<string> menuStrings)
        {
            for (var i = 0; i < iconButtons.Count; i++)
            {
                if (iconStrings[i] == null) throw new IndexOutOfRangeException("iconStrings");

                iconButtons[i].Text = iconStrings[i].ToLower(); 
            }

            for (var i = 0; i < menuItems.Count; i++)
            {
                if (menuStrings[i] == null) throw new IndexOutOfRangeException("menuStrings");

                menuItems[i].Text = menuStrings[i].ToLower();
            }
        }

        #endregion
    }
}