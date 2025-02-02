using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GCLauncher.Source {
    public static class ProgressBarExtensions {
        private static TimeSpan duration = TimeSpan.FromSeconds(2);
        public static void SetPercent(this ProgressBar progressBar, double percentage) {
            DoubleAnimation animation = new DoubleAnimation(percentage, duration);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }
    }
}
