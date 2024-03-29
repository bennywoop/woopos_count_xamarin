﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace WooPosCount.net.databasemart.iis3002.com.magentopos {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="POSLicenseServiceSoap", Namespace="http://www.woopos.com/")]
    public partial class POSLicenseService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback TestOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetLicenseDataOperationCompleted;
        
        private System.Threading.SendOrPostCallback SaveFileOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetFileLenOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetFileOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public POSLicenseService() {
            this.Url = "http://magentopos.com.iis3002.databasemart.net/POSLSService.asmx";
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event TestCompletedEventHandler TestCompleted;
        
        /// <remarks/>
        public event GetLicenseDataCompletedEventHandler GetLicenseDataCompleted;
        
        /// <remarks/>
        public event SaveFileCompletedEventHandler SaveFileCompleted;
        
        /// <remarks/>
        public event GetFileLenCompletedEventHandler GetFileLenCompleted;
        
        /// <remarks/>
        public event GetFileCompletedEventHandler GetFileCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.woopos.com/Test", RequestNamespace="http://www.woopos.com/", ResponseNamespace="http://www.woopos.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string Test(string InputString) {
            object[] results = this.Invoke("Test", new object[] {
                        InputString});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void TestAsync(string InputString) {
            this.TestAsync(InputString, null);
        }
        
        /// <remarks/>
        public void TestAsync(string InputString, object userState) {
            if ((this.TestOperationCompleted == null)) {
                this.TestOperationCompleted = new System.Threading.SendOrPostCallback(this.OnTestOperationCompleted);
            }
            this.InvokeAsync("Test", new object[] {
                        InputString}, this.TestOperationCompleted, userState);
        }
        
        private void OnTestOperationCompleted(object arg) {
            if ((this.TestCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.TestCompleted(this, new TestCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.woopos.com/GetLicenseData", RequestNamespace="http://www.woopos.com/", ResponseNamespace="http://www.woopos.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetLicenseData(string requestData) {
            object[] results = this.Invoke("GetLicenseData", new object[] {
                        requestData});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetLicenseDataAsync(string requestData) {
            this.GetLicenseDataAsync(requestData, null);
        }
        
        /// <remarks/>
        public void GetLicenseDataAsync(string requestData, object userState) {
            if ((this.GetLicenseDataOperationCompleted == null)) {
                this.GetLicenseDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLicenseDataOperationCompleted);
            }
            this.InvokeAsync("GetLicenseData", new object[] {
                        requestData}, this.GetLicenseDataOperationCompleted, userState);
        }
        
        private void OnGetLicenseDataOperationCompleted(object arg) {
            if ((this.GetLicenseDataCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetLicenseDataCompleted(this, new GetLicenseDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.woopos.com/SaveFile", RequestNamespace="http://www.woopos.com/", ResponseNamespace="http://www.woopos.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool SaveFile([System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] docbinaryarray, string FileName, int BlockNumber, int BlockSize) {
            object[] results = this.Invoke("SaveFile", new object[] {
                        docbinaryarray,
                        FileName,
                        BlockNumber,
                        BlockSize});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public void SaveFileAsync(byte[] docbinaryarray, string FileName, int BlockNumber, int BlockSize) {
            this.SaveFileAsync(docbinaryarray, FileName, BlockNumber, BlockSize, null);
        }
        
        /// <remarks/>
        public void SaveFileAsync(byte[] docbinaryarray, string FileName, int BlockNumber, int BlockSize, object userState) {
            if ((this.SaveFileOperationCompleted == null)) {
                this.SaveFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSaveFileOperationCompleted);
            }
            this.InvokeAsync("SaveFile", new object[] {
                        docbinaryarray,
                        FileName,
                        BlockNumber,
                        BlockSize}, this.SaveFileOperationCompleted, userState);
        }
        
        private void OnSaveFileOperationCompleted(object arg) {
            if ((this.SaveFileCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SaveFileCompleted(this, new SaveFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.woopos.com/GetFileLen", RequestNamespace="http://www.woopos.com/", ResponseNamespace="http://www.woopos.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int GetFileLen(string FileName) {
            object[] results = this.Invoke("GetFileLen", new object[] {
                        FileName});
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public void GetFileLenAsync(string FileName) {
            this.GetFileLenAsync(FileName, null);
        }
        
        /// <remarks/>
        public void GetFileLenAsync(string FileName, object userState) {
            if ((this.GetFileLenOperationCompleted == null)) {
                this.GetFileLenOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetFileLenOperationCompleted);
            }
            this.InvokeAsync("GetFileLen", new object[] {
                        FileName}, this.GetFileLenOperationCompleted, userState);
        }
        
        private void OnGetFileLenOperationCompleted(object arg) {
            if ((this.GetFileLenCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetFileLenCompleted(this, new GetFileLenCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.woopos.com/GetFile", RequestNamespace="http://www.woopos.com/", ResponseNamespace="http://www.woopos.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] GetFile(string FileName, int BlockNumber, int BlockSize) {
            object[] results = this.Invoke("GetFile", new object[] {
                        FileName,
                        BlockNumber,
                        BlockSize});
            return ((byte[])(results[0]));
        }
        
        /// <remarks/>
        public void GetFileAsync(string FileName, int BlockNumber, int BlockSize) {
            this.GetFileAsync(FileName, BlockNumber, BlockSize, null);
        }
        
        /// <remarks/>
        public void GetFileAsync(string FileName, int BlockNumber, int BlockSize, object userState) {
            if ((this.GetFileOperationCompleted == null)) {
                this.GetFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetFileOperationCompleted);
            }
            this.InvokeAsync("GetFile", new object[] {
                        FileName,
                        BlockNumber,
                        BlockSize}, this.GetFileOperationCompleted, userState);
        }
        
        private void OnGetFileOperationCompleted(object arg) {
            if ((this.GetFileCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetFileCompleted(this, new GetFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    public delegate void TestCompletedEventHandler(object sender, TestCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TestCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal TestCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    public delegate void GetLicenseDataCompletedEventHandler(object sender, GetLicenseDataCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetLicenseDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetLicenseDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    public delegate void SaveFileCompletedEventHandler(object sender, SaveFileCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SaveFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal SaveFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    public delegate void GetFileLenCompletedEventHandler(object sender, GetFileLenCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetFileLenCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetFileLenCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public int Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    public delegate void GetFileCompletedEventHandler(object sender, GetFileCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1586.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public byte[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591