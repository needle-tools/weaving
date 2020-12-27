using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;


namespace needle.Weaver
{
    public static class FodyAssemblyProcessor
    {

        public static void CollectAndProcessAssemblies(bool inMemory, HashSet<string> allowedAssemblies)
        {
            Debug.Log("Fody processor running -------------------------------------------------------");

            // This will hold the paths to all the assemblies that will be processed
            var assemblyPaths = new HashSet<string>();
            // This will hold the search directories for the resolver
            var assemblySearchDirectories = new HashSet<string>();
            // Create resolver
            var assemblyResolver = new DefaultAssemblyResolver();
            
            try
            {
                var assetPath = Path.GetFullPath(Application.dataPath);
                // TODO: not 2018 compatible
                var packagePath = Path.GetFullPath(Application.dataPath + "/../Packages");

                // Add all assemblies in the project to be processed, and add their directory to
                // the resolver search directories.
                foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.IsDynamic)
                        continue; 


                    if (string.IsNullOrEmpty(assembly.Location))
                    {
                        Debug.LogWarning("Assembly has no path: " + assembly.FullName + " -> skip");
                        continue;
                    }
                
                    try
                    {
                        var isAsset = Path.GetFullPath(assembly.Location).StartsWith(assetPath);
                        var isPackageAsset = Path.GetFullPath(assembly.Location).StartsWith(packagePath);
                        var isInProject = assembly.Location.Replace('\\', '/').StartsWith(Application.dataPath.Substring(0, Application.dataPath.Length - 7));
                        // Only process assemblies which are in the project
                        // if(isInProject)
                        {
                            // Debug.Log($"Add assembly {assembly.FullName} at " + assembly.Location);
                            assemblyPaths.Add(assembly.Location);
                            // if(isAsset || isPackageAsset)
                        }
                    
                        // But always add the assembly folder to the search directories
                        var dir = Path.GetDirectoryName(assembly.Location);
                        if(!assemblySearchDirectories.Contains(dir))
                        {
                            // Debug.Log("Add assembly search dir " + dir);
                            assemblySearchDirectories.Add(dir);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{assembly.FullName} - {e}");
                    }
                }
                // Add all directories found in the project folder
                foreach(var searchDirectory in assemblySearchDirectories )
                {
                    assemblyResolver.AddSearchDirectory( searchDirectory );
                }
                // Add path to the Unity managed dlls
                var managedDir = Path.GetDirectoryName(EditorApplication.applicationPath) + "/Data/Managed";
                assemblyResolver.AddSearchDirectory(managedDir);

                Debug.Log("Collected " + assemblyPaths.Count + " assembly paths now\n" + string.Join("\n", assemblyPaths));
                Debug.Log("Collected " + assemblySearchDirectories.Count + " assembly search dirs now\n" + string.Join("\n", assemblySearchDirectories));

                var fodyConfig = GetFodySettings();
                if (fodyConfig != null)
                {
                    var xva = fodyConfig.Root?.Attribute("ProcessAssemblies");
                    if (xva != null)
                    {
                        var xass = new HashSet<string>(xva.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                        assemblyPaths.RemoveWhere(a => !xass.Contains(Path.GetFileName(a)));
                    }
                    else
                    {
                        assemblyPaths.RemoveWhere(a => !allowedAssemblies.Contains(Path.GetFileName(a)));
                    }
                }
                else
                {
                    assemblyPaths.RemoveWhere(a => !allowedAssemblies.Contains(Path.GetFileName(a)));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
            try
            {
                // Lock assemblies while they may be altered
                EditorApplication.LockReloadAssemblies();
                ProcessAssemblies(assemblyPaths, assemblyResolver, inMemory);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }

            Debug.Log("Fody processor finished");
        }

        public static void ProcessAssemblies(HashSet<string> assemblyPaths, IAssemblyResolver assemblyResolver, bool inMemory)
        {
            // Create reader parameters with resolver
            var readerParameters = new ReaderParameters();
            readerParameters.AssemblyResolver = assemblyResolver; 

            // Create writer parameters
            var writerParameters = new WriterParameters(); 
            var fodyConfig = GetFodySettings();
            var weavers = InitializeWeavers(fodyConfig, assemblyResolver);
        
            Debug.Log("assemblyPaths after filtering: " + assemblyPaths.Count);

            // Process any assemblies which need it
            foreach (var assemblyPath in assemblyPaths)
            {
                Debug.Log("Path: " + assemblyPath);
            
                // mdbs have the naming convention myDll.dll.mdb whereas pdbs have myDll.pdb
                var mdbPath = assemblyPath + ".mdb";
                var pdbPath = assemblyPath.Substring(0, assemblyPath.Length - 3) + "pdb";

                // Figure out if there's an pdb/mdb to go with it
                if (File.Exists(pdbPath))
                {
                    Debug.Log("Found pdb path: " + pdbPath);
                    readerParameters.ReadSymbols = true;
                    readerParameters.SymbolReaderProvider = new PdbReaderProvider();
                    writerParameters.WriteSymbols = true;
                    writerParameters.SymbolWriterProvider = new PdbWriterProvider(); // pdb written out as mdb, as mono can't work with pdbs
                }
                // else if (File.Exists(mdbPath))
                // {
                //     Debug.Log("Found mdb path: " + mdbPath);
                //     readerParameters.ReadSymbols = true;
                //     readerParameters.SymbolReaderProvider = new Mono.Cecil.Mdb.MdbReaderProvider();
                //     writerParameters.WriteSymbols = true;
                //     writerParameters.SymbolWriterProvider = new Mono.Cecil.Mdb.MdbWriterProvider();
                // }
                else
                {
                    Debug.Log("Found no debug symbols");
                    readerParameters.ReadSymbols = false;
                    readerParameters.SymbolReaderProvider = null;
                    writerParameters.WriteSymbols = false;
                    writerParameters.SymbolWriterProvider = null;
                }

                Stream stream = null;
                ModuleDefinition module = null;
                try
                {

                    stream = LoadAssemblyForModule(assemblyPath, inMemory);
                    module = ModuleDefinition.ReadModule(stream, readerParameters);
                    var modified = module.IsModified();
                    stream.Dispose();
                    module.Dispose();
                    DllBackupHelper.GetFromBackupIfAvailable(assemblyPath, modified);

                    // Read assembly
                    stream = LoadAssemblyForModule(assemblyPath, inMemory);
                    Debug.Log("Read module from stream (in memory? " + inMemory + ")");
                    module = ModuleDefinition.ReadModule(stream, readerParameters);

                    Debug.Log("Prepare weavers");
                    PrepareWeaversForModule(weavers, module);

                    // Process it if it hasn't already
                    Debug.Log("Processing " + Path.GetFileName(assemblyPath));
                    if (ProcessAssembly(assemblyPath, module, weavers))
                    {
                        var path = assemblyPath;
                        Debug.Log("Writing processed assembly to " + path);
                        module.Write(stream, writerParameters);
                        Debug.Log("Done writing to " + path);
                    }
                }
                catch (IOException e)
                {
                    Debug.LogError(e);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
                finally
                {
                    stream?.Dispose();
                    module?.Dispose();
                }
            }
        }

        private static Stream LoadAssemblyForModule(string assemblyPath, bool inMemory)
        {
            if (!inMemory)
            {
                Debug.Log("Read Assembly from file");
                return new FileStream(assemblyPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
        
            Debug.Log("Read assembly from memory");
            // TODO: this does not work yet (BadImageException from Cecil ImageReader.ReadImage:63
            var rawAssembly = File.ReadAllBytes(assemblyPath);
            // to make it expandable https://stackoverflow.com/a/52052656
            var ms = new MemoryStream(0);
            ms.Write(rawAssembly, 0, rawAssembly.Length);
            return ms;
            // return new MemoryStream(rawAssembly, true);
        }

        private static bool ProcessAssembly(string assemblyPath, ModuleDefinition module, IEnumerable<WeaverEntry> weavers)
        {
            if (module.IsModified()) 
            {
                Debug.LogWarning($"Skipping {assemblyPath} as it is already processed");
                return false;
            }

            Debug.Log("Process assembly at " + assemblyPath);

            foreach (var weaver in weavers)
            {
                if (weaver.WeaverInstance == null) continue;

                try
                {
                    Debug.Log("<b>Execute " + weaver + "</b>");
                    weaver.Run("Execute");
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Failed to run weaver {0} on {1}: {2}", weaver.PrettyName(), assemblyPath, e);
                }
            }

            AddProcessedFlag(module);
            return true;
        }

        private static void PrepareWeaversForModule(List<WeaverEntry> weavers, ModuleDefinition module)
        {
            foreach(var weaver in weavers)
            {
                weaver.SetProperty("ModuleDefinition", module);
            }
        }

        static void AddProcessedFlag(ModuleDefinition module)
        {
            module.Types.Add(new TypeDefinition(null, "ProcessedByFody", TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Interface));
        }

        static XDocument GetFodySettings()
        {
            var configAsset = AssetDatabase.FindAssets("FodyWeavers t:TextAsset").FirstOrDefault();
            if (!string.IsNullOrEmpty(configAsset))
            {
                var configFile = AssetDatabase.GUIDToAssetPath(configAsset);

                Debug.Log("weavers file located at " + configFile);

                return GetDocument(configFile);
            }
            else
                Debug.LogFormat("no file found named FodyWeavers.xml");

            return null;
        }

        static List<WeaverEntry> InitializeWeavers(XDocument fodyConfig, IAssemblyResolver resolver)
        {
            var weavers = new List<WeaverEntry>();

            if (fodyConfig != null)
            {
                foreach (var element in fodyConfig.Root.Elements())
                {
                    var assemblyName = element.Name.LocalName + ".Fody";
                    var existing = weavers.FirstOrDefault(x => string.Equals(x.AssemblyName, assemblyName, StringComparison.OrdinalIgnoreCase));
                    var index = weavers.Count;
                    if (existing != null)
                    {
                        index = weavers.IndexOf(existing);
                        weavers.Remove(existing);
                    }
                    var weaverEntry = new WeaverEntry
                    {
                        Element = element.ToString(SaveOptions.None),
                        AssemblyName = assemblyName,
                        TypeName = "ModuleWeaver"
                    };
                    Debug.LogFormat("Added weaver {0}", weaverEntry.AssemblyName);
                    weavers.Insert(index, weaverEntry);
                }

                foreach (var weaverConfig in weavers.ToArray())
                {
                    if (weaverConfig.WeaverType != null) continue;

                    //determine the assembly path.
                    var weavePath = AssetDatabase.FindAssets(weaverConfig.AssemblyName).Select(w => AssetDatabase.GUIDToAssetPath(w)).FirstOrDefault(p => p.EndsWith(".dll"));
                    if (string.IsNullOrEmpty(weavePath))
                    {
                        Debug.LogWarningFormat("Could not find weaver named {0}", weaverConfig.AssemblyName);
                        weavers.Remove(weaverConfig);
                        continue;
                    }
                    weaverConfig.AssemblyPath = weavePath;

                    Debug.Log($"Load Weaver at '{weaverConfig.AssemblyPath}'.");
                    var assembly = LoadAssembly(weaverConfig.AssemblyPath);
                    Debug.Log("Loaded Weaver " + assembly.Location);

                    var weaverType = GetType(assembly, weaverConfig.TypeName);
                    if (weaverType == null)
                    {
                        weavers.Remove(weaverConfig);
                        continue;
                    }

                    weaverConfig.Activate(weaverType);
                    SetProperties(weaverConfig, resolver);
                }
            }

            //add a project weaver

            var projectWeavers = UnityEditor.TypeCache.GetTypesDerivedFrom<BaseModuleWeaver>();
            foreach(var weaver in projectWeavers)
            {
                Debug.LogFormat("Added project weaver {0}", weaver);
                var entry = new WeaverEntry();
                entry.Activate(weaver);
                SetProperties(entry,resolver);
                weavers.Add(entry);
            }

            Debug.Log($"Fody processor running for {weavers.Count} weavers \n{string.Join("\n", weavers.Select(w => w.PrettyName()))}");
            return weavers;
        }

        private static Type GetType(Assembly assembly, string typeName)
        {
            return assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
        }

        static XDocument GetDocument(string configFilePath)
        {
            try
            {
                return XDocument.Load(configFilePath);
            }
            catch (XmlException exception)
            {
                throw new Exception($"Could not read FodyWeavers.xml because it has invalid xml. Message: '{exception.Message}'.");
            }
        }

        private static void SetProperties(WeaverEntry weaverEntry, IAssemblyResolver resolver)
        {
            if (weaverEntry.WeaverInstance == null) return;

            if (weaverEntry.Element != null)
            {
                var weaverElement = XElement.Parse(weaverEntry.Element);
                weaverEntry.TrySetProperty("Config", weaverElement);
            }

            weaverEntry.TrySetProperty("AssemblyResolver", resolver);
            weaverEntry.TryAddEvent("LogDebug", new Action<string>((str) => Debug.Log(str)));
            weaverEntry.TryAddEvent("LogInfo", new Action<string>((str) => Debug.Log(str)));
            weaverEntry.TryAddEvent("LogWarning", new Action<string>((str) => Debug.LogWarning(str)));
        }

        static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        static Assembly LoadAssembly(string assemblyPath)
        {
            Assembly assembly;
            if (assemblies.TryGetValue(assemblyPath, out assembly))
            {
                Debug.Log($"Loading '{assemblyPath}' from cache.");
                return assembly;
            }
            Debug.Log($"Loading '{assemblyPath}' from disk.");
            return assemblies[assemblyPath] = LoadFromFile(assemblyPath);
        }

        static Assembly LoadFromFile(string assemblyPath)
        {
            Debug.Log("Load Assembly: " + assemblyPath);
            var pdbPath = Path.ChangeExtension(assemblyPath, "pdb");
            var rawAssembly = File.ReadAllBytes(assemblyPath);
            if (File.Exists(pdbPath))
            {
                return Assembly.Load(rawAssembly, File.ReadAllBytes(pdbPath));
            }
            var mdbPath = Path.ChangeExtension(assemblyPath, "mdb");
            if (File.Exists(mdbPath))
            {
                return Assembly.Load(rawAssembly, File.ReadAllBytes(mdbPath));
            }
            return Assembly.Load(rawAssembly);
        }

        class WeaverEntry
        {
            public string AssemblyName;
            public string AssemblyPath;
            public string Element;
            public string TypeName;
            public Type WeaverType;

            public object WeaverInstance;

            public string PrettyName()
            {
                if (WeaverType == null)
                    return "invalid weaver: " + AssemblyName + "::" + TypeName;
                return WeaverType.Assembly.GetName().Name + "::" + WeaverType.FullName;
            }

            internal void SetProperty(string property, object value)
            {
                WeaverType.GetProperty(property).SetValue(WeaverInstance, value, null);
            }

            internal void TrySetProperty(string property, object value)
            {
                var prop = WeaverType.GetProperty(property);
                if (prop == null) return;
                prop.SetValue(WeaverInstance, value, null);
            }

            internal void TryAddEvent(string evt, Delegate value)
            {
                var ev = WeaverType.GetEvent(evt);
                if (ev == null) return;
                ev.AddEventHandler(WeaverInstance, value);
            }

            internal void Activate(Type weaverType)
            {
                WeaverType = weaverType;
                WeaverInstance = Activator.CreateInstance(weaverType);
            }

            internal void Run(string methodName)
            {
                var method = WeaverType.GetMethod(methodName);
                if (method == null)
                    throw new MethodAccessException("Could not find a public method named " + methodName + " in the type " + WeaverType);
                method.Invoke(WeaverInstance, null);
            }
        }
    }
}