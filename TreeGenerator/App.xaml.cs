using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TreeGeneratorWPF
{
    using MahApps.Metro;

    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
    {
        // add custom accent and theme resource dictionaries to the ThemeManager
        // you should replace MahAppsMetroThemesSample with your application name
        // and correct place where your custom accent lives
        ThemeManager.AddAccent("TreeGeneratorAccent", new Uri("pack://application:,,,/TreeGenerator;component/MVVM/TreeGeneratorAccent.xaml"));

        // get the current app style (theme and accent) from the application
        Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

        // now change app style to the custom accent and current theme
        //ThemeManager.ChangeAppStyle(Application.Current,
        //                            ThemeManager.GetAccent("TreeGeneratorAccent"),
        //                            theme.Item1);

        base.OnStartup(e);
    }
    }
}
