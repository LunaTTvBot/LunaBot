﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IBot.Resources.Plugins.Points {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class PointLocale {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PointLocale() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("IBot.Resources.Plugins.Points.PointLocale", typeof(PointLocale).Assembly);
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
        ///   Looks up a localized string similar to Check how many {0} you currently have.
        /// </summary>
        internal static string CheckOwnPointsDescription {
            get {
                return ResourceManager.GetString("CheckOwnPointsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hey {0}, you have {1} {2}.
        /// </summary>
        internal static string CheckOwnPointsSuccess {
            get {
                return ResourceManager.GetString("CheckOwnPointsSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Check how many {0} a User has.
        /// </summary>
        internal static string CheckUserPointsDescription {
            get {
                return ResourceManager.GetString("CheckUserPointsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Something went horribly wrong, go complain that you lost {0} without anything happening.
        /// </summary>
        internal static string GenericTransferError {
            get {
                return ResourceManager.GetString("GenericTransferError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Give {0} to someone.
        /// </summary>
        internal static string GivePointsDescription {
            get {
                return ResourceManager.GetString("GivePointsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You gave {0} {1} {2}.
        /// </summary>
        internal static string GivePointsSuccess {
            get {
                return ResourceManager.GetString("GivePointsSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You don&apos;t have the necessary rights to do this, pal.
        /// </summary>
        internal static string NoPermission {
            get {
                return ResourceManager.GetString("NoPermission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You don&apos;t have enough {0} to do this.
        /// </summary>
        internal static string NotEnoughPoints {
            get {
                return ResourceManager.GetString("NotEnoughPoints", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} has {1} {2}.
        /// </summary>
        internal static string PointCheckSuccess {
            get {
                return ResourceManager.GetString("PointCheckSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transfer some of your {0} to someone else.
        /// </summary>
        internal static string TransferPointsDescription {
            get {
                return ResourceManager.GetString("TransferPointsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} gave {1} {2} {3}.
        /// </summary>
        internal static string UserGaveUserPoints {
            get {
                return ResourceManager.GetString("UserGaveUserPoints", resourceCulture);
            }
        }
    }
}
