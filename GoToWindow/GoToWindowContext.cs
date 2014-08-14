﻿using GoToWindow.Api;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using GoToWindow.Windows;
using GoToWindow.ViewModels;
using log4net;

namespace GoToWindow
{
    public interface IGoToWindowContext : IDisposable
    {
        void Init();
        void Show();
        void Hide();
        void EnableAltTabHook(bool enabled);
		void ShowSettings();
	}

    public class GoToWindowContext : IGoToWindowContext
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GoToWindowContext).Assembly, "GoToWindow");

        private GoToWindowPluginsContainer _pluginsContainer;
        private MainWindow _mainWindow;
        private KeyboardHook _hooks;

        public GoToWindowContext()
        {
            _pluginsContainer = GoToWindowPluginsContainer.LoadPlugins();
        }

        public void Init()
        {
            // Launch the main window once to run JIT once
            _mainWindow = new MainWindow();
            _mainWindow.Closing += MainWindow_Closing;
            _mainWindow.WindowStyle = WindowStyle.None;
            _mainWindow.Width = 0;
            _mainWindow.Height = 0;
            _mainWindow.Show();

            var viewModel = MainViewModel.Load(_pluginsContainer.Plugins);
            viewModel.Close += Hide;

            if (_mainWindow != null) // If the window auto-closed
            {
                _mainWindow.DataContext = viewModel;
                _mainWindow.Close();
            }
        }

        public void Show()
        {
            if (_mainWindow != null && _mainWindow.IsLoaded)
            {
                Log.Debug("Sending Tab Again to Main Window.");

                _mainWindow.TabAgain();
            }
            else
            {
                Log.Debug("Showing Main Window.");

                _mainWindow = new MainWindow();
                _mainWindow.Closing += MainWindow_Closing;
                _mainWindow.Show();

                var viewModel = MainViewModel.Load(_pluginsContainer.Plugins);
                viewModel.Close += Hide;
                _mainWindow.DataContext = viewModel;
            }
        }

        void MainWindow_Closing(object sender, EventArgs e)
        {
            _mainWindow = null;
        }

        public void Hide()
        {
            if (_mainWindow != null && _mainWindow.IsLoaded)
            {
                Log.Debug("Hiding Main Window.");
                _mainWindow.Close();
            }
                
        }

        private void Hide(object sender, EventArgs e)
        {
            Hide();
        }

        public void EnableAltTabHook(bool enabled)
        {
            if (_hooks == null && enabled)
            {
                _hooks = new KeyboardHook(HandleAltTab);
            }
            else if (_hooks != null && !enabled)
            {
                _hooks.Dispose();
                _hooks = null;
            }
        }

		public void ShowSettings()
		{
			if (Application.Current.Windows.OfType<SettingsWindow>().Any())
				return;

			var settingswindow = new SettingsWindow();
			settingswindow.DataContext = new SettingsViewModel(this);
			settingswindow.ShowDialog();
		}

        private void HandleAltTab()
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(Show),
                DispatcherPriority.Normal,
                null);
        }

        public void Dispose()
        {
            if (_hooks != null)
            {
                _hooks.Dispose();
                _hooks = null;
            }
        }
	}
}
