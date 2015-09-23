using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnitLite.Runner;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NUnitLiteForUnity
{
    public static class NUnitLiteForUnityTestRunner
    {
        #region Public Methods

        public static void RunTests(string assemblyName = "Assembly-CSharp")
        {
            ITestAssemblyRunner testRunner = new NUnitLiteTestAssemblyRunner(new NUnitLiteTestAssemblyBuilder());

            Assembly testAssembly;

            try
            {
                testAssembly = Assembly.Load(assemblyName);
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Error encountered when loading assembly {0} - {0}", assemblyName, e.ToString()));
                return;
            }

            bool hasTestLoaded = testRunner.Load(assembly: testAssembly, settings: new Hashtable());
            if (!hasTestLoaded)
            {
                Debug.Log(string.Format("No tests found in assembly {0}", testAssembly.GetName().Name));
                return;
            }

            ITestResult rootTestResult = testRunner.Run(TestListener.NULL, TestFilter.Empty);

            ResultSummary summary = new ResultSummary(rootTestResult);
            Debug.Log(ToSummryString(summary));

            List<ITestResult> testResultList = EnumerateTestResult(rootTestResult).Where(it => !it.HasChildren).ToList();

            if (summary.FailureCount > 0 || summary.ErrorCount > 0)
            {
                DebugLogErrorResults(testResultList);
            }

            if (summary.NotRunCount > 0)
            {
                DebugLogNotRunResults(testResultList);
            }
        }

        #endregion Public Methods



        #region Private Methods

        private static string ToSummryString(ResultSummary resultSummry)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                stringWriter.Write("{0} Tests :", resultSummry.TestCount);
                stringWriter.Write(" {0} Pass", resultSummry.PassCount);

                if (resultSummry.FailureCount > 0)
                {
                    stringWriter.Write(", {0} Failure", resultSummry.FailureCount);
                }

                if (resultSummry.ErrorCount > 0)
                {
                    stringWriter.Write(", {0} Error", resultSummry.ErrorCount);
                }

                if (resultSummry.NotRunCount > 0)
                {
                    stringWriter.Write(", {0} NotRun", resultSummry.NotRunCount);
                }

                if (resultSummry.InconclusiveCount > 0)
                {
                    stringWriter.Write(", {0} Inconclusive", resultSummry.InconclusiveCount);
                }

                return stringWriter.GetStringBuilder().ToString();
            }
        }

        private static IEnumerable<ITestResult> EnumerateTestResult(ITestResult result)
        {
            if (result.HasChildren)
            {
                foreach (ITestResult child in result.Children)
                {
                    foreach (ITestResult c in EnumerateTestResult(child))
                    {
                        yield return c;
                    }
                }
            }
            yield return result;
        }

        private static void DebugLogErrorResults(IEnumerable<ITestResult> testResults)
        {
            foreach (ITestResult testResult in testResults)
            {
                if (testResult.ResultState == ResultState.Error || testResult.ResultState == ResultState.Failure)
                {
                    string foramt = "{0}\n{1} ({2})\n{3}\n{4}";
                    string log = string.Format(foramt, testResult.ResultState, testResult.Name, testResult.FullName, testResult.Message, testResult.StackTrace);
                    Debug.Log(log);
                }
            }
        }

        private static void DebugLogNotRunResults(IEnumerable<ITestResult> testResults)
        {
            foreach (ITestResult testResult in testResults)
            {
                if (testResult.ResultState == ResultState.Ignored || testResult.ResultState == ResultState.NotRunnable || testResult.ResultState == ResultState.Skipped)
                {
                    string foramt = "{0}\n{1} ({2})\n{3}";
                    string log = string.Format(foramt, testResult.ResultState, testResult.Name, testResult.FullName, testResult.Message);
                    Debug.Log(log);
                }
            }
        }

        #endregion Private Methods
    }
}