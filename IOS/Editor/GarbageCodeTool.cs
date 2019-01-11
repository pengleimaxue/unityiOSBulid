
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
//垃圾代码参数
  public class GarbageCodeParam {
      public string className;         //类名
      public string mainFuncName;      //入口函数名
      public string mainFuncParam1;    //入口函数参数1
      public string mainFuncParam2;    //入口函数参数2
      public string mainFuncParam3;    //入口函数参数3
  }

//垃圾代码生成器
public class GarbageCodeTool : EditorWindow
{
    //生成垃圾代码函数模版个数
    private const int funcTplCount = 1;
    //函数模版委托:入口函数名,入口函数参数1，入口函数参数2，入口函数参数3 ；返回生成该函数的字符串
    private delegate string FuncTplHandle(string methonName, string param1, string param2, string param3);
    //函数模版数组
    private static FuncTplHandle[] arrFuncTplHandle;
    //管理所有垃圾代码的文件名
    private const string garbageCodeManagerName = "CallAllCodeManager";
    private static bool isGenerateXcode = false;    

    //c#垃圾代码变量
    private const  string namespaceName = "PLFramework";
    //生成代码的命名空间    
    private static string mstCreateCodeFilePath =                                 //生成代码路径
        UnityEngine.Application.dataPath + "/GarbageCode";  
    private const  int    maxFileCount = 1000;                                     //垃圾代码个数

    //Xcode垃圾代码变量
    private static string mstCreateXCodeFilePath =                                //生成XCode代码路径
        UnityEngine.Application.dataPath + "/GarbageXCode";
    private const int maxXCodeFileCount = 50;                                     //XCode垃圾代码个数
	private static string  codeFilePath=                                 //已有C#代码路径
		UnityEngine.Application.dataPath + "/Script"; 
    //生成C#垃圾代码
	[MenuItem("Tools/生成C#垃圾代码")]
    static void GenerateGarbageCode() {
        isGenerateXcode = false;

        //删除旧目录代码
        if (Directory.Exists(mstCreateCodeFilePath))
        {
            Directory.Delete(mstCreateCodeFilePath, true);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

#if !UNITY_IPHONE
        //只有苹果才生成垃圾代码
        return;
#endif
       
        //重新创建目录
        Directory.CreateDirectory(mstCreateCodeFilePath);
        Directory.CreateDirectory(mstCreateCodeFilePath + "/Code");

        //获取本地时间作为生成md5条件之一
        string timeNow = DateTime.Now.ToString();
        string[] arrClassName  = new string[maxFileCount];
        string[] arrMethonName = new string[maxFileCount];

#region 生成所有垃圾代码
        StringBuilder stringBuilder = null;
        for (int i = 0; i < maxFileCount; i++) {
            //获取垃圾代码参数
            GarbageCodeParam funcTplParam = GetFuncTplParam(timeNow,i);

            //生成文件名
            string fileName = Path.Combine(mstCreateCodeFilePath + "/Code", funcTplParam.className + ".cs");
            if (string.IsNullOrEmpty(fileName))
            {
                continue;
            }

            //构造生成文本对象
            stringBuilder = new StringBuilder();

            //添加命名空间
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendFormat(string.Format("namespace {0}\n", namespaceName));
                stringBuilder.AppendLine("{");
            }
            stringBuilder.AppendLine("");

            //生成类名
            stringBuilder.AppendFormat("public class {0} \n", funcTplParam.className);
            stringBuilder.AppendLine("{");

            //随机选择一种函数模版作为类函数
            string funcContent = GetFuncContent(funcTplParam.mainFuncName, 
                funcTplParam.mainFuncParam1, funcTplParam.mainFuncParam2, funcTplParam.mainFuncParam3);
            stringBuilder.Append(funcContent);

            //添加最后的括号，保存文件
            stringBuilder.AppendLine("}");
            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.AppendLine("}");
            }
            File.WriteAllText(fileName, stringBuilder.ToString());

            arrClassName[i]  = funcTplParam.className;
            arrMethonName[i] = funcTplParam.mainFuncName;
        }
#endregion 生成所有垃圾代码

#region 生成调用所有垃圾代码的代码
        string managerFilePath = Path.Combine(mstCreateCodeFilePath, garbageCodeManagerName + ".cs");
        if (string.IsNullOrEmpty(managerFilePath))
        {
            return;
        }
        stringBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(namespaceName))
        {
            stringBuilder.AppendFormat(string.Format("namespace {0}\n", namespaceName));
            stringBuilder.AppendLine("{");
        }
        stringBuilder.AppendLine("using UnityEngine;");
        stringBuilder.AppendLine("using System.Collections;");
        stringBuilder.AppendLine("");
        stringBuilder.AppendLine("namespace PLFramework {");
        stringBuilder.AppendLine("    //垃圾代码管理器");
        stringBuilder.AppendFormat("public class {0}\n", garbageCodeManagerName);
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        //调用所有垃圾代码");
        stringBuilder.AppendLine("        public static void CallAllGarbageCode() {");


        //调用所有垃圾代码
        Random rd = new Random();
        for (int i = 0; i < arrClassName.Length; i++)
        {
            string className = arrClassName[i];
            string methonName = arrMethonName[i];
            if (className == "" || methonName == "")
            {
                continue;
            }
            int randa = rd.Next(0, 1000);
            int randb = rd.Next(0, 1000);
            int randc = rd.Next(0, 1000);
            stringBuilder.AppendFormat("            {0} _{1} = new {2}();\n", className, className, className);
            if (randc > 500)
            {
                stringBuilder.AppendFormat("            _{0}.{1}({2},{3});\n", className, methonName, randa, randb);
            }
            else
            {
                stringBuilder.AppendFormat("            _{0}.{1}({2},{3},{4});\n", className, methonName, randa, randb, randc);
            }
            stringBuilder.AppendLine("");
        }


        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine("");
        if (!string.IsNullOrEmpty(namespaceName))
        {
            stringBuilder.AppendLine("}");
        }
        File.WriteAllText(managerFilePath, stringBuilder.ToString());
#endregion 生成调用所有垃圾代码的代码

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

		EditorUtility.DisplayDialog("生成C#垃圾代码","生成完毕！", "确定");
    }

    //生成Xcode垃圾代码
	[MenuItem("Tools/生成Xcode垃圾代码")]
    static void GenerateXCodeGarbageCodes()
    {
        isGenerateXcode = true;

        //删除旧目录代码
        if (Directory.Exists(mstCreateXCodeFilePath))
        {
            Directory.Delete(mstCreateXCodeFilePath, true);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

#if !UNITY_IPHONE
        //只有苹果才生成垃圾代码
        return;
#endif

        //重新创建目录
        Directory.CreateDirectory(mstCreateXCodeFilePath);
        Directory.CreateDirectory(mstCreateXCodeFilePath + "/Code");

        //获取本地时间作为生成md5条件之一
        string timeNow = DateTime.Now.ToString();
        string[] arrClassName = new string[maxXCodeFileCount];
        string[] arrMethonName = new string[maxXCodeFileCount];

#region 生成所有xcode垃圾代码     
        StringBuilder stringBuilder = null;
        for (int i = 0; i < maxXCodeFileCount; i++)
        {
            //获取垃圾代码参数
            GarbageCodeParam funcTplParam = GetFuncTplParam(timeNow, i);

            //生成文件名
            string fileName = Path.Combine(mstCreateXCodeFilePath + "/Code", funcTplParam.className + ".mm");
            if (string.IsNullOrEmpty(fileName))
            {
                continue;
            }

            //构造生成文本对象
            stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("");

            //随机选择一种函数模版作为类函数
            string funcContent = GetFuncContent(funcTplParam.mainFuncName,
                funcTplParam.mainFuncParam1, funcTplParam.mainFuncParam2, funcTplParam.mainFuncParam3);
            stringBuilder.Append(funcContent);

            //保存文件
            File.WriteAllText(fileName, stringBuilder.ToString());

            arrClassName[i] = funcTplParam.className;
            arrMethonName[i] = funcTplParam.mainFuncName;
        }
#endregion 生成所有xcode垃圾代码

#region 生成调用所有垃圾代码的代码
        string managerFilePath = Path.Combine(mstCreateXCodeFilePath, garbageCodeManagerName + ".mm");
        if (string.IsNullOrEmpty(managerFilePath))
        {
            return;
        }
        stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("        //调用所有代码");
        stringBuilder.AppendFormat("         void {0}()\n", garbageCodeManagerName);
        stringBuilder.AppendLine("{");

        //调用所有垃圾代码
        Random rd = new Random();
        for (int i = 0; i < arrMethonName.Length; i++)
        {
            string methonName = arrMethonName[i];
            if (methonName == "")
            {
                continue;
            }
            int randa = rd.Next(0, 1000);
            int randb = rd.Next(0, 1000);
            int randc = rd.Next(0, 1000);

            stringBuilder.AppendFormat("            extern int {0}(int a,int b,int c = 0);\n", methonName);
            if (randc > 500)
            {
                stringBuilder.AppendFormat("            {0}({1},{2});\n", methonName, randa, randb);
            }
            else
            {
                stringBuilder.AppendFormat("            {0}({1},{2},{3});\n", methonName, randa, randb, randc);
            }
            stringBuilder.AppendLine("");
        }
        stringBuilder.AppendLine("}");
        stringBuilder.AppendLine("");
        File.WriteAllText(managerFilePath, stringBuilder.ToString());
#endregion 生成调用所有垃圾代码的代码

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

		EditorUtility.DisplayDialog("xcode垃圾代码", "生成完毕！", "确定");
    }

	[MenuItem("Tools/C#加入混淆代码")]

	public  static void addGarbageCodeForExistCSFile() {
		#if !UNITY_IPHONE
		//只有苹果才生成垃圾代码
		return;
		#endif

		if (Directory.Exists(codeFilePath))
		{
			plSearchFiles(codeFilePath);
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();			
			EditorUtility.DisplayDialog("混淆C#垃圾代码","生成完毕！", "确定");
		}
	}

	 public static void plSearchFiles(string filePath) {

      DirectoryInfo fileDir = new DirectoryInfo(filePath);

      FileSystemInfo[] fsinfos = fileDir.GetFileSystemInfos();
		foreach (FileSystemInfo fsinfo in fsinfos) {
			string fileFullName = fsinfo.FullName;
			if (Directory.Exists(fileFullName)) {
				plSearchFiles(fsinfo.FullName);
			}
			else {

				if ( fileFullName.EndsWith(".cs") ) 
				{
					//Encoding encoding = GetType(fileFullName);
					StreamReader sr = new StreamReader(fileFullName, System.Text.Encoding.UTF8);
					String fileContent = sr.ReadToEnd().TrimStart();
					sr.Close();
					sr.Dispose();
					if (!string.IsNullOrEmpty(fileContent))
					{
						string timeNow = DateTime.Now.ToString();
						GarbageCodeParam funcTplParam = GetFuncTplParam(timeNow+fsinfo.Name,UnityEngine.Random.Range(fsinfos.Length, fsinfos.Length*2));
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendLine(fileContent);
						stringBuilder.AppendLine("");
						stringBuilder.AppendFormat("public class {0} \n", funcTplParam.className);
						stringBuilder.AppendLine("{");
						
			
						string funcContent = GetFuncContent(funcTplParam.mainFuncName, 
						                                    funcTplParam.mainFuncParam1, funcTplParam.mainFuncParam2, funcTplParam.mainFuncParam3);
						stringBuilder.Append(funcContent);
					
						stringBuilder.AppendLine("}");
						File.WriteAllText(fsinfo.FullName, stringBuilder.ToString());
						//关闭流
						sr.Close();
						//销毁流
						sr.Dispose();
					} else {
						sr.Close();
						sr.Dispose();
					}
				    
				}

			}
		}
}
    //根据时间字符加序号产生垃圾代码参数
    public static GarbageCodeParam GetFuncTplParam(string timeNow,int index) {      
		//根据当前时间+序号+当前时间戳 生成md5值
		string md5 = CalcMd5(timeNow + index.ToString()+(System.DateTime.Now).ToFileTime().ToString());
		md5 = "_" + md5;

        //用md5值生成类名、入口函数名、参数名
        GarbageCodeParam garbageCodeParam  = new GarbageCodeParam();
        garbageCodeParam.className      = md5;
        garbageCodeParam.mainFuncName   = md5 + "m";
        garbageCodeParam.mainFuncParam1 = md5 + "a";
        garbageCodeParam.mainFuncParam2 = md5 + UnityEngine.Random.Range(0, 100);
        garbageCodeParam.mainFuncParam3 = md5 + "c";
        return garbageCodeParam;
    }

    //获取函数体（根据已有模版随机生成）
    public static string GetFuncContent(string methonName, string param1, string param2, string param3) {
#if !UNITY_IPHONE
        //只有苹果才生成垃圾代码
        return "";
#endif 

        //如果没有初始化则初始化函数模版
        if (null == arrFuncTplHandle || arrFuncTplHandle.Length < funcTplCount) {
            arrFuncTplHandle = new FuncTplHandle[funcTplCount];
            arrFuncTplHandle[0] = FuncTpl1;
        }

        //随机一种模版生成函数体
        int randNum = UnityEngine.Random.Range(0, funcTplCount);
        if (arrFuncTplHandle[randNum] == null)
        {
			UnityEngine.Debug.LogError("不存在范式，索引：" + randNum.ToString());
            return "";
        }
        return arrFuncTplHandle[randNum](methonName, param1, param2, param3);
    }

    /// <summary>
    /// 计算字符串的MD5值
    /// </summary>
    static string CalcMd5(string source)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
        byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
        md5.Clear();

        string destString = "";
        for (int i = 0; i < md5Data.Length; i++)
        {
            destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
        }
        destString = destString.PadLeft(32, '0');
        return destString;
    }


    //函数模版1
    static string FuncTpl1(string methonName, string param1, string param2, string param3)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("    int {0}2(int {1})\n", methonName, param1);
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendFormat("        return (int)(3.1415926535897932384626433832795028841 * {0} * {0});\n", param1);
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("");
        if (isGenerateXcode)
        {
            //入口函数xcode没有public
            stringBuilder.AppendFormat("    int {0}(int {1},int {2},int {3} = 0) \n", methonName, param1, param2, param3);
        }
        else
        {
            stringBuilder.AppendFormat("    public int {0}(int {1},int {2},int {3} = 0) \n", methonName, param1, param2, param3);
        }    
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendFormat("        int t{0}p = {0} * {1};\n", param1, param2);
        stringBuilder.AppendFormat("        if ({1} != 0 && t{0}p > {1})\n", param1,param3);
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendFormat("            t{1}p = t{1}p / {0};\n", param3, param1);
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("        else");
        stringBuilder.AppendLine("        {");
        stringBuilder.AppendFormat("            t{1}p -= {0};\n", param3, param1);
        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("");
        stringBuilder.AppendFormat("        return {0}2(t{1}p);\n", methonName, param1);
        stringBuilder.AppendLine("    }");            
        return stringBuilder.ToString();
    }
 
}