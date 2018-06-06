﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.MegaApi
{
    class GetContactDataRequestListener : BaseRequestListener
    {
        private readonly Contact _megaContact;

        public GetContactDataRequestListener(Contact megaContact)
        {            
            _megaContact = megaContact;
        }

        protected override string ProgressMessage
        {
            get { return ProgressMessages.GetContactData; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string ErrorMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return false; }
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

        #region Override Methods

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            if (request.getType() == MRequestType.TYPE_GET_ATTR_USER)
            {
                switch (request.getParamType())
                {
                    case (int)MUserAttrType.USER_ATTR_FIRSTNAME:
                        Deployment.Current.Dispatcher.BeginInvoke(() => _megaContact.FirstName = request.getText());
                        break;

                    case (int)MUserAttrType.USER_ATTR_LASTNAME:
                        Deployment.Current.Dispatcher.BeginInvoke(() => _megaContact.LastName = request.getText());
                        break;
                }
            }
        }

        #endregion
    }
}
