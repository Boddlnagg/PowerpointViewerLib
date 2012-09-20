/******************************************************************************
* PptViewLib - PowerPoint Viewer 2003/2007 Controller                         *
* Originally part of OpenLP - Open Source Lyrics Projection                   *
* --------------------------------------------------------------------------- *
* Copyright (c) 2012 Kai Patrick Reisert                                      *
* Original copyright (c) 2008-2011 Raoul Snyman                              *
* Portions copyright (c) 2008-2011 Tim Bentley, Jonathan Corwin, Michael      *
* Gorven, Scott Guerrieri, Matthias Hub, Meinert Jordan, Armin Köhler,        *
* Andreas Preikschat, Mattias Põldaru, Christian Richter, Philip Ridout,      *
* Maikel Stuivenberg, Martin Thompson, Jon Tibble, Frode Woldsund             *
* --------------------------------------------------------------------------- *
* This program is free software; you can redistribute it and/or modify it     *
* under the terms of the GNU General Public License as published by the Free  *
* Software Foundation; version 2 of the License.                              *
*                                                                             *
* This program is distributed in the hope that it will be useful, but WITHOUT *
* ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or       *
* FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for    *
* more details.                                                               *
*                                                                             *
* You should have received a copy of the GNU General Public License along     *
* with this program; if not, write to the Free Software Foundation, Inc., 59  *
* Temple Place, Suite 330, Boston, MA 02111-1307 USA                          *
******************************************************************************/

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <io.h>
#include <direct.h>
#include <time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include "pptviewlib.h"

// Because of the callbacks used by SetWindowsHookEx, the memory used needs to
// be sharable across processes (the callbacks are done from a different
// process) Therefore use data_seg with RWS memory.
//
// See http://msdn.microsoft.com/en-us/library/aa366551(VS.85).aspx for
// alternative method of holding memory, removing fixed limits which would allow
// dynamic number of items, rather than a fixed number. Use a Local\ mapping,
// since global has UAC issues in Vista.

#pragma data_seg(".PPTVIEWLIB")
PPTVIEW pptView[MAX_PPTS] = {NULL};
HHOOK globalHook = NULL;
BOOL debug = FALSE;
#pragma data_seg()
#pragma comment(linker, "/SECTION:.PPTVIEWLIB,RWS")

HINSTANCE hInstance = NULL;

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ulReasonForCall,
	LPVOID lpReserved)
{
	hInstance = (HINSTANCE)hModule;
	switch(ulReasonForCall)
	{
		case DLL_PROCESS_ATTACH:
			DEBUG("PROCESS_ATTACH\n");
			break;
		case DLL_THREAD_ATTACH:
			//DEBUG("THREAD_ATTACH\n");
			break;
		case DLL_THREAD_DETACH:
			//DEBUG("THREAD_DETACH\n");
			break;
		case DLL_PROCESS_DETACH:
			// Clean up... hopefully there is only the one process attached?
			// We'll find out soon enough during tests!
			DEBUG("PROCESS_DETACH\n");
			
			break;
	}
	return TRUE;
}

DllExport void SetDebug(BOOL onOff)
{
	printf("SetDebug\n");
	debug = onOff;
	DEBUG("enabled\n");
}

DllExport VOID Shutdown()
{
	for (int i = 0; i < MAX_PPTS; i++)
	{
		if (pptView[i].state != PPT_CLOSED) // Why is this called when one process exits?
			ClosePPT(i);
	}
}

// Open the PointPoint, count the slides and take a snapshot of each slide
// for use in previews
// previewpath is a prefix for the location to put preview images of each slide.
// "<n>.bmp" will be appended to complete the path. E.g. "c:\temp\slide" would
// create "c:\temp\slide1.bmp" slide2.bmp, slide3.bmp etc.
// It will also create a *info.txt containing information about the ppt
DllExport int OpenPPT(char *command, func_type callbackFunc, HWND hParentWnd, int x, int y, int width, int height)
{
	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	//char cmdLine[MAX_PATH * 2];
	int id;

	DEBUG("OpenPPT start: %s\n", command);
	DEBUG("OpenPPT start: %u; %i, %i, %i, %i\n", hParentWnd, x, y, width, height);
	/*if (GetPPTViewerPath(cmdLine, sizeof(cmdLine)) == FALSE)
	{
		DEBUG("OpenPPT: GetPPTViewerPath failed\n");
		return -1;
	}*/
	id = -1;
	for (int i = 0; i < MAX_PPTS; i++)
	{
		if (pptView[i].state == PPT_CLOSED)
		{
			id = i;
			break;
		}
	}
	if (id < 0)
	{
		DEBUG("OpenPPT: Too many PPTs\n");
		return -1;
	}
	memset(&pptView[id], 0, sizeof(PPTVIEW));
	pptView[id].callbackFunc = callbackFunc;
	pptView[id].locked = false;
	pptView[id].nextMsg = 0;
	pptView[id].state = PPT_CLOSED;
	pptView[id].slideCount = 0;
	pptView[id].currentSlide = 0;
	pptView[id].hParentWnd = hParentWnd;
	pptView[id].hWnd = NULL;
	pptView[id].hWnd2 = NULL;
	pptView[id].firstSlideNo = 0;
	if (hParentWnd != NULL && x == 0 && y == 0 && width == 0 && height == 0)
	{
		LPRECT windowRect = NULL;
		GetWindowRect(hParentWnd, windowRect);
		pptView[id].rect.top = 0;
		pptView[id].rect.left = 0;
		pptView[id].rect.bottom = windowRect->bottom - windowRect->top;
		pptView[id].rect.right = windowRect->right - windowRect->left;
	}
	else
	{
		pptView[id].rect.top = y;
		pptView[id].rect.left = x;
		pptView[id].rect.bottom = y + height;
		pptView[id].rect.right = x + width;

		DEBUG("width %d\n", pptView[id].rect.bottom - pptView[id].rect.top);
		DEBUG("height %d\n", pptView[id].rect.right - pptView[id].rect.left);
	}
	memset(&si, 0, sizeof(si));
	memset(&pi, 0, sizeof(pi));

	/*
	 * I'd really like to just hook on the new threadid. However this always
	 * gives error 87. Perhaps I'm hooking to soon? No idea... however can't
	 * wait since I need to ensure I pick up the WM_CREATE as this is the only
	 * time the window can be resized in such away the content scales correctly
	 *
	 * hook = SetWindowsHookEx(WH_CBT,CbtProc,hInstance,pi.dwThreadId);
	 */
	if (globalHook != NULL)
	{
		UnhookWindowsHookEx(globalHook);
	}
	globalHook = SetWindowsHookEx(WH_CBT, CbtProc, hInstance, NULL);
	if (globalHook == 0)
	{
		DEBUG("OpenPPT: SetWindowsHookEx failed\n");
		ClosePPT(id, true);
		return -1;
	}
	pptView[id].state = PPT_STARTED;
	Sleep(10);
	if (!CreateProcess(NULL, command, NULL, NULL, FALSE, 0, 0, NULL, &si, &pi))
	{
		DEBUG("OpenPPT: CreateProcess failed: %s\n", command);
		ClosePPT(id, true);
		return -1;
	}
	pptView[id].dwProcessId = pi.dwProcessId;
	pptView[id].dwThreadId = pi.dwThreadId;
	pptView[id].hThread = pi.hThread;
	pptView[id].hProcess = pi.hProcess;

	pptView[id].listenerThread = CreateThread(NULL, 0, ProcessCallbackMessages, &id, 0, NULL);

	DEBUG("Listener thread for %d: %d\n", id, pptView[id].listenerThread);

	while (pptView[id].state == PPT_STARTED)
	{
		Sleep(10);
		DWORD exitCode;
		if(!GetExitCodeProcess(pi.hProcess, &exitCode) || exitCode == 0)
		{
			return -1;
		}
	}

	pptView[id].steps = 0;
	int steps = 0;
	while (pptView[id].state == PPT_OPENED)
	{
		if (steps <= pptView[id].steps)
		{
			Sleep(20);
			DEBUG("OpenPPT: Step %d/%d\n", steps, pptView[id].steps);
			steps++;
			InternalNextStep(id);
		}
		Sleep(10);
	}

	if (pptView[id].state == PPT_CLOSING
		|| pptView[id].slideCount <= 0)
	{
		ClosePPT(id);
		id=-1;
	}

	if (id >= 0)
	{
		if (pptView[id].msgHook != NULL)
		{
			UnhookWindowsHookEx(pptView[id].msgHook);
		}
		pptView[id].msgHook = NULL;
	}

	DEBUG("OpenPPT: Exit: id=%i\n", id);
	return id;
}

// Unhook the Windows hook
void Unhook(int id)
{
	DEBUG("Unhook: start %d\n", id);
	if (pptView[id].hook != NULL)
	{
		UnhookWindowsHookEx(pptView[id].hook);
	}
	if (pptView[id].msgHook != NULL)
	{
		UnhookWindowsHookEx(pptView[id].msgHook);
	}
	pptView[id].hook = NULL;
	pptView[id].msgHook = NULL;
	DEBUG("Unhook: exit ok\n");
}

void ClosePPT(int id, bool force)
{
	DEBUG("ClosePPT: start%d\n", id);
	if (!force)
	{
		SendCallback(id, 6, 0);
		pptView[id].state = PPT_CLOSED;
	}
	Unhook(id);
	if (force || pptView[id].hWnd == 0)
	{
		TerminateThread(pptView[id].hThread, 0);
	}
	else
	{
		PostMessage(pptView[id].hWnd, WM_CLOSE, 0, 0);
	}

	Sleep(100);

	TerminateThread(pptView[id].listenerThread, 0);
	CloseHandle(pptView[id].listenerThread);
	if (force && pptView[id].hProcess != NULL)
	{
		TerminateProcess(pptView[id].hProcess, 0);
	}
	CloseHandle(pptView[id].hThread);
	CloseHandle(pptView[id].hProcess);
	memset(&pptView[id], 0, sizeof(PPTVIEW));
	DEBUG("ClosePPT: exit ok\n");
	return;
}

// Close the PowerPoint viewer, release resources
DllExport void ClosePPT(int id)
{
	ClosePPT(id, false);
}

// Take a step forwards through the show
void InternalNextStep(int id)
{
	DEBUG("NextStep:%d (%d)\n", id, pptView[id].currentSlide);
	if (pptView[id].currentSlide > pptView[id].slideCount) return;
	PostMessage(pptView[id].hWnd2, WM_MOUSEWHEEL, MAKEWPARAM(0, -WHEEL_DELTA),
		0);
}

DWORD WINAPI ProcessCallbackMessages( LPVOID lpParam ) 
{
	int id = *((int*)lpParam);

	while (pptView[id].state != PPT_CLOSED)
	{
		if (pptView[id].locked)
		{
			DEBUG("waiting for lock\n");
		}
		else if(pptView[id].nextMsg > 0)
		{
			// TODO: some kind of locking
			pptView[id].locked = true;
			DEBUG("Sending callback %d\n", pptView[id].nextMsg);
			pptView[id].callbackFunc(pptView[id].nextMsg, pptView[id].nextMsgParam);
			DEBUG("Sent callback %d\n", pptView[id].nextMsg);
			pptView[id].nextMsg = 0;
			pptView[id].locked = false;
		}
		Sleep(10);
	}

	Sleep(50);

	if (pptView[id].locked)
	{
		DEBUG("waiting for lock\n");
	}
	else if(pptView[id].nextMsg > 0)
	{
		pptView[id].locked = true;
		DEBUG("Sending callback to %d (after) %d\n", id, pptView[id].nextMsg);
		pptView[id].callbackFunc(pptView[id].nextMsg, pptView[id].nextMsgParam);
		DEBUG("Sent callback to %d (after) %d\n", id, pptView[id].nextMsg);
		pptView[id].nextMsg = 0;
		pptView[id].locked = false;
	}	
	
	return 0; 
}

// This hook is started with the PPTVIEW.EXE process and waits for the
// WM_CREATEWND message. At this point (and only this point) can the
// window be resized to the correct size.
// Release the hook as soon as we're complete to free up resources
LRESULT CALLBACK CbtProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	HHOOK hook = globalHook;
	if (nCode == HCBT_CREATEWND)
	{
		char csClassName[16];
		HWND hCurrWnd = (HWND)wParam;
		DWORD retProcId = NULL;
		GetClassName(hCurrWnd, csClassName, sizeof(csClassName));
		if ((strcmp(csClassName, "paneClassDC") == 0)
		  ||(strcmp(csClassName, "screenClass") == 0))
		{
			int id = -1;
			DWORD windowThread = GetWindowThreadProcessId(hCurrWnd, NULL);
			for (int i=0; i < MAX_PPTS; i++)
			{
				if (pptView[i].dwThreadId == windowThread)
				{
					id = i;
					break;
				}
			}
			if (id >= 0)
			{
				if (strcmp(csClassName, "paneClassDC") == 0)
				{
					pptView[id].hWnd2 = hCurrWnd;
				}
				else
				{
					pptView[id].hWnd = hCurrWnd;
					
					CBT_CREATEWND* cw = (CBT_CREATEWND*)lParam;
					if (pptView[id].hParentWnd != NULL)
					{
						cw->lpcs->hwndParent = pptView[id].hParentWnd;
					}
					cw->lpcs->cy = pptView[id].rect.bottom
						- pptView[id].rect.top;
					cw->lpcs->cx = pptView[id].rect.right
						- pptView[id].rect.left;
					cw->lpcs->y = -32000;
					cw->lpcs->x = -32000;
				}
				if ((pptView[id].hWnd != NULL) && (pptView[id].hWnd2 != NULL))
				{
					UnhookWindowsHookEx(globalHook);
					globalHook = NULL;
					pptView[id].hook = SetWindowsHookEx(WH_CALLWNDPROC,
						CwpProc, hInstance, pptView[id].dwThreadId);
					pptView[id].msgHook = SetWindowsHookEx(WH_GETMESSAGE,
						GetMsgProc, hInstance, pptView[id].dwThreadId);
					Sleep(10);
					pptView[id].state = PPT_OPENED;
					SendCallback(id, 1, (int)pptView[id].hWnd);
					SendCallback(id, 2, (int)pptView[id].hWnd2);
				}
			}
		}
	}
	return CallNextHookEx(hook, nCode, wParam, lParam);
}

// This hook exists whilst the slideshow is loading but only listens on the
// slideshows thread. It listens out for mousewheel events
LRESULT CALLBACK GetMsgProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	HHOOK hook = NULL;
	MSG *pMSG = (MSG *)lParam;
	DWORD windowThread = GetWindowThreadProcessId(pMSG->hwnd, NULL);
	int id = -1;
	for (int i = 0; i < MAX_PPTS; i++)
	{
		if (pptView[i].dwThreadId == windowThread)
		{
			id = i;
			hook = pptView[id].msgHook;
			break;
		}
	}
	if (id >= 0 && nCode == HC_ACTION && wParam == PM_REMOVE
		&& pMSG->message == WM_MOUSEWHEEL)
	{
		SendCallback(id, 3, 0);
		if (pptView[id].state != PPT_LOADED)
		{
			pptView[id].steps++;
		}
	}
	return CallNextHookEx(hook, nCode, wParam, lParam);
}
// This hook exists whilst the slideshow is running but only listens on the
// slideshows thread. It listens out for slide changes, message WM_USER+22.
LRESULT CALLBACK CwpProc(int nCode, WPARAM wParam, LPARAM lParam){
	CWPSTRUCT *cwp;
	cwp = (CWPSTRUCT *)lParam;
	HHOOK hook = NULL;

	DWORD windowThread = GetWindowThreadProcessId(cwp->hwnd, NULL);
	int id = -1;
	for (int i = 0; i < MAX_PPTS; i++)
	{
		if (pptView[i].dwThreadId == windowThread)
		{
			id = i;
			hook = pptView[id].hook;
			break;
		}
	}
	if ((id >= 0) && (nCode == HC_ACTION))
	{
		if (cwp->message == WM_USER + 22)
		{
			SendCallback(id, 4, cwp->wParam);

			if (pptView[id].state != PPT_LOADED)
			{
				if (((cwp->wParam == 0)
					|| (pptView[id].firstSlideNo == cwp->wParam))
					&& (pptView[id].currentSlide > 0))
				{
					pptView[id].state = PPT_LOADED;
					pptView[id].currentSlide = pptView[id].slideCount + 1;
				}
				else
				{
					if (cwp->wParam > 0)
					{
						pptView[id].currentSlide = pptView[id].currentSlide + 1;
						if (pptView[id].currentSlide == 1)
							pptView[id].firstSlideNo = cwp->wParam;
						pptView[id].slideCount = pptView[id].currentSlide;
					}
				}
			}
		}
		if ((pptView[id].state != PPT_CLOSED)
			&&(cwp->message == WM_CLOSE || cwp->message == WM_QUIT))
		{
			pptView[id].state = PPT_CLOSING;
			SendCallback(id, 5, 0);
		}
	}
	return CallNextHookEx(hook, nCode, wParam, lParam);
}

VOID SendCallback(int id, int msg, int param)
{
	while(pptView[id].locked || pptView[id].nextMsg != 0)
	{
		Sleep(15);
	}

	pptView[id].locked = true;
	pptView[id].nextMsg = msg;
	pptView[id].nextMsgParam = param;
	pptView[id].locked = false;
}
