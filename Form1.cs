using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;

namespace CodeDOMDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textEditor = new TextEditor();
            textEditor.ShowLineNumbers = true;
            textEditor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            textEditor.FontSize = 14;
            textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            string dir = @"D:\";
            if (File.Exists(dir + "CSCodeScript.cs"))
            {
                textEditor.Load(dir + "CSCodeScript.cs");
            }
            elementHost1.Child = textEditor;


            mMyCodeScript.CreateCompiler("C#", true, "BBBBB");
            button2.Enabled = mMyCodeScript.IsComplied;
        }

        private MyCodeScript mMyCodeScript = new MyCodeScript();
        TextEditor textEditor;
        //编译
        private void button1_Click(object sender, EventArgs e)
        {
            mMyCodeScript.SourceText = textEditor.Text;
            mMyCodeScript.Compile();
            richTextBox1.AppendText(mMyCodeScript.CompilerInfo);
            button2.Enabled = mMyCodeScript.IsComplied;
        }
        //执行
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                mMyCodeScript.Invoke("CodeDOMDemo1.CSCodeScript", "DO", new object[] { });
            }
            catch (Exception ee)
            {
                richTextBox1.AppendText(ee.Message + "\r\n");
            }
        }
        //保存
        private void button3_Click(object sender, EventArgs e)
        {
            string dir = @"D:\";
            textEditor.Save(dir + "CSCodeScript.cs");
        }
    }


    public class cccc
    {
        public static void ABC()
        {
            MessageBox.Show("ABC");
        }
    }
    public class MyCodeScript
    {
        CompilerParameters theParameters;
        CodeDomProvider theProvider;
        public void CreateCompiler(string strLanguage, bool DebugMode, string strAssemblyFileName)
        {
            theParameters = new CompilerParameters();
            theParameters.OutputAssembly = System.IO.Path.Combine(System.IO.Path.GetTempPath(), strAssemblyFileName + ".dll");
            theParameters.GenerateExecutable = false;
            theParameters.GenerateInMemory = true;
            if (DebugMode)
            {
                theParameters.IncludeDebugInformation = true;
                //theParameters.CompilerOptions += "#define TRACE=1; #define DEBUG=1";
            }
            else
            {
                theParameters.IncludeDebugInformation = false;
                //theParameters.CompilerOptions += "#define TRACE=1";
            }
            theParameters.ReferencedAssemblies.Add("System.dll");
            theParameters.ReferencedAssemblies.Add("System.Data.dll");
            theParameters.ReferencedAssemblies.Add("System.Xml.dll");
            theParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            theParameters.ReferencedAssemblies.Add(@"D:\AA\CodeDOMDemo\CodeDOMDemo\bin\Debug\CodeDOMDemo.exe");
            //if(strLanguage=="c#")
            theProvider = new Microsoft.CSharp.CSharpCodeProvider();
            //if (!DebugMode)
                //theParameters.CompilerOptions += "/optimize";

        }
        string theCompilerInfo;
        public string CompilerInfo
        {
            get { return theCompilerInfo; }
        }
        bool isCompiled = false;
        public bool IsComplied
        {
            get { return isCompiled; }
        }
        public Assembly theCompiledAssembly = null;
        CompilerResults theCompilerResults;
        string _SourceText = "";
        public string SourceText
        {
            get { return _SourceText; }
            set { _SourceText = value; }
        }
        /// <summary>
        /// 编译
        /// </summary>
        /// <returns></returns>
        public bool Compile()
        {
            theCompilerInfo = "";
            isCompiled = false;
            theCompiledAssembly = null;
            theCompilerResults = theProvider.CompileAssemblyFromSource(theParameters, _SourceText);
            if (theCompilerResults.NativeCompilerReturnValue == 0)
            {
                isCompiled = true;
                theCompiledAssembly = theCompilerResults.CompiledAssembly;
            }
            StringBuilder sbCompilerInfo = new StringBuilder();
            foreach (CompilerError err in theCompilerResults.Errors)
            {
                sbCompilerInfo.Append(err.ToString() + "\r\n");
            }
            theCompilerInfo = sbCompilerInfo.ToString();
            return isCompiled;
        }

        public object Invoke(string StrModule, string StrMethod, object[] Arguments)
        {
            if (!isCompiled || theCompiledAssembly == null)
                throw new Exception("脚本程序没有成功编译！");
            Type[] temp = theCompiledAssembly.GetTypes();
            Type _Moduletype = theCompiledAssembly.GetType(StrModule);
            if (_Moduletype == null)
                throw new Exception(string.Format("指定的类或模块({0})", StrModule));
            MethodInfo _MothodInfo = _Moduletype.GetMethod(StrMethod);
            if (_MothodInfo == null)
                throw new Exception(string.Format("指定方法({0}::{1})未定义", StrModule, StrMethod));
            try
            {
                return _MothodInfo.Invoke(null, Arguments);
            }
            catch (TargetParameterCountException ex)
            {
                throw new Exception(string.Format("指定方法({0}::{1})参数错误", StrModule, StrMethod));
            }
            catch (Exception ex1)
            {
                throw ex1;
            }
        }
    }
}
