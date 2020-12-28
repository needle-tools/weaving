
### TODO
- ~~Can we use harmony to patch calls to some other assembly that uses types defined inside the assembly we're patching (e.g. using InputDevice in another assembly that we call leads to Linker Error at build time)~~
- Merge multiple patches (prefix, postfix etc)
- Support for prefix
- If we implement everything in a harmony patch the patch signature MUST match the method we patch exactly, otherwise we get a IL error at build time
- Local variables are not yet working


### Known issues

- WebGL builds throw runtime exception when building with ``C++ Compiler Configuration`` set to ``Debug``