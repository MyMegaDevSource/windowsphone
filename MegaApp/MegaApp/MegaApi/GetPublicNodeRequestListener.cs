﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetPublicNodeRequestListener: BaseRequestListener
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        public GetPublicNodeRequestListener(CloudDriveViewModel cloudDriveViewModel)
        {
            this._cloudDriveViewModel = cloudDriveViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.ImportFile; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.ImportFileFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.ImportFileFailed_Title; }
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

        protected override Action SuccesAction
        {
            get
            {
                MNode publicNode = Request.getPublicNode();
                return () => DialogService.ShowOpenLink(publicNode.getName(), Request.getLink(), _cloudDriveViewModel);
            }
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
    }
}
