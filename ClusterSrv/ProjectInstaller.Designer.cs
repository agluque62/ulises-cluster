namespace ClusterSrv
{
   partial class ProjectInstaller
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Component Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
            System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
            System.ServiceProcess.ServiceInstaller serviceInstaller;
            serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller
            // 
            serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            serviceProcessInstaller.Password = null;
            serviceProcessInstaller.Username = null;
            // 
            // serviceInstaller
            // 
            serviceInstaller.Description = "Emulador de Cluster por Software";
            serviceInstaller.DisplayName = "ClusterSrv";
            serviceInstaller.ServiceName = "ClusterSrv";
            serviceInstaller.ServicesDependedOn = new string[] {
        "dot3svc"};
            serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            serviceProcessInstaller,
            serviceInstaller});

      }

      #endregion

   }
}