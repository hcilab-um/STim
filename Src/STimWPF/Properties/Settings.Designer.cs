﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1008
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace STimAttentionWPF.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.5")]
        public double CloseZoneConstrain {
            get {
                return ((double)(this["CloseZoneConstrain"]));
            }
            set {
                this["CloseZoneConstrain"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public double InteractionZoneConstrain {
            get {
                return ((double)(this["InteractionZoneConstrain"]));
            }
            set {
                this["InteractionZoneConstrain"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public double NotificationZoneConstrain {
            get {
                return ((double)(this["NotificationZoneConstrain"]));
            }
            set {
                this["NotificationZoneConstrain"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int BlockPercentBufferSize {
            get {
                return ((int)(this["BlockPercentBufferSize"]));
            }
            set {
                this["BlockPercentBufferSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1000")]
        public int UploadPeriod {
            get {
                return ((int)(this["UploadPeriod"]));
            }
            set {
                this["UploadPeriod"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\{USERNAME}\\Dropbox\\STimStatus\\Image\\")]
        public string ImageFolder {
            get {
                return ((string)(this["ImageFolder"]));
            }
            set {
                this["ImageFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("yyMMdd-HHmmss-fff")]
        public string DateTimeFileNameFormat {
            get {
                return ((string)(this["DateTimeFileNameFormat"]));
            }
            set {
                this["DateTimeFileNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("yyMMdd-HH:mm:ss.fff")]
        public string DateTimeLogFormat {
            get {
                return ((string)(this["DateTimeLogFormat"]));
            }
            set {
                this["DateTimeLogFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1.06")]
        public double DisplayWidthInMeters {
            get {
                return ((double)(this["DisplayWidthInMeters"]));
            }
            set {
                this["DisplayWidthInMeters"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.325")]
        public double DisplayHeightInMeters {
            get {
                return ((double)(this["DisplayHeightInMeters"]));
            }
            set {
                this["DisplayHeightInMeters"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.18")]
        public double KinectDistanceZ {
            get {
                return ((double)(this["KinectDistanceZ"]));
            }
            set {
                this["KinectDistanceZ"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.265")]
        public double KinectDistanceY {
            get {
                return ((double)(this["KinectDistanceY"]));
            }
            set {
                this["KinectDistanceY"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public double BlockDepthPercent {
            get {
                return ((double)(this["BlockDepthPercent"]));
            }
            set {
                this["BlockDepthPercent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4")]
        public int ScreenGridRows {
            get {
                return ((int)(this["ScreenGridRows"]));
            }
            set {
                this["ScreenGridRows"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4")]
        public int ScreenGridColumns {
            get {
                return ((int)(this["ScreenGridColumns"]));
            }
            set {
                this["ScreenGridColumns"] = value;
            }
        }
    }
}
