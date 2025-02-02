using System;
using System.IO;
using System.Windows;
using GCLauncher.Source;

namespace GCLauncher{
    public partial class App : Application{
        string stringData;
        public static Info statusApp = new Info();
        void AppMain(object sender, StartupEventArgs e){
            stringData = statusApp.GetFileString(2);
            if(stringData == null){
                MessageBox.Show("Tente novamente mais tarde!", "Sem conexão!");
                Environment.Exit(0);
            }
            else{
                if(!statusApp.IsAdministrator()){
                    MessageBox.Show("Execute o launcher como administrador!", "Erro de permissão!");
                    Environment.Exit(0);
                }
                else{
                    if (File.Exists("d3d9.dll")){
                        File.Move("d3d9.dll", "_d3d9.dll");
                    }
                    Splash Load = new Splash();
                }
            }
        }
    }
}