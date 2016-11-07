// // ----------------------------------------------------------------------
// // <copyright file="AccountViewModel.cs" company="Microsoft Corporation">
// // Copyright (c) Microsoft Corporation.
// // All rights reserved.
// // THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// // KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// // IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// // PARTICULAR PURPOSE.
// // </copyright>
// // ----------------------------------------------------------------------
// // <summary>AccountViewModel.cs</summary>
// // ----------------------------------------------------------------------

namespace TranslationAssistant.DocumentTranslationInterface.ViewModel
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using TranslationAssistant.Business;
    using TranslationAssistant.Business.Model;
    using TranslationAssistant.DocumentTranslationInterface.Common;
    using System.Threading.Tasks;

    /// <summary>
    ///     The account view model.
    /// </summary>
    public class AccountViewModel : Notifyer
    {
        #region Fields

        /// <summary>
        ///     The application id.
        /// </summary>
        private string subscriptionKey;

        /// <summary>
        ///     The category identifier.
        /// </summary>
        private string categoryID;

        /// <summary>
        ///     The save account settings click command.
        /// </summary>
        private ICommand saveAccountSettingsClickCommand;

        /// <summary>
        ///     The status text.
        /// </summary>
        private string statusText;

        #endregion

        #region Constructors and Destructors


        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the application id.
        /// </summary>
        public string SubscriptionKey
        {
            get
            {
                return subscriptionKey;
            }

            set
            {
                subscriptionKey = value;
                NotifyPropertyChanged("ClientID");
            }
        }
        
        public string CategoryID
        {
            get
            {
                return this.categoryID;
            }

            set
            {
                categoryID = value;
                NotifyPropertyChanged("CategoryID");
            }
        }

        /// <summary>
        ///     Gets the save account settings click command.
        /// </summary>
        public ICommand SaveAccountSettingsClickCommand
        {
            get
            {
                return this.saveAccountSettingsClickCommand
                       ?? (saveAccountSettingsClickCommand = new DelegateCommand(this.SaveAccountClick));
            }
        }

        /// <summary>
        ///     Gets or sets the status text.
        /// </summary>
        public string StatusText
        {
            get
            {
                return statusText;
            }

            set
            {
                statusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Load the account settings from the DocumentTranslator.settings file, which is actually in the user appsettings folder and named user.config.
        /// </summary>
        public AccountViewModel()
        {
            //Initialize in order to load the credentials.
            TranslationServices.Core.TranslationServiceFacade.Initialize();
            subscriptionKey = TranslationServices.Core.TranslationServiceFacade.SubscriptionKey;
            categoryID = TranslationServices.Core.TranslationServiceFacade.CategoryID;
        }

        /// <summary>
        ///     Saves the account settings to the settings file for next use.
        /// </summary>
        async private void SaveAccountClick()
        {
            //Set the Account values and save.
            TranslationServices.Core.TranslationServiceFacade.SubscriptionKey = subscriptionKey;
            Task<bool> t_isready = TranslationServices.Core.TranslationServiceFacade.IsTranslationServiceReadyAsync();
            TranslationServices.Core.TranslationServiceFacade.CategoryID = categoryID;
            TranslationServices.Core.TranslationServiceFacade.SaveCredentials();

            if (await t_isready) {
                StatusText = "Settings saved. Ready to translate.";
                NotifyPropertyChanged("SettingsSaved");
                //Need to initialize with new credentials in order to get the language list.
                TranslationServices.Core.TranslationServiceFacade.Initialize();
                SingletonEventAggregator.Instance.GetEvent<AccountValidationEvent>().Publish(true);
            }
            else
            {
                StatusText = "The subscription key is invalid.\r\nPlease visit https://portal.azure.com to obtain a subscription and define a key for Microsoft Translator - Text.";
                SingletonEventAggregator.Instance.GetEvent<AccountValidationEvent>().Publish(false);
            }
            if (!TranslationServices.Core.TranslationServiceFacade.IsCategoryValid(this.categoryID))
            {
                StatusText = "Category is invalid.\r\nPlease visit https://hub.microsofttranslator.com to determine a valid category ID, leave empty, or use one of the standard categories.";
                SingletonEventAggregator.Instance.GetEvent<AccountValidationEvent>().Publish(false);
            }
          
        }

        #endregion
    }
}