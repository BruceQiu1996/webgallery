using AppGallery.SIR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

using DatabaseChoice = AppGallery.SIR.Package.DatabaseChoice;
using LogEventType = AppGallery.SIR.ILog.LogEvent.LogEventType;
using ValidationEvent = AppGallery.SIR.ILog.LogEvent;
using ValidationResult = AppGallery.SIR.ILog.ValidationResult;

using CustomValidParamTag = WebGallery.Services.SIR.AppGalleryRequirementsSection.CustomValidParamTagsCollection.CustomValidParamTag;
using OptionalProviderElement = WebGallery.Services.SIR.AppGalleryRequirementsSection.OptionalProvidersCollection.OptionalProviderElement;
using RequiredProviderElement = WebGallery.Services.SIR.AppGalleryRequirementsSection.RequiredProvidersCollection.RequiredProviderElement;

namespace WebGallery.Services.SIR
{
    public class SirPackageValidator
    {
        public PackageValidation PackageValidation { get; set; }

        public SirPackageValidator(PackageValidation validation)
        {
            PackageValidation = validation;
        }

        public void Validate()
        {
            var packageValidationManager = new PackageValidationManager
            {
                SourcePath = PackageValidation.PackagePath,
                SkipInstallation = true,
                SkipReportGeneration = true,
            };

            var configurationError = string.Empty;
            var requiredSettingsString = string.Empty;
            if (LoadConfiguration(packageValidationManager, PackageValidation.WorkingFolder, out configurationError) &&
                packageValidationManager.CheckIfRequiredSettingsPresent(out requiredSettingsString))
            {
                packageValidationManager.ValidationStatusUpdated += new PackageValidationManager.ValidationStatusUpdatedHandler(PackageValidationManager_ValidationStatusUpdated);
                packageValidationManager.ValidationCompleted += new PackageValidationManager.ValidationCompletedHandler(PackageValidationManager_ValidationCompleted);

                packageValidationManager.ValidatePackage(PackageValidation.PackagePath);

                // since some validation items should be ignored and not be added to ValidationItems, if no one in validationItems is "Fail", the validaton should be "Pass" 
                PackageValidation.Result = ValidationResult.Pass;
                foreach (var item in PackageValidation.ValidationItems)
                {
                    if (item.Type == LogEventType.Fail)
                    {
                        PackageValidation.Result = ValidationResult.Fail;
                    }
                }
            }
            else
            {
                PackageValidation.Result = ValidationResult.Fail;
                PackageValidation.ErrorMessage = string.IsNullOrEmpty(configurationError) ? requiredSettingsString : configurationError;
            }
        }

        private void PackageValidationManager_ValidationStatusUpdated(object sender, StatusUpdatedEventArgs e)
        {
            switch (e.ValidationEvent.Type)
            {
                case LogEventType.Installation:
                    // ignore installation events
                    break;
                default:

                    // these errors should be ignored
                    // 1.Literal String replacements will render application unusable after publish, Absolute Regular Expressions should be used in TextFile match
                    // 2.Missing required provider: setacl
                    if (e.ValidationEvent.Message.Contains("Literal String replacements will render application unusable after publish, Absolute Regular Expressions should be used in TextFile match") ||
                        e.ValidationEvent.Message.Contains("Missing required provider: setacl"))
                    {
                        PackageValidation.ValidationItems.Enqueue(new ValidationItem { ValidationEvent = e.ValidationEvent, Type = LogEventType.Informational });
                    }
                    else
                    {
                        PackageValidation.ValidationItems.Enqueue(new ValidationItem { ValidationEvent = e.ValidationEvent, Type = e.ValidationEvent.Type });
                    }

                    break;
            }
        }

        private void PackageValidationManager_ValidationCompleted(object sender, ValidationCompletedEventArgs e)
        {
            PackageValidation.MD5 = Package.Current.MD5Hash;
            PackageValidation.SHA = Package.Current.SHA1Hash;

            // validate SHA-1 hash
            var sha1HashValidationEvent = ValidateSha1Hash(PackageValidation.SHA, PackageValidation.Sha1HashToValidate);
            PackageValidation.ValidationItems.Enqueue(new ValidationItem { ValidationEvent = sha1HashValidationEvent, Type = sha1HashValidationEvent.Type });

            PackageValidation.Result = (sha1HashValidationEvent.Type == LogEventType.Fail) ? ValidationResult.Fail : e.Result;
        }

        private static ValidationEvent ValidateSha1Hash(string packageSha1Hash, string Sha1HashToValidate)
        {
            var sha1HashValidationResult = packageSha1Hash.Equals(Sha1HashToValidate, StringComparison.OrdinalIgnoreCase) ? LogEventType.Pass : LogEventType.Fail;
            var message = packageSha1Hash.Equals(Sha1HashToValidate, StringComparison.OrdinalIgnoreCase) ? $"The SHA-1 hash ({Sha1HashToValidate}) from the submission form matches the one ({packageSha1Hash}) from a calculation with the pacakge." : $"SHA-1 hash codes don't match. The hash from the submission form is {Sha1HashToValidate}, the hash from a calculation with the pacakge is {packageSha1Hash}.";

            return new ValidationEvent(sha1HashValidationResult, message);
        }

        public static bool LoadConfiguration(PackageValidationManager packageValidationManager, string workingFolder, out string configurationError)
        {
            configurationError = string.Empty;
            try
            {
                if (ConfigurationManager.AppSettings["unzippedFolderLocation"] != null)
                {
                    packageValidationManager.UnzippedFolderLocation = Path.Combine(workingFolder, ConfigurationManager.AppSettings["unzippedFolderLocation"]);
                }

                if (ConfigurationManager.AppSettings["dbType"] != null)
                {
                    try
                    {
                        packageValidationManager.DbChoice = (DatabaseChoice)Enum.Parse(typeof(DatabaseChoice), ConfigurationManager.AppSettings["dbType"]);
                    }
                    catch (Exception) { }
                }

                var sqlInfo = ConfigurationManager.GetSection("SqlInfo") as SqlInfo;
                if (sqlInfo != null)
                {
                    packageValidationManager.SqlDbInfo = new DbInfo(sqlInfo.AdminUsername, sqlInfo.AdminPassword, sqlInfo.Server);
                }

                var mySqlInfo = ConfigurationManager.GetSection("MySqlInfo") as MySqlInfo;
                if (mySqlInfo != null)
                {
                    packageValidationManager.MySqlDbInfo = new DbInfo(mySqlInfo.AdminUsername, mySqlInfo.AdminPassword, mySqlInfo.Server);
                }

                var appGalleryConfigSection = ConfigurationManager.GetSection("AppGalleryRequirements") as AppGalleryRequirementsSection;
                if (appGalleryConfigSection.RequiredProviders != null)
                {
                    foreach (RequiredProviderElement requiredProviderElement in appGalleryConfigSection.RequiredProviders)
                    {
                        if (!AppGalleryRequirements.RequiredProviders.Contains(requiredProviderElement.Name))
                        {
                            AppGalleryRequirements.RequiredProviders.Add(requiredProviderElement.Name);
                        }
                    }
                }
                if (appGalleryConfigSection.OptionalProviders != null)
                {
                    foreach (OptionalProviderElement optionalProviderElement in appGalleryConfigSection.OptionalProviders)
                    {
                        if (!AppGalleryRequirements.OptionalProviders.Contains(optionalProviderElement.Name))
                        {
                            AppGalleryRequirements.OptionalProviders.Add(optionalProviderElement.Name);
                        }
                    }
                }
                if (appGalleryConfigSection.CustomValidParamTags != null)
                {
                    foreach (CustomValidParamTag customValidParamTag in appGalleryConfigSection.CustomValidParamTags)
                    {
                        if (!AppGalleryRequirements.CustomTags.Contains(customValidParamTag.Name))
                        {
                            AppGalleryRequirements.CustomTags.Add(customValidParamTag.Name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                configurationError = "Unexpected exception occurred while parsing configuration: " + e.Message;

                return false;
            }

            return true;
        }
    }

    public class ValidationItem
    {
        public ValidationEvent ValidationEvent { get; set; }
        public LogEventType Type { get; set; }
    }

    public class PackageValidation
    {
        public string PackagePath { get; private set; }
        public string Sha1HashToValidate { get; private set; }
        public string WorkingFolder { get; private set; }

        public string MD5 { get; set; } = string.Empty;
        public string SHA { get; set; } = string.Empty;

        public Queue<ValidationItem> ValidationItems { get; set; } = new Queue<ValidationItem>();
        public ValidationResult Result { get; set; } = ValidationResult.Fail;
        public string ErrorMessage { get; set; } = string.Empty;

        public PackageValidation(string packagePath, string sha1HashToValidate, string workingFolder)
        {
            PackagePath = packagePath;
            Sha1HashToValidate = sha1HashToValidate;
            WorkingFolder = workingFolder;
        }

        public static PackageValidation Fail(string packageUrl, string sha1HashToValidate, string workingFolder, string errorMessage)
        {
            return new PackageValidation(packageUrl, sha1HashToValidate, workingFolder)
            {
                Result = ValidationResult.Fail,
                ErrorMessage = errorMessage,
            };
        }
    }

    public class AppGalleryRequirementsSection : ConfigurationSection
    {
        [ConfigurationProperty("RequiredProviders")]
        public RequiredProvidersCollection RequiredProviders
        {
            get { return (RequiredProvidersCollection)(base["RequiredProviders"]); }
        }

        [ConfigurationProperty("OptionalProviders")]
        public OptionalProvidersCollection OptionalProviders
        {
            get { return (OptionalProvidersCollection)(base["OptionalProviders"]); }
        }

        [ConfigurationProperty("CustomValidParamTags")]
        public CustomValidParamTagsCollection CustomValidParamTags
        {
            get { return (CustomValidParamTagsCollection)(base["CustomValidParamTags"]); }
        }

        #region RequiredProvidersCollection

        public class RequiredProvidersCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new RequiredProviderElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((RequiredProviderElement)(element)).Name;
            }

            public RequiredProviderElement this[int index]
            {
                get { return (RequiredProviderElement)base.BaseGet(index); }
            }

            public class RequiredProviderElement : ConfigurationElement
            {
                [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
                public string Name
                {
                    get { return (string)(base["name"]); }
                }
            }
        }

        #endregion

        #region OptionalProvidersCollection

        public class OptionalProvidersCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new OptionalProviderElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((OptionalProviderElement)(element)).Name;
            }

            public OptionalProviderElement this[int index]
            {
                get { return (OptionalProviderElement)base.BaseGet(index); }
            }

            public class OptionalProviderElement : ConfigurationElement
            {
                [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
                public string Name
                {
                    get { return (string)(base["name"]); }
                }
            }
        }

        #endregion

        #region CustomValidParamTagsCollection 

        public class CustomValidParamTagsCollection : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new CustomValidParamTag();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((CustomValidParamTag)(element)).Name;
            }

            public CustomValidParamTag this[int index]
            {
                get { return (CustomValidParamTag)base.BaseGet(index); }
            }

            public class CustomValidParamTag : ConfigurationElement
            {
                [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
                public string Name
                {
                    get { return (string)(base["name"]); }
                }
            }
        }

        #endregion
    }

    public class DbInfoSection : ConfigurationSection
    {
        private static ConfigurationProperty _adminUsername;
        private static ConfigurationProperty _adminPassword;
        private static ConfigurationProperty _server;

        static DbInfoSection()
        {
            _adminUsername = new ConfigurationProperty(
                "adminUsername",
                typeof(string),
                string.Empty,
                ConfigurationPropertyOptions.IsRequired
            );
            _adminPassword = new ConfigurationProperty(
                "adminPassword",
                typeof(string),
                string.Empty,
                ConfigurationPropertyOptions.IsRequired
            );
            _server = new ConfigurationProperty(
                "server",
                typeof(string),
                string.Empty,
                ConfigurationPropertyOptions.IsRequired
            );
        }

        [ConfigurationProperty("adminUsername")]
        public string AdminUsername
        {
            get { return (string)base[_adminUsername]; }
        }

        [ConfigurationProperty("adminPassword")]
        public string AdminPassword
        {
            get { return (string)base[_adminPassword]; }
        }

        [ConfigurationProperty("server")]
        public string Server
        {
            get { return (string)base[_server]; }
        }
    }

    public class MySqlInfo : DbInfoSection
    {
    }

    public class SqlInfo : DbInfoSection
    {
    }
}