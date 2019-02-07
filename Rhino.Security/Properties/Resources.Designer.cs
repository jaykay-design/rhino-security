﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Rhino.Security.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Rhino.Security.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to &apos;{0}&apos; (&apos;{1}&apos; is a member of &apos;{2}&apos;).
        /// </summary>
        internal static string EntityWithGroups {
            get {
                return ResourceManager.GetString("EntityWithGroups", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to everything.
        /// </summary>
        internal static string Everything {
            get {
                return ResourceManager.GetString("Everything", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Names must be unique.
        /// </summary>
        internal static string NonUniqueName {
            get {
                return ResourceManager.GetString("NonUniqueName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to not assoicated with any group.
        /// </summary>
        internal static string NotAssociatedWithAnyGroup {
            get {
                return ResourceManager.GetString("NotAssociatedWithAnyGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation &apos;{0}&apos; was not defined.
        /// </summary>
        internal static string OperationNotDefined {
            get {
                return ResourceManager.GetString("OperationNotDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission (level {3}) for operation &apos;{0}&apos; was denied to &apos;{1}&apos; on &apos;{2}&apos;.
        /// </summary>
        internal static string PermissionDeniedForUser {
            get {
                return ResourceManager.GetString("PermissionDeniedForUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission (level {4}) for operation &apos;{0}&apos; was denied to group &apos;{1}&apos; on &apos;{2}&apos; (&apos;{3}&apos; is a member of &apos;{5}&apos;).
        /// </summary>
        internal static string PermissionDeniedForUsersGroup {
            get {
                return ResourceManager.GetString("PermissionDeniedForUsersGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission for operation &apos;{0}&apos; was not granted to user &apos;{1}&apos; or to the groups &apos;{1}&apos; is associated with (&apos;{2}&apos;).
        /// </summary>
        internal static string PermissionForOperationNotGrantedToUser {
            get {
                return ResourceManager.GetString("PermissionForOperationNotGrantedToUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission for operation &apos;{0}&apos; was not granted to user &apos;{1}&apos; or to the groups &apos;{1}&apos; is associated with (&apos;{2}&apos;) on &apos;{3}&apos; or any of the groups &apos;{3}&apos; is associated with (&apos;{4}&apos;).
        /// </summary>
        internal static string PermissionForOperationNotGrantedToUserOnEntity {
            get {
                return ResourceManager.GetString("PermissionForOperationNotGrantedToUserOnEntity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission (level {3}) for operation &apos;{0}&apos; was granted to &apos;{1}&apos; on &apos;{2}&apos;.
        /// </summary>
        internal static string PermissionGrantedForUser {
            get {
                return ResourceManager.GetString("PermissionGrantedForUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission (level {4}) for operation &apos;{0}&apos; was granted to group &apos;{1}&apos; on &apos;{2}&apos; (&apos;{3}&apos; is a member of &apos;{5}&apos;).
        /// </summary>
        internal static string PermissionGrantedForUsersGroup {
            get {
                return ResourceManager.GetString("PermissionGrantedForUsersGroup", resourceCulture);
            }
        }
    }
}
