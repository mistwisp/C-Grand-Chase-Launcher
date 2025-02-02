using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GCLauncher{
    public partial class Config : Window {
        public Launcher parentWindow {  get; set; }
        public Config(int option, Launcher parent) {
            InitializeComponent();
            parentWindow = parent;
            ConfigStarter(option);
            Show();
        }
        public void ConfigStarter(int option) {
            if(option == 0) {
                TitleText.Text = parentWindow.GetPassLangString("language_title");
                ConfigMsg.Text = parentWindow.GetPassLangString("language_text");
                DllSelect.IsEnabled = false;
                DllSelect.Opacity = 0;
                LangSelect.IsEnabled = true;
                LangSelect.Opacity = 1;
            }
            else {
                TitleText.Text = parentWindow.GetPassLangString("dll_title");
                ConfigMsg.Text = parentWindow.GetPassLangString("dll_text");
                DllSelect.IsEnabled = true;
                DllSelect.Opacity = 1;
                LangSelect.IsEnabled = false;
                LangSelect.Opacity = 0;
            }
        }
        private void DLLSelect(object sender, EventArgs e) {
            if (File.Exists("_d3d9.dll")) {
                File.Delete("_d3d9.dll");
            }
            if (File.Exists("d3d9.dll")) {
                File.Delete("d3d9.dll");
            }
            if (File.Exists("Dll/dll" + (DllSelect.SelectedIndex + 1) + ".dll")) {
                File.Copy("Dll/dll" + (DllSelect.SelectedIndex + 1) + ".dll", "_d3d9.dll");
            }
            Close();
        }
        private void LANGSelect(object sender, EventArgs e) {
            int langNumber = LangSelect.SelectedIndex;
            using (StreamWriter writer = new StreamWriter("language.bin")) {
                writer.WriteLine(langNumber);
            }
            parentWindow.UpdateLauncherText();
            Close();
        }
        private void ClickConfig(object sender, RoutedEventArgs e) {
            string content = (sender as Button).Name.ToString();
            switch(content){
                case "btnclose": Close(); break;
            }
        }
    }
}
