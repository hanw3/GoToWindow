﻿using GoToWindow.Api;
using GoToWindow.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace GoToWindow.ViewModels
{
    public class MainWindowViewModel
    {
        public static MainWindowViewModel Load()
        {
            var instance = new MainWindowViewModel();
            var viewSource = new CollectionViewSource();
            viewSource.Source = WindowsListFactory.Load().Windows;
            instance.Windows = viewSource;
            return instance;
        }

        public CollectionViewSource Windows { get; private set; }
        public IWindowEntry SelectedWindowEntry { get; set; }

        public ICommand GoToWindowEntryShortcut { get; private set; }

        public event EventHandler Close;

        public MainWindowViewModel()
        {
            var goToWindowEntryShortcutCommand = new GoToWindowEntryShortcutCommand(GetEntryAt);
            goToWindowEntryShortcutCommand.Executed += goToWindowEntryShortcutCommand_Executed;
            GoToWindowEntryShortcut = goToWindowEntryShortcutCommand;
        }

        private IWindowEntry GetEntryAt(int index)
        {
            var items = Windows.View.Cast<IWindowEntry>().ToArray();

            if (index < items.Length)
                return items[index];

            return null;
        }

        void goToWindowEntryShortcutCommand_Executed(object sender, EventArgs e)
        {
            if (Close != null)
                Close(this, new EventArgs());
        }

    }
}
