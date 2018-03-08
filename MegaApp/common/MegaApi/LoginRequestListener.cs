﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class LoginRequestListener : BaseRequestListener
    {
        private readonly LoginViewModel _loginViewModel;
        private readonly LoginPage _loginPage;

        // Timer for ignore the received API_EAGAIN (-3) during login
        private DispatcherTimer timerAPI_EAGAIN;
        private bool isFirstAPI_EAGAIN;

        public LoginRequestListener(LoginViewModel loginViewModel, LoginPage loginPage = null)
        {
            _loginViewModel = loginViewModel;
            _loginPage = loginPage;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                timerAPI_EAGAIN = new DispatcherTimer();
                timerAPI_EAGAIN.Tick += timerTickAPI_EAGAIN;
                timerAPI_EAGAIN.Interval = new TimeSpan(0, 0, 10);
            });
        }

        // Method which is call when the timer event is triggered
        private void timerTickAPI_EAGAIN(object sender, object e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (timerAPI_EAGAIN != null)
                    timerAPI_EAGAIN.Stop();
                ProgressService.SetProgressIndicator(true, ProgressMessages.ServersTooBusy);
            });
        }

        #region  Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_Login; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.LoginFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.LoginFailed_Title.ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return true; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { return (typeof(MainPage)); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { return NavigationParameter.Login; }
        }

        #endregion

        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);

                _loginViewModel.ControlState = true;

                if (timerAPI_EAGAIN != null)
                    timerAPI_EAGAIN.Stop();                
            });            

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                _loginViewModel.SessionKey = api.dumpSession();
            }
            else
            {
                if (_loginPage != null)
                    Deployment.Current.Dispatcher.BeginInvoke(() => _loginPage.SetApplicationBar(true));

                switch (e.getErrorCode())
                {
                    case MErrorType.API_ENOENT: // E-mail unassociated with a MEGA account or Wrong password
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            new CustomMessageDialog(ErrorMessageTitle, AppMessages.WrongEmailPasswordLogin,
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialog());
                        return;

                    case MErrorType.API_ETOOMANY: // Too many failed login attempts. Wait one hour.
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            new CustomMessageDialog(ErrorMessageTitle,
                                String.Format(AppMessages.AM_TooManyFailedLoginAttempts, DateTime.Now.AddHours(1).ToString("HH:mm:ss")),
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialog());
                        return;

                    case MErrorType.API_EINCOMPLETE: // Account not confirmed
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            new CustomMessageDialog(ErrorMessageTitle, AppMessages.AM_AccountNotConfirmed,
                                App.AppInformation, MessageDialogButtons.Ok).ShowDialog());
                        return;
                }
            }            

            base.onRequestFinish(api, request, e);
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            this.isFirstAPI_EAGAIN = true;
            base.onRequestStart(api, request);
        }

        public override void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Starts the timer when receives the first API_EAGAIN (-3)
            if (e.getErrorCode() == MErrorType.API_EAGAIN && this.isFirstAPI_EAGAIN)
            {
                this.isFirstAPI_EAGAIN = false;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (timerAPI_EAGAIN != null)
                        timerAPI_EAGAIN.Start();
                });
            }

            base.onRequestTemporaryError(api, request, e);
        }

        #endregion

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            SettingsService.SaveMegaLoginData(_loginViewModel.Email, 
                _loginViewModel.SessionKey);

            // Validate product subscription license on background thread
            Task.Run(() => LicenseService.ValidateLicenses());
        }

        #endregion
    }
}
