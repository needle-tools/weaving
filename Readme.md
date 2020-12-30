# Getting Started


## Weaving

To modify an assembly create a new class deriving from ``BaseModuleWeaver``. Weavers are autodiscovered. Every weaver is called for every assembly that is requested to be modified. Use the ``ModuleDefinition`` field to get information about the current assembly and its types.

## Writing a patch for weaving

Mark your patch with ``NeedlePatch(<Type.To.Patch)>`` attribute. You can either pass in the type to patch directly or the FullName of that type (Namespace + TypeName).
To patch members just implement them in your patch method with the exact same name and signature as defined in the type you want to patch. This works for methods, properties, events, constructors
- Constructor patching: Currently it's necessary to add another ``[NeedlePatch]`` attribute to the patch constructor, otherwise constructors will not be patched


### Example
For patching ``InputDevices.GetDeviceAtXRNode(XRNode)`` your patch would look like this:

```csharp
[NeedlePatch(typeof(InputDevices))]
public class InputDevices_Patch
{
	private static InputDevice GetDeviceAtXRNode(XRNode node)
	{
		return new InputDevice();
	}
}
```

## Utilities

- Todo

----

## Technical Details

### ToDos
- [x] Make patching on base types work - for some reason they are not picked up or discarded
  - Issue was that the base type was defined in another assembly
  - [ ] add warning if patches are not applied to any method
- [ ] Menu item to restore all modified assemblies from backup
- [x] Make ``this`` work -> currently when using ``this`` in a patch the generated IL calls the member on the patch. Instead it should call the member on the patched type
  - when there is a member with the same name and signature (parameters, constraints, type) that ``this`` refers to the call is redirected to that member instead of the member in the patch class
- [x] ~~Merge multiple patches (prefix, postfix etc)~~
  - ~~Issue with harmony is it outputs a dynamic method - there seems to be no way to get the body of a dynamic method (Mono.Reflection throws exception when trying to access bytes for IL)~~
  - ~~Possible solution: patch harmony or fork harmony and provide access to CodeInstruction array (or provide access to internal patch methods)~~
     - see issues listed below at #4 -> best bet is to use custom attribute
  - **Alternative: Use harmony for editor patching and custom attribute for runtime patching**

### Issues

1) WebGL builds throw runtime exception when building with ``C++ Compiler Configuration`` set to ``Debug``
2) ~~Can we use harmony to patch calls to some other assembly that uses types defined inside the assembly we're patching (e.g. using InputDevice in another assembly that we call leads to Linker Error at build time)~~
   - Not sure what resolved it but works now, maybe the Linker issue was not related
3) ~~Local variables are not yet working~~ 
   - the issue seems to be because of method signature mismatch -> I got the postfix from harmony and parsed IL for the postfix instead of the final method, so the variables and parameters did not match
4) Applying harmony patch -> getting IL (CodeInstructions) from harmony -> converting to Cecil Instructions -> patching 
   - results in reference to dll in which harmony patch is (e.g. a editor dll)
     - this has to do with harmony not really embedding the patch IL but instead just calling the patch (e.g. your method marked with Prefix). Look e.g. in Harmony.MethodPatcher.AddPrefixes:645
   - when patching from runtime assembly we get a linker error (cyclic reference? XR.dll referencing Patch.dll referencing XR.dll
   - the method returned by Harmony.Patch is a dynamic method. By default it's not possible to get instructions from that.Also not using cecil apparently: https://stackoverflow.com/questions/2656987/dynamicmethod-in-cecil
   - Possible solution to make it work? - internally harmony seems to use cecil as well, maybe we can intercept that process and get the cecil instructions when harmony is done

### Gotchas

- Rider might pickup patched assemblies and show wrong suggestions / decompiler shows patched code instead of original. In that case try ``File/Invalidate caches and restart``
- ~~If we implement everything in a harmony patch the patch signature MUST match the method we patch exactly, otherwise we get a IL error at build time~~ only true if we apply the patch method

