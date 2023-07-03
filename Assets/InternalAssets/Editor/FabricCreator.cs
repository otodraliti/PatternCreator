using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FabricCreator : EditorWindow
{
    private string IClassName = "";
    private string AbstractClassName = "";
    private string ScriptableName = "";

    private string InterfaceFabricName = "";
    private string FabricManagerName = "";

    private string ClassTypeEnumName = "";
    
    private List<string> classNames = new List<string>();
    private int inheritedClassNumber;
    
    
    
    [MenuItem("Custom Tools/Fabric/New Fabric")]
    private static void SetUpFolders()
    {
        // Check if the window is already open
        FabricCreator existingWindow = EditorWindow.GetWindow<FabricCreator>(false, "New Fabric");
        if (existingWindow != null)
        {
            // If the window is open, close it
            existingWindow.Close();
        }

        // Create a new instance of the window
        FabricCreator window = GetWindow<FabricCreator>();
        window.titleContent = new GUIContent("New Fabric");
        window.minSize = new Vector2(800   , 300);
        window.Show();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Enter IClass Name:", EditorStyles.boldLabel);

        IClassName = EditorGUILayout.TextField("IClass Name", IClassName);
        
        GUILayout.Space(10);

        inheritedClassNumber = EditorGUILayout.IntField("Inherited Class Number", inheritedClassNumber);

        GUILayout.Space(10);

        for (int i = 0; i < inheritedClassNumber; i++)
        {
            GUILayout.Label("Inherited Class Name " + (i + 1) + ":", EditorStyles.boldLabel);
            if (i >= classNames.Count)
            {
                classNames.Add("");
            }
            classNames[i] = EditorGUILayout.TextField(classNames[i]);
        }
        
        
        EditorGUILayout.LabelField("Fabric Generator");
        this.Repaint();
        GUILayout.Space(70);
        if (GUILayout.Button("Generate!"))
        {
            SetNames();
            
            CreateObjectInterface();
            CreateAbstractClass();
            CreateObjectClass(classNames);
            CreateScriptableOfClass();

            CreateFabricInterface();
            CreateObjectsFabric(classNames);
            
            CreateFabricManager();

            CreateFabricImplementation();
            
            this.Close();
        }
        if (GUILayout.Button("Close!"))
        {
            this.Close();
        }
    }
    
    private void CreateObjectInterface()
    {
        string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", IClassName + ".cs");
        if (!File.Exists(scriptPath))
        {
            string scriptContent = @"public interface " + IClassName + @"
{
    void Init();
}";
            File.WriteAllText(scriptPath, scriptContent);
        }
        AssetDatabase.Refresh();
    }

    private void SetNames()
    {
        AbstractClassName =  "A" + IClassName.Substring(1);
        ScriptableName = "Scriptable" + IClassName.Substring(1);
        InterfaceFabricName = IClassName + "Fabric";
        FabricManagerName = IClassName.Substring(1) + "FabricManager";
        ClassTypeEnumName = IClassName.Substring(1) + "Type";
    }

    

    private void CreateAbstractClass()
    {
        string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", AbstractClassName + ".cs");
        if (!File.Exists(scriptPath))
        {
            string scriptContent = @"using UnityEngine;
public abstract class " + AbstractClassName + @": MonoBehaviour," + IClassName + @"
{
    public " + ScriptableName + @" objectSettings;
    public void Init()
    {

    }
}";
            File.WriteAllText(scriptPath, scriptContent);
        }
        AssetDatabase.Refresh();
    }
    
    
    private void CreateObjectClass(List<string> ClassName)
    {
        foreach (var className in ClassName)
        {
            string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", className + ".cs");
            if (!File.Exists(scriptPath))
            {
                string scriptContent = @"public class " + className + ": " + AbstractClassName + @"
{


}";
                File.WriteAllText(scriptPath, scriptContent);
            }
        }

        AssetDatabase.Refresh();
    }

    private void CreateScriptableOfClass()
    {
        string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", ScriptableName + ".cs");
        if (!File.Exists(scriptPath))
        {
            string scriptContent = @"
using UnityEngine;
[CreateAssetMenu(fileName = """ + IClassName.Substring(1) + @""", menuName = ""ScriptableObjects/ + " + IClassName.Substring(1)+ "s /" + "new"+ IClassName.Substring(1) + @""")]
    public class " + ScriptableName + @": ScriptableObject
    {
        public " + ClassTypeEnumName  + " " + IClassName.Substring(1).ToLower() +@"Type;
        public GameObject prefab;
    }

";
            File.WriteAllText(scriptPath, scriptContent);
        }
        AssetDatabase.Refresh();
    }

    private void CreateFabricInterface()
    {
        string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", InterfaceFabricName + ".cs");
        if (!File.Exists(scriptPath))
        {
            string scriptContent = @"using UnityEngine;
public interface " + InterfaceFabricName + @"
{
   " + IClassName + @" CreateObject(" + ScriptableName + @" settings, Transform spawnPoint, Transform container);
}";
            File.WriteAllText(scriptPath, scriptContent);
        }
        AssetDatabase.Refresh();
    }
    
    
    private void CreateObjectsFabric(List<string> ClassNemes)
    {
        foreach (var className in ClassNemes)
        {
            var concreteFabricName = className +"Fabric";
            string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", className +"Fabric.cs");
            if (!File.Exists(scriptPath))
            {
                string scriptContent = @"
using UnityEngine;
public class " + concreteFabricName + @": MonoBehaviour," + InterfaceFabricName + @"
{
    private int _ID;
    public " + IClassName + " CreateObject(" + ScriptableName + @" settings, Transform spawnPoint, Transform container)
    {
        var newObject = Instantiate(settings.prefab, spawnPoint.position, spawnPoint.rotation);
        newObject.name += ""ID: "" + _ID;
                if (container != null)
                {
                    newObject.transform.SetParent(container);
                }
                " + className + @" objectComponent;
                if (!newObject.transform.GetComponent<" +className + @">())
                {
                    objectComponent = newObject.AddComponent<" + className + @">();
                }
                else
                {
                    objectComponent = newObject.transform.GetComponent<" + className + @">();
                }
                objectComponent.objectSettings = settings;
                return objectComponent;
            }
        }


";
                File.WriteAllText(scriptPath, scriptContent);
            }
        }
        AssetDatabase.Refresh();
    }

    private void CreateFabricManager()
    {
        var subName = IClassName.Substring(1).ToLower();
        
        string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", FabricManagerName + ".cs");
        if (!File.Exists(scriptPath))
        {
            string scriptContent = @"using UnityEngine;
public class " + FabricManagerName + @"
{
    private " + InterfaceFabricName + @" fabric;

    public void " + "Set" + IClassName.Substring(1)+"Fabric" + @"(" + InterfaceFabricName + @" fabric)
    {
        this.fabric = fabric;
    }

    public void " + "CreateAndInitialize" + IClassName.Substring(1) + "(" + ScriptableName + @" settings, Transform spawnPoint, Transform container)
    {
        " + IClassName +" " + subName + @"= fabric.CreateObject(settings, spawnPoint, container);
        " + subName + @".Init();
    }
}";
            File.WriteAllText(scriptPath, scriptContent);
        }
        AssetDatabase.Refresh();
    }
    
    private void CreateFabricImplementation()
    {
        CreateFabricEnum();
        string fabricName = IClassName.Substring(1) + "Fabric";
        string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", fabricName + ".cs");

        if (!File.Exists(scriptPath))
        {
            string scriptContent = @"

using UnityEngine;
public class " + fabricName + @" : MonoBehaviour
{
    private " + FabricManagerName + @" _fabricManager;
    private Transform container;
   
    [Header(""Settings"")] 
    [SerializeField] private " + ClassTypeEnumName + @" myFabricType;
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float repeatRate = 1f;
    [SerializeField] private " + ScriptableName + @" settings;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool isNeedContainer;
    private void Start()
    {
        CreateContainer();
        InitFactory();
        InvokeRepeating(""Spawn" + IClassName.Substring(1) + @""", firstSpawnDelay, repeatRate);
    }
    private void Spawn" + IClassName.Substring(1) + @"()
    {
        _fabricManager.CreateAndInitialize" + IClassName.Substring(1) + @"(settings, spawnPoint, container);
    }
    private void InitFactory()
    {
        _fabricManager = new " + FabricManagerName + @"();
" +
            GetAllSwitches() + @"
        
    }
    private void CreateContainer()
    {
        if(!isNeedContainer) return;
        var containerObject = new GameObject();
        container = containerObject.transform;
        container.name = myFabricType.ToString() + ""Container"";
        container.SetParent(this.transform);
    }
}
";
            File.WriteAllText(scriptPath, scriptContent);
        }
        AssetDatabase.Refresh();
    }
    
    private void CreateFabricEnum()
    {
        string scriptPath = Path.Combine("Assets", "InternalAssets", "Editor", ClassTypeEnumName + ".cs");

        string ClassNamesEnum = "";
        foreach (var className in classNames)
        {
            ClassNamesEnum += className + ", ";
        }
        
        if (!File.Exists(scriptPath))
        {
            string scriptContent = @"
    public enum " + ClassTypeEnumName + @"
    {" + ClassNamesEnum + @"}";

            File.WriteAllText(scriptPath, scriptContent);
        }
        AssetDatabase.Refresh();
    }

    private string GetAllSwitches()
    {
        string IfabricName = IClassName + "Fabric";

        string myString = @"switch (myFabricType)
        {
";
        for (int i = 0; i < classNames.Count; i++)
        {
            myString += "case " + IClassName.Substring(1) + "Type." + classNames[i] + ":\n";
            myString += "{\n";
            myString += IfabricName +" " + classNames[i].ToLower() + "Fabric" + " = new " + classNames[i]+"Fabric(); \n";
            myString += "_fabricManager." + "Set" + IClassName.Substring(1) + "Fabric" + "(" + classNames[i].ToLower() + "Fabric" +
                        "); \n";
            myString += "    break;\n";
            myString += "}\n";
        }

        myString += "}";
        return myString;
    }

}
