using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System;

/// <summary>
/// Ensures that when running from Visual Studio on Windows, the console encoding is set to UTF-8 
/// to support full Unicode and emoji output.
/// </summary>
class ConsoleEncodingInitializer
{
#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    [ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    public static void Init()
    {

#if DEBUG
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
#endif
    }
}
