﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    class PasswordViewModel: BaseViewModel
    {
        public PasswordViewModel()
        {
            
        }

        #region Methods

        public bool CheckPassword()
        {
            if (String.IsNullOrWhiteSpace(Password)) return false;

            string hashValue = CryptoService.HashData(Password);
            if (!hashValue.Equals(SettingsService.LoadSetting<string>(SettingsResources.UserPinLock)))
            {
                new CustomMessageDialog(
                        AppMessages.WrongPassword_Title,
                        AppMessages.WrongPassword,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                return false;
            }

            return true;
        }

        public void Logout()
        {
            SdkService.MegaSdk.logout(new LogOutRequestListener());
        }

        #endregion

        #region Properties

        public string Password { get; set; }        

        #endregion
    }
}
