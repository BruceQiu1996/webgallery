//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option or rebuild the Visual Studio project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Web.Application.StronglyTypedResourceProxyBuilder", "14.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class InvitationDetail {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InvitationDetail() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Resources.InvitationDetail", global::System.Reflection.Assembly.Load("App_GlobalResources"));
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I Accept.
        /// </summary>
        internal static string AcceptOwnershipRequestButtonText {
            get {
                return ResourceManager.GetString("AcceptOwnershipRequestButtonText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to App ID.
        /// </summary>
        internal static string AppIDLabel {
            get {
                return ResourceManager.GetString("AppIDLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to I Decline.
        /// </summary>
        internal static string DeclineOwnershipRequestButtonText {
            get {
                return ResourceManager.GetString("DeclineOwnershipRequestButtonText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OK.
        /// </summary>
        internal static string DeleteOwnershipRequestButtonText {
            get {
                return ResourceManager.GetString("DeleteOwnershipRequestButtonText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We&apos;re sorry but this invitation has expired. It was issued more than a week ago. The issuer will need to submit a new invitation. Please acknowledge this message by clicking OK now..
        /// </summary>
        internal static string OwnershipRequestExpiredExplanation {
            get {
                return ResourceManager.GetString("OwnershipRequestExpiredExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have been invited to become a co-owner of an application..
        /// </summary>
        internal static string OwnershipRequestIntro {
            get {
                return ResourceManager.GetString("OwnershipRequestIntro", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You were invited to become an application co-owner but the invitation is no longer active. It was either rescinded or you accepted/declined the invitation previously. The invitation code is: .
        /// </summary>
        internal static string OwnershipRequestProblemExplanation {
            get {
                return ResourceManager.GetString("OwnershipRequestProblemExplanation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do you wish to accept or decline the invitation?.
        /// </summary>
        internal static string OwnershipRequestQuestion {
            get {
                return ResourceManager.GetString("OwnershipRequestQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Version.
        /// </summary>
        internal static string VersionLabel {
            get {
                return ResourceManager.GetString("VersionLabel", resourceCulture);
            }
        }
    }
}
