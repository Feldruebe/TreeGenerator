namespace TreeGeneratorWPF
{
    using System.Windows;

    using TreeGeneratorWPF.ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void MainWindowOnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)((MainWindow)sender).DataContext;
            viewModel.GenerateTreeCommand.Execute(null);
        }
    }
}
