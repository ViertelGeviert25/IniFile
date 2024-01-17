using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace IniFile
{

    public static class AutomationExtensions
    {
        public static string GetText(this AutomationElement element)
        {
            object patternObj;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }
            else
            {
                return element.Current.Name;
            }
        }
    }

    public class Program
    {
        static void AutomateCalc()
        {

            var psi = new ProcessStartInfo("calc.exe")
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Minimized,
                CreateNoWindow = true
            };
            var proc = Process.Start(psi);

            // "C:\Program Files (x86)\Windows Kits\10\bin\10.0.xx.0\x64\inspect.exe"

            // https://stackoverflow.com/questions/66928721/c-sharp-automation-click-button-on-windows-10-calculator


            Thread.Sleep(2000);

            AutomationElement calculatorElement = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "Rechner"));

            if (calculatorElement != null)
            {
                AutomationElement inputBoxElement = calculatorElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "num9Button"));

                var invokePattern = inputBoxElement.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();

                AutomationElement plusButton = calculatorElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "plusButton"));

                invokePattern = plusButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();

                invokePattern = inputBoxElement.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();

                AutomationElement equalButton = calculatorElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "equalButton"));

                invokePattern = equalButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();

                AutomationElement calcResult = calculatorElement.FindFirst(
            TreeScope.Descendants,
            new PropertyCondition(AutomationElement.AutomationIdProperty, "CalculatorResults"));

                Console.WriteLine(calcResult.GetText());
            }
            else
            {
                Console.WriteLine("Calculator not found.");
            }

            proc.Close();
        }



        static void TestIniFile()
        {
            var iniFilePath = @"H:\testdata\myIni.txt";
            var iniFile = new IniFile(iniFilePath);


            var foundSection = iniFile.TryGetSectionByKeyAndValue("Type", "t1", out string section);
            if (foundSection)
            {
                Console.WriteLine(section);
                iniFile.WriteEntry(section, "Path", @"E:\temp\dt.txt");
            }

            var myEntries = iniFile.GetEntries("ws");

            iniFile.WriteEntries("doc2", myEntries);
        }

        public static void Main(string[] args)
        {
            AutomateCalc();

            Console.ReadKey();
        }
    }
}
