#include "pch.h"
#include "Core.h"
#include "iostream"
#include "MinHook.h"
#include "Sims3/ScriptHost.h"

typedef int(__thiscall *INITIALIZESCRIPTHOST)(void* me);

INITIALIZESCRIPTHOST fpInitializeScriptHost = NULL;

// thiscall hooking hack
int __fastcall DetourInitializeScriptHost(void* me, void* _) {
	printf("Initializing ScriptHost");
	int result = fpInitializeScriptHost(me);
	ScriptHost::GetInstance()->CreateMonoClass("MonoPatcherLib", "DLLEntryPoint");
	return result;
}

Core* Core::_instance = nullptr;

Core* Core::GetInstance() {
	return _instance;
}

bool Core::Create() {
	_instance = new Core();
	return _instance->Initialize();
}

bool Core::Initialize() {
	printf("Mono Patcher CPP Core initializing\n");
	// Initialize MinHook.
	if (MH_Initialize() != MH_OK)
		return false;

	if (MH_CreateHook((void*)0x00411be0, &DetourInitializeScriptHost,
		reinterpret_cast<LPVOID*>(&fpInitializeScriptHost)) != MH_OK)
	{
		return false;
	}

	if (MH_EnableHook((void*)0x00411be0) != MH_OK)
	{
		return false;
	}
}