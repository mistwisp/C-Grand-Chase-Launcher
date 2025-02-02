using System.Collections.Generic;

namespace GCLauncher.Source {
    public class Language {
        public List<Dictionary<string, string>> Languages {  get; set; } = new List<Dictionary<string, string>> {
            new Dictionary<string, string> {
                {"splash_check", "Verificando arquivos"},
                {"splash_update_launcher", "Atualizazndo launcher"},
                {"launcher_worker_complete", "Atualização completa!"},
                {"launcher_worker_maintenance", "Servidor está em manutenção!"},
                {"launcher_worker_downloading", "Atualizando"},
                {"launcher_worker_total", "Total"},
                {"launcher_game_start", "JOGAR"},
                {"launcher_title_bar", "Hope Eternal Launcher v1.0" },
                {"language_title", "Alterar idioma"},
                {"language_text", "Selecione um idioma da lista abaixo.\nIsto altera o idioma do launchers\ne o idioma dentro de jogo também!"},
                {"dll_title", "Correção de FPS para o Windows 10"},
                {"dll_text", "Selecione abaixo uma opção para corrigir o FPS\ndentro do jogo. Caso a opção desejada não funcionar, \nbasta escolher outra na lista e tentar novamente."},
            },
            new Dictionary<string, string> {
                {"splash_check", "Verifying files"},
                {"splash_update_launcher", "Updating launcher"},
                {"launcher_worker_complete", "Update complete!"},
                {"launcher_worker_maintenance", "Server is in maintenance!"},
                {"launcher_worker_downloading", "Updating"},
                {"launcher_worker_total", "Total"},
                {"launcher_game_start", "GAME START"},
                {"launcher_title_bar", "Hope Eternal Launcher v1.0" },
                {"language_title", "Change language"},
                {"language_text", "Select a language from the list below.\nThis option changes the launcher's language\nand the language in-game as well!"},
                {"dll_title", "Windows 10 FPS Fix"},
                {"dll_text", "Select below an option to fix the FPS in-game.\nIf the selected option doesn't work, \njust select another one on the list and try again."},
            },
            new Dictionary<string, string> {
                {"splash_check", "检查文件"},
                {"splash_update_launcher", "更新启动器"},
                {"launcher_worker_complete", "更新完成！"},
                {"launcher_worker_maintenance", "服务器处于维护模式！"},
                {"launcher_worker_downloading", "更新中"},
                {"launcher_worker_total", "合计"},
                {"launcher_game_start", "开始游戏"},
                {"launcher_title_bar", "Hope Eternal 启动器 v1.0" },
                {"language_title", "改变语言"},
                {"language_text", "從下面的清單中選擇一種語言。 \n此選項會更改啟動器語言\n並且也會更改遊戲語言！"},
                {"dll_title", "Windows 10 FPS 修复"},
                {"dll_text", "选择下面的选项来修复游戏中的 FPS。\n如果所选选项不起作用，\n只需选择列表中的另一个，然后重试。"},
            }
        };

        public string GetString(int lang, string key) {
            if (lang < 0 || lang >= Languages.Count) {
                return "";
            }
            Dictionary<string, string> languageDictionary = Languages[lang];
            if (languageDictionary.ContainsKey(key)) {
                return languageDictionary[key];
            }
            else {
                return "";
            }
        }
    }
}
