using Microsoft.Win32;
using NaviSqlSsms.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviSqlSsms.Modules
{
    class NaviSettingsManager
    {
        private static RegistryKey GetRoot()
        {
            var settingKeyRoot = Registry.CurrentUser.CreateSubKey(@"AxialSqlTools");
            var settingsKey = settingKeyRoot.CreateSubKey("Settings");

            return settingsKey;
        }

        private static string GetRegisterValue(string parameter)
        {
            try
            {
                using (var rootKey = GetRoot())
                {
                    var value = rootKey.GetValue(parameter);
                    return value?.ToString() ?? string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool GetApplyAdditionalCodeFormatting()
        {
            //bool result = false;

            //bool success = bool.TryParse(GetRegisterValue("ApplyAdditionalCodeFormat"), out result);  // axial쪽 레지스트리 이용 코드
            //bool success = bool.TryParse(GetRegisterValue("ApplyAdditionalCodeFormatYn"), out result); //리소스 문자열 이용코드
            //return result;


            bool isCodeFormatYn = false;
            string codeFormatString = Resources.ApplyAdditionalCodeFormatYn;        //내 리소스 문자열 이용코드
            if (string.IsNullOrEmpty(codeFormatString))
            {
                isCodeFormatYn = false;
            }
            else
            {
                isCodeFormatYn = codeFormatString.ToLower() == "y" ? true : false;
            }            
            
            return isCodeFormatYn;
        }
    }
}
