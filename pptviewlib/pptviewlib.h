/******************************************************************************
* PptViewLib - PowerPoint Viewer 2003/2007 Controller                         *
* Originally part of OpenLP - Open Source Lyrics Projection                   *
* --------------------------------------------------------------------------- *
* Copyright (c) 2012-2013 Kai Patrick Reisert                                      *
* Original copyright (c) 2008-2011 Raoul Snyman                               *
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

#define DllExport extern "C"  __declspec( dllexport )

#define DEBUG(...)  if (debug) printf(__VA_ARGS__)

typedef int (__cdecl *func_type)(int msg, int param);

enum PPTVIEWSTATE {PPT_CLOSED, PPT_STARTED, PPT_OPENED, PPT_LOADED,
	PPT_CLOSING};

DllExport int OpenPPT(char *command, func_type callbackFunc, HWND hParentWnd, int x, int y, int width, int height);
DllExport void ClosePPT(int id);
DllExport void SetDebug(BOOL onOff);
DllExport void Shutdown();

LRESULT CALLBACK CbtProc(int nCode, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK CwpProc(int nCode, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK GetMsgProc(int nCode, WPARAM wParam, LPARAM lParam);
DWORD WINAPI ProcessCallbackMessages( LPVOID lpParam );

void Unhook(int id);
VOID SendCallback(int id, int msg, int param);
void InternalNextStep(int id);
void ClosePPT(int id, bool force);

#define MAX_PPTS 16
#define MAX_SLIDES 256

struct PPTVIEW
{
	HHOOK hook;
	HHOOK msgHook;
	HWND hWnd;
	HWND hWnd2;
	HWND hParentWnd;
	HANDLE hProcess;
	HANDLE hThread;
	DWORD dwProcessId;
	DWORD dwThreadId;
	RECT rect;
	int slideCount;
	int currentSlide;
	int steps;
	int firstSlideNo;
	func_type callbackFunc;
	bool locked;
	HANDLE listenerThread;
	int nextMsg;
	int nextMsgParam;
	PPTVIEWSTATE state;
};
