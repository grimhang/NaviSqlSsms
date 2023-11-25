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

namespace DaviSqlSsms
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExecutorCommand
    {
        public const int ExecuteStatementCommandId = 0x0100;
        //public const int ExecuteInnerStatementCommandId = 0x0101;


        public static readonly Guid CommandSet = new Guid("fc414d62-d245-4820-8b28-e4378b61211b");

        private readonly AsyncPackage package;
        private static DTE2 dte;

        private ExecutorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            //var menuCommandID = new CommandID(CommandSet, ExecuteStatementCommandId);
            //var menuItem = new MenuCommand(this.Execute, menuCommandID);            
            //commandService.AddCommand(menuItem);

            CommandID menuCommandID;
            OleMenuCommand menuCommand;

            // Create execute current statement menu item
            menuCommandID = new CommandID(CommandSet, ExecuteStatementCommandId);
            //var menuItem = new MenuCommand(this.Execute, menuCommandID);
            menuCommand = new OleMenuCommand(this.Command_Exec, menuCommandID);
            commandService.AddCommand(menuCommand);

            // Create execute inner satetement menu item
            /*
            menuCommandID = new CommandID(CommandSet, ExecuteInnerStatementCommandId);
            menuCommand = new OleMenuCommand(Command_SelectSql, menuCommandID);
            //menuCommand.BeforeQueryStatus += Command_QueryStatus;
            commandService.AddCommand(menuCommand);
            */
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

            dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
        }

        private Executor.ExecScope GetScope(int commandId)
        {
            var scope = Executor.ExecScope.Block;
            //if (commandId == ExecuteInnerStatementCommandId)
            //{
            //    scope = Executor.ExecScope.Inner;
            //}
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

        private void Command_Exec(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menuCommand)
            {               
                var executor = new Executor(dte);
                var scope = GetScope(menuCommand.CommandID.ID);

                executor.ExecuteStatement(scope);
            }
        }


    }
}
