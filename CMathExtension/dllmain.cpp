#include "pch.h"
#include "framework.h"

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        // Initialization code when the DLL is loaded into a process
        break;
    case DLL_PROCESS_DETACH:
        // Cleanup code when the DLL is unloaded from a process
        break;
    }

    return TRUE;
}
