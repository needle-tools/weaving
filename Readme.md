
### ToDos
- Merge multiple patches (prefix, postfix etc)
  - Issue with harmony is it outputs a dynamic method - there seems to be no way to get the body of a dynamic method (Mono.Reflection throws exception when trying to access bytes for IL)
  - Possible solution: patch harmony or fork harmony and provide access to CodeInstruction array (or provide access to internal patch methods)
  - Alternative: Use harmony for editor patching and custom attribute for runtime patching
- If we implement everything in a harmony patch the patch signature MUST match the method we patch exactly, otherwise we get a IL error at build time


### Known issues

- WebGL builds throw runtime exception when building with ``C++ Compiler Configuration`` set to ``Debug``
- ~~Can we use harmony to patch calls to some other assembly that uses types defined inside the assembly we're patching (e.g. using InputDevice in another assembly that we call leads to Linker Error at build time)~~
  - Not sure what resolved it but works now, maybe the Linker issue was not related
- ~~Local variables are not yet working~~ 
  - the issue seems to be because of method signature mismatch -> I got the postfix from harmony and parsed IL for the postfix instead of the final method, so the variables and parameters did not match


## Overview

- Todo

### Weavers

When wanting to modify an assembly create a new class deriving from ``BaseModuleWeaver``. Weavers are autodiscovered. Every weaver is called for every assembly that is requested to be modified. Use the ``ModuleDefinition`` field to get information about the current assembly and its types.

#### Editor Patches for weaving

- Todo

#### Utility

- Todo