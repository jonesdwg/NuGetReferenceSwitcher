//-----------------------------------------------------------------------
// <copyright file="MainDialog.xaml.cs" company="NuGet Reference Switcher">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>http://nugetreferenceswitcher.codeplex.com/license</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using EnvDTE;
using MyToolkit.Collections;
using MyToolkit.Mvvm;
using NuGetReferenceSwitcher.Presentation.Models;
using NuGetReferenceSwitcher.Presentation.ViewModels;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Window = System.Windows.Window;

namespace NuGetReferenceSwitcher.Presentation.Views
{
    /// <summary>Interaction logic for MainDialog.xaml </summary>
    public partial class MainDialog : Window
    {
        private OpenFileDialog _fileDlg;
        private FolderBrowserDialog _folderDlg;

        /// <summary>Initializes a new instance of the <see cref="MainDialog"/> class. </summary>
        /// <param name="application">The application object. </param>
        /// <param name="extensionAssembly">The assembly of the extension. </param>
        public MainDialog(DTE application, Assembly extensionAssembly)
        {
            InitializeComponent();

            Model.ExtensionAssembly = extensionAssembly; 
            Model.Application = application;
            Model.Dispatcher = Dispatcher;

            ViewModelHelper.RegisterViewModel(Model, this);

            Model.Projects.ExtendedCollectionChanged += OnProjectsChanged;
            KeyUp += OnKeyUp;
        }

        /// <summary>Gets the view model. </summary>
        public MainDialogModel Model
        {
            get { return (MainDialogModel)Resources["ViewModel"]; }
        }

        private void OnOpenHyperlink(object sender, RoutedEventArgs e)
        {
            var uri = ((Hyperlink)sender).NavigateUri;
            System.Diagnostics.Process.Start(uri.ToString());
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
                Close();
        }

        private void OnProjectsChanged(object sender, ExtendedNotifyCollectionChangedEventArgs<ProjectModel> args)
        {
            if (Model.Projects.Any(p => p.CurrentToNuGetTransformations.Any()))
                Tabs.SelectedIndex = 1;
        }

        private async void OnSwitchToProjectReferences(object sender, RoutedEventArgs e)
        {
            await Model.SwitchToProjectReferencesAsync();
            Close();
        }

        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            Model.FilterVisibleTransformations();
        }

        private async void OnSwitchToNuGetReferences(object sender, RoutedEventArgs e)
        {
            await Model.SwitchToNuGetReferencesAsync();
            Close();
        }

        private void OnSelectProjectFile(object sender, RoutedEventArgs e)
        {
            var fntpSwitch = (FromNuGetToProjectTransformation)((Button)sender).Tag;
            if (_fileDlg == null)
            {
                _fileDlg = new OpenFileDialog();
                _fileDlg.Filter = "CSharp Projects (*.csproj)|*.csproj|VB.NET Projects (*.vbproj)|*.vbproj";

                // switch to VB if any VB project is already referenced
                if (Model.Transformations.Any(t => t.ToProjectPath != null && t.ToProjectPath.EndsWith(".vbproj", System.StringComparison.OrdinalIgnoreCase)))
                    _fileDlg.FilterIndex = 2;
            }

            _fileDlg.Title = string.Format("Select Project for '{0}'", fntpSwitch.FromAssemblyName);

            if (_fileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                fntpSwitch.ToProjectPath = _fileDlg.FileName;
        }

        private void OnAddSearchDirectory(object sender, RoutedEventArgs e)
        {
            if (_folderDlg == null)
            {
                _folderDlg = new FolderBrowserDialog();                              
            }

            if (_folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!Model.SearchDirectories.Contains(_folderDlg.SelectedPath))
                {
                    Model.SearchDirectories.Add(_folderDlg.SelectedPath);
                    Model.SearchForProjectSources();
                }
            }               
        }

        private void OnRemoveSelectedSearchDirectory(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedSearchDirectory != null)
            {
                Model.SearchDirectories.Remove(Model.SelectedSearchDirectory);
                Model.SearchForProjectSources();
            }
        }

        private void OnMoveUpSelectedSearchDirectory(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedSearchDirectory != null)
            {
                var selected = Model.SelectedSearchDirectory;
                var index = Model.SearchDirectories.IndexOf(selected);
                if (index > 0)
                {
                    Model.SearchDirectories.RemoveAt(index);
                    Model.SearchDirectories.Insert(index -1, selected);
                    Model.SelectedSearchDirectory = selected;

                    Model.SearchForProjectSources();
                }
            }
        }

        private void OnMoveDownSelectedSearchDirectory(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedSearchDirectory != null)
            {
                var selected = Model.SelectedSearchDirectory;
                var index = Model.SearchDirectories.IndexOf(selected);
                if (index < Model.SearchDirectories.Count - 1)
                {
                    Model.SearchDirectories.RemoveAt(index);
                    if (index + 1 == Model.SearchDirectories.Count - 1)
                    {
                        Model.SearchDirectories.Add(selected);
                    }
                    else
                    {
                        Model.SearchDirectories.Insert(index + 1, selected);
                    }
                    Model.SelectedSearchDirectory = selected;

                    Model.SearchForProjectSources();
                }
            }
        }
    }
}
