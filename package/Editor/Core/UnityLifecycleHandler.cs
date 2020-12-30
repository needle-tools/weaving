using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;

namespace needle.Weaver
{
	[InitializeOnLoad]
	public static class UnityLifecycleHandler
	{
        // https://forum.unity.com/threads/solved-burst-and-mono-cecil.781148/#post-5201255
        public static bool InMemory = false;
        public static readonly HashSet<string> DefaultAssemblies = new HashSet<string>()
        {
            // "SomeAssemblyToBeFixed.dll"
            "UnityEngine.XRModule.dll"
            // "Assembly-CSharp.dll",
            // "Assembly-CSharp-firstpass.dll"
        };


        static UnityLifecycleHandler()
        {
            // var res = new DefaultAssemblyResolver();
            // res.AddSearchDirectory(Constants.EngineAssembliesPath);
            // if (!Directory.Exists(Constants.ManualPatchingAssembliesPath))
            //     Directory.CreateDirectory(Constants.ManualPatchingAssembliesPath);
            // var dlls = Directory.GetFiles(Constants.WebGLAssembliesPath, "UnityEngine.XRModule.dll", SearchOption.AllDirectories);
            // var assemblies = new HashSet<string>();
            // foreach (var dll in dlls) assemblies.Add(dll);
            // FodyAssemblyProcessor.ProcessAssemblies(assemblies, res);
            
            // Debug.Log("INITIALIZE ON LOAD");
            // // if(!InMemory)
            // //     DoProcessing(false);
            // AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
            // AssemblyReloadEvents.afterAssemblyReload += OnAfterReload;
            // // https://forum.unity.com/threads/solved-burst-and-mono-cecil.781148/#post-5201255
            // CompilationPipeline.compilationFinished += OnAfterCompilationFinished;
            // CompilationPipeline.assemblyCompilationFinished += OnAfterAssemblyCompilationFinished;
        }
        
        // [PostProcessBuild(1)]
        // public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        // {
        //     var exeDir = Path.GetDirectoryName(pathToBuiltProject) ?? "";
        //     var dataFolder = Path.Combine(exeDir, Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
        //     if (!Directory.Exists(dataFolder))
        //         return;
        //     var managed = Path.Combine(dataFolder, "Managed");
        //     if (!Directory.Exists(managed))
        //         return;
        //     //Debug.LogFormat("Fody post-weaving {0}", pathToBuiltProject);
        //     var assemblyResolver = new DefaultAssemblyResolver();
        //     assemblyResolver.AddSearchDirectory(managed);
        //     HashSet<string> assemblyPaths = new HashSet<string>();
        //     foreach(var file in Directory.GetFiles(managed).Where(d => Path.GetExtension(d) == ".dll"))
        //     {
        //         assemblyPaths.Add(file);
        //     }
        //     FodyAssemblyProcessor.ProcessAssemblies(assemblyPaths, assemblyResolver, false);
        // }

        // [PostProcessScene]
        // public static void PostprocessScene()
        // {
        //     if (!BuildPipeline.isBuildingPlayer)
        //         return;
        //
        //     var scene = SceneManager.GetActiveScene();
        //     if (!scene.IsValid() || scene.buildIndex != 0)
        //         return;
        //
        //     DoProcessing(true);
        // }
        
        private static void OnAfterAssemblyCompilationFinished(string arg1, CompilerMessage[] arg2)
        {
            // TODO: if any assembly containing weaver changes we need to re weave everything
            Debug.Log("Assembly compilation finished: " + arg1);
        }

        private static void OnAfterCompilationFinished(object obj)
        {
            Debug.Log("COMPILATION FINISHED " + obj);
            if(!InMemory)
                AssemblyWeaver.CollectAndProcessAssemblies(false, DefaultAssemblies);
        }

        private static void OnBeforeReload()
        {
            // Debug.Log("BEFORE ASSEMBLY RELOAD");
            // if(!InMemory)DoProcessing
            //     DoProcessing(false);
        }

        private static void OnAfterReload()
        {
            Debug.Log("AFTER ASSEMBLY RELOAD");
            if(InMemory)
                AssemblyWeaver.CollectAndProcessAssemblies(true, DefaultAssemblies);
        }
	}
}