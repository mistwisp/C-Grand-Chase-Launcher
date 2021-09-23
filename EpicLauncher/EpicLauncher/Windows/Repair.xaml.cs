using System.Windows;
using GCLauncher.Source;
using System.Windows.Controls;

namespace GCLauncher{
    public partial class Repair : Window{
        public static Info RepairInfo = new Info();
        public Repair(){
            InitializeComponent();
            RepairStart();
            Show();
        }
        public void RepairStart(){
            RepairTitle.Text = "Reparar Cliente";
            RepairExp.Text = "Selecione uma opção abaixo para corrigir os problemas em seu cliente. [ Rápida: Repara somente a pasta Stage. | Completa: repara o cliente todo. ]";
            ButtonResolve.Text = "RÁPIDA";
            ButtonResolve2.Text = "COMPLETA";
        }
        public void RepairStart(int option){
            foreach(Window window in Application.Current.Windows){
                if(window.GetType() == typeof(Launcher)){
                    (window as Launcher).WorkerUpdating(option);
                    Close();
                }
            }
        }
        private void ClickRepair(object sender, RoutedEventArgs e){
            string content = (sender as Button).Name.ToString();
            switch(content){
                case    "btnquick": RepairStart(1); break;
                case "btncomplete": RepairStart(2); break;
                case        "clbs":        Close(); break;
            }
        }
    }
}
