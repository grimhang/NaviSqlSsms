using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

using EnvDTE;
using EnvDTE80;

namespace ChoolSsmsAssist
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExecutorCommand
    {
        public const int ExecuteStatementCommandId = 0x0100;
        public const int ExecuteInnerStatementCommandId = 0x0101;


        public static readonly Guid CommandSet = new Guid("fc414d62-d245-4820-8b28-e4378b61211b");

        private readonly AsyncPackage package;
        DTE2 dte;

        private ExecutorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            //dte = (DTE2)ServiceProvider.GetServiceAsync(typeof(DTE));
            //dte = package.GetServiceAsync(typeof(DTE)).ConfigureAwait(false) as DTE2;

            var menuCommandID = new CommandID(CommandSet, ExecuteStatementCommandId);
            //var menuItem = new MenuCommand(this.Execute, menuCommandID);
            var menuItem = new OleMenuCommand(this.Command_Exec, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ExecutorCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in AssistMain's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new ExecutorCommand(package, commandService);
        }

        private Executor.ExecScope GetScope(int commandId)
        {
            var scope = Executor.ExecScope.Block;
            if (commandId == ExecuteInnerStatementCommandId)
            {
                scope = Executor.ExecScope.Inner;
            }
            return scope;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "AssistMain";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private async void Command_Exec(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menuCommand)
            {
                if (dte == null)
                {
                    dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
                }

                var executor = new Executor(dte);
                var scope = GetScope(menuCommand.CommandID.ID);

                executor.ExecuteStatement(scope);
            }
        }
    }
}
