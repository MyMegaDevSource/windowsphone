﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class GetPreviewRequestListener: BaseRequestListener
    {
        private readonly ImageNodeViewModel _node;

        public GetPreviewRequestListener(ImageNodeViewModel node)
        {
            this._node = node;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowProgressMessage
        {
            get { return false; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.GetPreviewFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.GetPreviewFailed_Title; }
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
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
        
        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>_node.PreviewImageUri = new Uri(request.getFile()));
        }

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            base.onRequestStart(api, request);
            Deployment.Current.Dispatcher.BeginInvoke(() => _node.IsBusy = true);
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => _node.IsBusy = false);
            base.onRequestFinish(api, request, e);
        }

        #endregion

    }
}
