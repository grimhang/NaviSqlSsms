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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;

using System.IO;
using System.Diagnostics;
using System.Linq;
using Microsoft;
using DaviSqlSsms.Properties;
using System.Text;

namespace DaviSqlSsms
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExecutorCommand
    {
        public const int ExecuteStatementCommandId = 0x0100;
        public const int cmdIdImeAutoFix = 0x0101;


        public static readonly Guid CommandSet = new Guid("fc414d62-d245-4820-8b28-e4378b61211b");

        private readonly AsyncPackage package;
        private static DTE2 dte;

        // ----------------------------------------------------------------------
        private delegate IntPtr LocalKeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LocalKeyboardHookProc _proc = HookCallback;
        private const int WM_IME_CONTROL = 643;

        #region Win32 Api Import

        private const int WH_KEYBOARD = 2;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LocalKeyboardHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        //private static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);
        private static extern bool ImmGetConversionStatus(IntPtr hImc, out int lpConversion, out int lpSentence);

        [DllImport("Imm32.dll")]
        private static extern Boolean ImmSetConversionStatus(IntPtr hIMC, Int32 fdwConversion, Int32 fdwSentence);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr IParam);

        #endregion Win32 Api Import
        // ----------------------------------------------------------------------

        private ExecutorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            //var menuCommandID = new CommandID(CommandSet, ExecuteStatementCommandId);
            //var menuItem = new MenuCommand(this.Execute, menuCommandID);            
            //commandService.AddCommand(menuItem);

            CommandID menuCommandID;
            OleMenuCommand menuCommand;

            // Create Lang Autofix Command
            menuCommandID = new CommandID(CommandSet, cmdIdImeAutoFix);
            menuCommand = new OleMenuCommand(Command_AutoFixStart, menuCommandID);
            //menuCommand.BeforeQueryStatus += Command_QueryStatus;
            commandService.AddCommand(menuCommand);

            // Create execute current statement menu item
            menuCommandID = new CommandID(CommandSet, ExecuteStatementCommandId);
            //var menuItem = new MenuCommand(this.Execute, menuCommandID);
            menuCommand = new OleMenuCommand(this.Command_Exec, menuCommandID);
            //menuCommand.BeforeQueryStatus += Command_QueryStatus;
            commandService.AddCommand(menuCommand);


        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ExecutorCommand Instance
        {
            get;
            private set;
        }

        private static IVsOutputWindowPane OutputWindow;

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

            OutputWindow = await package.GetServiceAsync(typeof(SVsGeneralOutputWindowPane)) as IVsOutputWindowPane;
            Assumes.Present(OutputWindow);
            dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(dte);
        }

        private Executor.ExecScope GetScope(int commandId)
        {
            var scope = Executor.ExecScope.Block;
            //if (commandId == cmdIdImeAutoFix)
            //{
            //    scope = Executor.ExecScope.Inner;
            //}
            return scope;
        }

        private void Command_AutoFixStart(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var obj = sender as MenuCommand;

            if (obj.Checked)
            {
                obj.Checked = false;
                if (UnhookWindowsHookEx(_hookID))
                {
                    MessageBox.Show("감시 종료됨");
                }

                OutputWindow.OutputString($"{DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")} 감시 종료" + Environment.NewLine);

                //dte.Events.WindowEvents.WindowActivated -= OnWindowActivated;
            }
            else
            {
                obj.Checked = true;
                MessageBox.Show("감시 시작됨");
                _hookID = SetHook(_proc);

                OutputWindow.OutputString($"{DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")} 감시 시작" + Environment.NewLine);

                //dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE;
                //dte.Events.WindowEvents.WindowActivated += OnWindowActivated;                
            }

            //ThreadHelper.ThrowIfNotOnUIThread();
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "MainCommand창";

            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process[] localAll = System.Diagnostics.Process.GetProcesses().OrderBy(x => x.ProcessName).ToArray<System.Diagnostics.Process>();

            if (p == null)
                return;

            IntPtr hwnd = p.MainWindowHandle;
            IntPtr hime = ImmGetDefaultIMEWnd(hwnd);    //ime 클래스에 대한 기본 창 핸들 검색
            IntPtr status = SendMessage(hime, WM_IME_CONTROL, new IntPtr(0x5), new IntPtr(0));  //현재 IME 언어 얻어오기

            //message = string.Format(CultureInfo.CurrentCulture, "자판상태:{0}", status.ToInt32());

            WriteHanEngType(status.ToInt32() == 0 ? "Eng" : "Han");
        }

        /*
        private void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (GotFocus.Kind == "Document")
            {
                //Debug.Print(GotFocus.Caption + "이 호출되었당께");
                OutputWindow.OutputString(DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss") + GotFocus.Caption + "이 포커싱 얻음" + $"{GetIME()}-->{ReadHanEngType()}" + Environment.NewLine);
            }

            if (LostFocus.Kind == "Document")
            {
                //Debug.Print(GotFocus.Caption + "이 호출되었당께");
                OutputWindow.OutputString(DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss") + GotFocus.Caption + "이 포커싱 읾음" + $"{GetIME()}-->{ReadHanEngType()}" + Environment.NewLine);
            }
        }
        */

        private static string GetIME()
        {
            IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            IntPtr hime = ImmGetDefaultIMEWnd(hWnd);
            IntPtr status = SendMessage(hime, WM_IME_CONTROL, new IntPtr(0x5), new IntPtr(0));

            return status.ToInt32() != 0 ? "Han" : "Eng";
        }

        private static void WriteHanEngType(string inputString)
        {
            string folderName = Resources.FolderPath;    // 파일 위치. 리소스 문자열로 변경 "C:\DaviSqlSsms";

            DirectoryInfo di = new DirectoryInfo(folderName);
            if (di.Exists == false)
                di.Create();

            using (StreamWriter sw = new StreamWriter(folderName + "/" + Resources.ConfigFileName))
            {
                sw.Write(inputString);
                //sw.WriteLine(inputString.Trim());
            }
            //   File.WriteAllText(folderName + "/HanEngConfig.txt", "Han고정", Encoding.Default);
            //writeTextFile
        }

        private static string ReadHanEngType()
        {
            // 파일 위치
            string folderName = Resources.FolderPath;    // 리소스 문자열로 변경 "C:/SSMSPlayWith";

            DirectoryInfo di = new DirectoryInfo(folderName);
            return File.ReadAllText(folderName + "/" + Resources.ConfigFileName).TrimEnd();
        }

        /*
        private async void OnButtonClickAsync(object sender, EventArgs e)
        {
            var obj = sender as OleMenuCommand;

            //ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "버튼 {0}", obj.CommandID.ID == CommandId2 ? "한글버튼" : "영어버튼");
            string title = "MainCommand창";

            //// Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            //OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;

            var mcs = await this.ServiceProvider.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(mcs);

            var newCmdID2 = new CommandID(new Guid(DaviSqlSsmsPackage.MainCommandPackageGuidString), CommandId2);
            var newCmdID3 = new CommandID(new Guid(DaviSqlSsmsPackage.MainCommandPackageGuidString), CommandId3);

            OleMenuCommand omc2 = (OleMenuCommand)mcs.FindCommand(newCmdID2);
            OleMenuCommand omc3 = (OleMenuCommand)mcs.FindCommand(newCmdID3);
            if (obj.CommandID.ID == CommandId2)
            {
                //obj.Visible = false;
                omc3.Enabled = true;
                omc2.Enabled = false;
            }
            else
            {
                //obj.Visible = false;
                omc2.Enabled = true;
                omc3.Enabled = false;
            }

            string curIMEMode = GetIME();
            WriteHanEngType(curIMEMode);
            //TestMethod();
        }
        */

        private static IntPtr SetHook(LocalKeyboardHookProc proc)
        {
            using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    //return SetWindowsHookEx(WH_KEYBOARD_LL, proc, curProcess.Handle, 0);
                    //return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);        // 1. 첫번째 버전
                    //return SetWindowsHookEx(WH_KEYBOARD_LL, proc, curModule.BaseAddress, 0);                           // 2. 두번째 버전
                    //return SetWindowsHookEx(WH_KEYBOARD_LL, proc, curModule.BaseAddress, threadID);                           // 2. 두번째 버전
                    return SetWindowsHookEx(WH_KEYBOARD, proc, IntPtr.Zero, (uint)curProcess.Threads[0].Id);            // 2. 로컬 후킹
                    //return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), (uint)Thread.CurrentThread.ManagedThreadId);
                    //return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 3);
                }
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IntPtr mainWinHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
           
            string temp = "";
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();          //현재 활성화된 창

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                temp = Buff.ToString();
            }

            string keyStatus = ((int)lParam & 2147483648) == 0 ? "KeyDown" : "KeyUp";

            //OutputWindow.OutputString($"{DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")} 후킹버튼눌려짐 nCode({nCode}) WindowText({temp}) {keyStatus}" + Environment.NewLine);
            //var ttt = System.Windows. Application.Current;//   .Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);            

            //02. 일반로직

            #region 전역후킹일때
            /*
                if (nCode >= 0)// && wParam == (IntPtr)WM_KEYDOWN)
                {
                    //int vkCode = Marshal.ReadInt32(lParam);

                    //if ((Keys)vkCode == Keys.KanaMode)
                    if ((int)wParam == 17)
                    {
                        WriteHanEngType(GetIME() == "Han" ? "Eng" : "Han");
                        WriteLogFile("한영전환버튼만");
                        Debug.WriteLine("한영전환버튼만");
                    }
                    else
                    {
                        //WriteLogFile("키보드:" + (Keys)vkCode);
                        Debug.WriteLine($"Wparam:{wParam}");
                        Debug.WriteLine($"lparam:{lParam}");
                        Debug.WriteLine("키보드:" + ((char)wParam).ToString());
                    }
                }
                */
            #endregion

            //로컬 후킹일때
            //lparam 31번째 0이면 눌려짐. 1이면 떼짐.
            //현재는 두개 이벤트가 다 들어오기 때문에 비트연산으로 31번째 비트가 0인지 1인지 알아내야 함
            //최상위 비트가 1[2147483648]이면 WM_KEYUP. 0[0]이면 WM_KEYDOWN.
            // nCode는 3 or 0가 들어오는데 0이 정상적인 입력 체크이고 3은 사전작업 같음. 불일치일때 WM_KEYDOWN의 사전작업(3)때 강제한영전환이 필요한듯.

            // 지역 hook일때 nCode(3), WM_KEDOWN --> nCode(0), WM_KEYDOWN --> nCode(3), WM_KEYUP --> nCode(0), WM_KEYUP 총 4번 호출되는듯 예상. 정확치는 않음.

            if ((int)wParam != 21 && nCode == 3 && (((int)lParam & 2147483648) == 0)) // 사전작업(3), WM_KEYDOWN, 한영키가 아닐때만
            {

                //Debug.WriteLine($"nCode:{nCode}");
                //Debug.WriteLine($"Wparam:{wParam}");
                //Debug.WriteLine($"lparam:{lParam}");
                //Debug.WriteLine("키보드:" + ((char)wParam).ToString());
                //Debug.WriteLine($"눌려짐확인: {(int)lParam & 2147483648}");

                string txtMsg;
                string nowIme = GetIME();
                string nowHanEngText = ReadHanEngType();

                try
                {
                    if (nowIme != nowHanEngText) //설정파일의 한영과 현재의 한영이 다르면 설정의 자판으로 변경
                    {
                        //SetCurrentLang(GetModuleHandle(curModule.ModuleName), ReadHanEngType());        //01. 일단 한영으로 변경하고
                        //SetCurrentLang(Process.GetCurrentProcess().Handle, ReadHanEngType());        //01. 일단 한영으로 변경하고
                        //SetCurrentLang(ImmGetDefaultIMEWnd(Process.GetCurrentProcess().MainWindowHandle), ReadHanEngType());
                        SetCurrentLang(mainWinHandle, ReadHanEngType());

                        //SetCurrentLang(hwnd, ReadHanEngType());        //01. 일단 한영으로 변경하고
                        //Debug.WriteLine("강제한영전환");                        

                        txtMsg = "강제한영전환";
                        WriteLogFile(txtMsg);
                        OutputWindow.OutputString($"{DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")} {txtMsg} {nowIme}-->{nowHanEngText}" + Environment.NewLine);
                    }

                    /*
                    // 편집기 창에만 한영버튼이 눌려져야 한다.
                    if ((int)wParam == 21)  //한영버튼
                    {
                        txtMsg = "한영전환";
                        string finalIme = nowIme == "Han" ? "Eng" : "Han";

                        WriteHanEngType(finalIme);
                        WriteLogFile(txtMsg);

                        OutputWindow.OutputString($"{DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")} {txtMsg} {nowIme}-->{finalIme}" + Environment.NewLine);
                        //Debug.WriteLine("한영전환버튼만");

                        //Debug.WriteLine($"Wparam:{wParam}");
                        //Debug.WriteLine($"lparam:{lParam}");
                        //Debug.WriteLine("키보드:" + ((char)wParam).ToString());
                        //Debug.WriteLine($"눌려짐확인: {(int)lParam & 2147483648}");
                    }
                    else
                    {
                        //Debug.WriteLine($"Wparam:{wParam}");
                        //Debug.WriteLine($"lparam:{lParam}");
                        //Debug.WriteLine("키보드:" + ((char)wParam).ToString());
                        //Debug.WriteLine($"눌려짐확인: {(int)lParam & 2147483648}");
                    }
                    */
                }
                catch (IOException err)
                {
                    MessageBox.Show("IOError :" + err.Message + Environment.NewLine + "Hooking close");
                    UnhookWindowsHookEx(_hookID);
                }
            }

            if ((int)wParam == 21 && nCode == 0 && (((int)lParam & 2147483648) != 0)) // 한영키, 사전작업 아님(0), WM_KEYUP 일때만
            {

                //Debug.WriteLine($"nCode:{nCode}");
                //Debug.WriteLine($"Wparam:{wParam}");
                //Debug.WriteLine($"lparam:{lParam}");
                //Debug.WriteLine("키보드:" + ((char)wParam).ToString());
                //Debug.WriteLine($"눌려짐확인: {(int)lParam & 2147483648}");

                string txtMsg;
                //string nowIme = GetIME();
                string nowHanEngText = ReadHanEngType();

                try
                {

                    txtMsg = "한영전환";
                    string finalIme = nowHanEngText == "Han" ? "Eng" : "Han";

                    WriteHanEngType(finalIme);
                    WriteLogFile(txtMsg);

                    OutputWindow.OutputString($"{DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")} {txtMsg} {nowHanEngText}-->{finalIme}" + Environment.NewLine);


                }
                catch (IOException err)
                {
                    MessageBox.Show("IOError :" + err.Message + Environment.NewLine + "Hooking close");
                    UnhookWindowsHookEx(_hookID);
                }
            }

            //ToolButtonVisible(GetIME());
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void SetCurrentLang(IntPtr handle, string inputLang)
        {
            //int readType = GCS_COMPSTR;
            IntPtr hIMC = ImmGetContext(handle);
            try
            {
                int langNO;
                /*
                int lpConversion;
                int lpSentence;

                ImmGetConversionStatus(hIMC, out lpConversion, out lpSentence);
                */
                //Debug.Print(lpConversion.ToString());

                //ImmSetConversionStatus(hIMC, 0, 0); // 일단 무조건 한글으로
                //ImmSetConversionStatus(hIMC, 1, 0); // 일단 무조건 영문으로

                langNO = inputLang == "Han" ? 1 : 0;

                ImmSetConversionStatus(hIMC, langNO, 0);
            }
            finally
            {
                ImmReleaseContext(handle, hIMC);
            }
        }

        private static void WriteLogFile(string str)
        {
            string yyyymmdd = DateTime.Now.ToString("yyyyMMdd");
            string logFileFullPath = Resources.FolderPath + "/" + Resources.LogFileNamePrefix + yyyymmdd + ".txt";

            //var myFile = File.Create(myPath);
            //myFile.Close();
            // file.Create는 FileStream을 오픈하기 때문에 항상 Close하지 않으면 다른 프로세스에서 접근할수 없다는 오류 뜸
            //FileStream fs;

            if (!File.Exists(logFileFullPath))
            {
                //var fs = File.Create(logFileFullPath);
                //fs.Close();

                File.Create(logFileFullPath).Close();
            }

            File.AppendAllText(logFileFullPath, DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + " : " + str + Environment.NewLine);
        }

        //private void Command_AutoFixStart(object sender, EventArgs e)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
        //    string title = "AssistMain";

        //    // Show a message box to prove we were here
        //    VsShellUtilities.ShowMessageBox(
        //        this.package,
        //        message,
        //        title,
        //        OLEMSGICON.OLEMSGICON_INFO,
        //        OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        //}

        private void Command_Exec(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menuCommand)
            {               
                var executor = new Executor(dte);
                var scope = GetScope(menuCommand.CommandID.ID);

                executor.ExecuteStatement(scope);
            }
        }

        private void Command_QueryStatus(object sender, EventArgs e)
        {
            //string txtMsg;

            ThreadHelper.ThrowIfNotOnUIThread();
            if (sender is OleMenuCommand menuCommand)
            {
                if (menuCommand.Checked)  //Autofix 버튼 반전
                {
                    menuCommand.Checked = false;
                    //txtMsg = "LangAutoFix End";
                }
                else
                {
                    menuCommand.Checked = true;
                    //txtMsg = "LangAutoFix Start";
                }
                
                //OutputWindow.OutputString($"{DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss")} {txtMsg} {GetIME()}-->{ReadHanEngType()}" + Environment.NewLine);
            }
        }
    }
}
