#![allow(unused_variables)]
#![allow(non_snake_case)]
#![feature(slicing_syntax)]

//extern crate libc;
extern crate winapi;
extern crate file_mapping;

//use std::c_str::CString;
use winapi::{DWORD,HHOOK,HINSTANCE,INT,LONG,LPARAM,WPARAM,LRESULT,HANDLE,LPVOID,BOOL,TRUE,LPCSTR,HWND,LPDWORD,RECT,HMENU,LPCWSTR};
use std::str::from_c_str;
use std::io::process::Command;
use std::io::fs::File;
use std::io::FileMode::Open;
use std::io::FileAccess::Write;
//use std::io::timer::sleep;
//use std::time::duration::Duration;
use file_mapping::StructFileMapping;
use std::{ptr, mem};
//use std::mem::transmute;

static mut hInstance: Option<HINSTANCE> = None; // TODO: use mutex?

const MAX_PPTS: uint = 16; // might lift this restriction later

#[deriving(PartialEq, Eq)]
#[allow(non_camel_case_types)]
#[allow(dead_code)]
enum PPTVIEWSTATE {
    PPT_CLOSED,
    PPT_STARTED,
    PPT_OPENED,
    PPT_LOADED,
	PPT_CLOSING
}

struct PPTVIEW {
	/*hook: HHOOK,
	msgHook: HHOOK,*/
	hWnd: HWND,
	hWnd2: HWND,
	hParentWnd: HWND,
	/*hProcess: HANDLE,
	hThread: HANDLE,
	dwProcessId: DWORD,*/
	dwThreadId: DWORD,
	rect: RECT,
	/*int slideCount;
	int currentSlide;
	int steps;
	int firstSlideNo;
	func_type callbackFunc;
	bool locked;
	listenerThread: HANDLE,
	int nextMsg;
	int nextMsgParam;*/
	state: PPTVIEWSTATE
}

struct SharedData {
    globalHook: HHOOK,
    pptView: [PPTVIEW, ..MAX_PPTS]
}

#[no_mangle]
pub extern "system" fn DllMain(hModule: HANDLE, ulReasonForCall: DWORD, lpReserved: LPVOID) -> BOOL {
    unsafe {
        hInstance = Some(hModule as HINSTANCE);
    }
    TRUE
}

type HOOKPROC = extern "system" fn(INT, WPARAM, LPARAM) -> LRESULT;
const WH_CBT: INT = 5;
const HCBT_CREATEWND: INT = 3;

#[repr(C)]
#[deriving(Copy)]
#[allow(dead_code)]
struct CREATESTRUCT {
  lpCreateParams: LPVOID,
  hInstance: HINSTANCE,
  hMenu: HMENU,
  hwndParent: HWND,
  cy: INT,
  cx: INT,
  y: INT,
  x: INT,
  style: LONG,
  lpszName: LPCWSTR, // actually LPCTSTR, but this doesn't matter here
  lpszClass: LPCWSTR, // actually LPCTSTR, but this doesn't matter here
  dwExStyle: DWORD
}

type LPCREATESTRUCT = *mut CREATESTRUCT;

#[repr(C)]
#[deriving(Copy)]
struct CBT_CREATEWND {
  lpcs: LPCREATESTRUCT,
  hwndInsertAfter: HWND
}

#[link(name = "user32")]
extern "system" {
    fn SetWindowsHookExW(
        idHook: INT,
        lpfn: HOOKPROC,
        hMod: HINSTANCE,
        dwThreadId: DWORD
    ) -> HHOOK;
    
    fn UnhookWindowsHookEx(
        hhk: HHOOK
    ) -> BOOL;
    
    fn CallNextHookEx(
        hhk: HHOOK,
        nCode: INT,
        wParam: WPARAM,
        lParam: LPARAM
    ) -> LRESULT;
    
    fn GetClassNameW(
      hWnd: HWND,
      lpClassName: LPCSTR,
      nMaxCount: INT
    ) -> INT;
    
    fn GetWindowThreadProcessId(
      hWnd: HWND,
      lpdwProcessId: LPDWORD
    ) -> DWORD;
}

const BUFFER_NAME: &'static str = "BlahMyFileMappingObject";

fn win_get_class_name(window: winapi::HWND, buffer: &mut [u16]) -> String {
    let len = unsafe { GetClassNameW(window, buffer.as_ptr() as *const i8, buffer.len() as i32) };
    String::from_utf16(buffer[..len as uint]).unwrap_or("".into_string())
}

#[no_mangle]
#[allow(unused_must_use)]
pub extern "C" fn OpenPPT(exe: *const i8, ppt: *const i8, /*func: extern "C" fn(i32,i32) -> i32,*/  hParentWnd: HWND, x: i32, y: i32, width: i32, height: i32) -> i32 {
    let exe = unsafe { from_c_str(exe) };
    let ppt = unsafe { from_c_str(ppt) };
    
    // init logging
    let mut file = match File::open_mode(&Path::new("log.txt"), Open, Write) {
        Ok(f) => f,
        Err(e) => return -1
    };
    let log = &mut file;
    
    // get current module handle (set in DllMain)
    let hInst: HINSTANCE = unsafe { match hInstance {
        Some(i) => i,
        None => return -2
    }};
    
    writeln!(log, "OpenPPT {} {}, hInstance: {}", exe, ppt, hInst);
    
    // open shared memory
    let mut map = match StructFileMapping::<SharedData>::create(BUFFER_NAME) {
        Ok(m) => m,
        Err(msg) => return -3
    };
    let shared = map.get_mut_ref();
    
    // get next free id
    let mut id: Option<uint> = None;
	for i in range(0, MAX_PPTS) {
		if shared.pptView[i].state == PPTVIEWSTATE::PPT_CLOSED {
			id = Some(i);
			break;
		}
	}
    let id: uint = match id {
        None => return -4,
        Some(i) => i
    };
    
    shared.pptView[id].hParentWnd = hParentWnd;
	shared.pptView[id].hWnd = ptr::null_mut();
	shared.pptView[id].hWnd2 = ptr::null_mut();
    
    if !hParentWnd.is_null() && x == 0 && y == 0 && width == 0 && height == 0 {
		/*LPRECT windowRect = NULL;
		GetWindowRect(hParentWnd, windowRect);
		pptView[id].rect.top = 0;
		pptView[id].rect.left = 0;
		pptView[id].rect.bottom = windowRect->bottom - windowRect->top;
		pptView[id].rect.right = windowRect->right - windowRect->left;*/
        return -99; // not implemented!
	} else {
		shared.pptView[id].rect.top = y;
		shared.pptView[id].rect.left = x;
		shared.pptView[id].rect.bottom = y + height;
		shared.pptView[id].rect.right = x + width;

		writeln!(log, "width: {}, height: {}", shared.pptView[id].rect.bottom - shared.pptView[id].rect.top, shared.pptView[id].rect.right - shared.pptView[id].rect.left);
	}
    
    /*if (globalHook != NULL)
	{
		UnhookWindowsHookEx(globalHook);
	}*/
	let globalHook = unsafe { SetWindowsHookExW(WH_CBT, CbtProc, hInst, 0) };
    if globalHook.is_null() {
        //DEBUG("OpenPPT: SetWindowsHookEx failed\n");
        //ClosePPT(id, true);
        return -5;
    }
    shared.globalHook = globalHook;
    writeln!(log, "globalHook: {}", globalHook);
    
    let process = match Command::new(exe).args(&["/F", "/S", ppt]).spawn() {
        Ok(p) => p,
        Err(e) => return -5
    };
    // when this is uncommented, the application crashes (it probably does't crash otherwise because the hook procedure is never called)!
    //sleep(Duration::milliseconds(1));
    let unhooked = unsafe { UnhookWindowsHookEx(globalHook) };
    writeln!(log, "unhooked: {}", unhooked);
    
    id as i32
}

// This hook is started with the PPTVIEW.EXE process and waits for the
// WM_CREATEWND message. At this point (and only this point) can the
// window be resized to the correct size.
// Release the hook as soon as we're complete to free up resources
extern "system" fn CbtProc(nCode: INT, wParam: WPARAM, lParam: LPARAM) -> LRESULT {
    let hook: HHOOK;
    {
        let mut map = match unsafe { StructFileMapping::<SharedData>::open(BUFFER_NAME) } {
            Ok(m) => m,
            Err(_) => return 0
        };
        let shared = map.get_mut_ref();
        hook = shared.globalHook;
        if nCode == HCBT_CREATEWND
        {
            let hCurrWnd: HWND = wParam as HWND;
            let mut buf: [u16, ..16] = [0, ..16];
            let csClassName = win_get_class_name(hCurrWnd, buf.as_mut_slice());
            if csClassName == "paneClassDC" || csClassName == "screenClass" {
                let windowThread = unsafe { GetWindowThreadProcessId(hCurrWnd, ptr::null_mut()) };
                let mut id: Option<uint> = None;
                for i in range(0, MAX_PPTS) {
                    if shared.pptView[i].dwThreadId == windowThread {
                        id = Some(i);
                        break;
                    }
                }
                
                if let Some(id) = id {
                    /*let mut file = match File::open_mode(&Path::new("C:\\Users\\Patrick\\Desktop\\cbtlog.txt"), Open, Write) {
                        Ok(f) => f,
                        Err(e) => return 0
                    };
                    let log = &mut file;
                    writeln!(log, "In CbtProc!");*/
                
                    if csClassName == "paneClassDC" {
                        shared.pptView[id].hWnd2 = hCurrWnd;
                    } else {
                        shared.pptView[id].hWnd = hCurrWnd;
                        let cw: *mut CBT_CREATEWND = lParam as *mut CBT_CREATEWND;
                        let lpcs: &mut CREATESTRUCT = unsafe { mem::transmute((*cw).lpcs) };
                        if !shared.pptView[id].hParentWnd.is_null() {
                            lpcs.hwndParent = shared.pptView[id].hParentWnd;
                        }
                        lpcs.cy = shared.pptView[id].rect.bottom - shared.pptView[id].rect.top;
                        lpcs.cx = shared.pptView[id].rect.right - shared.pptView[id].rect.left;
                        lpcs.y = -32000;
                        lpcs.x = -32000;
                    }
                    
                    if !shared.pptView[id].hWnd.is_null() && !shared.pptView[id].hWnd2.is_null() {
                        unsafe { UnhookWindowsHookEx(hook) };
                        shared.globalHook = ptr::null_mut();
                        /*pptView[id].hook = SetWindowsHookEx(WH_CALLWNDPROC,
                            CwpProc, hInstance, pptView[id].dwThreadId);
                        pptView[id].msgHook = SetWindowsHookEx(WH_GETMESSAGE,
                            GetMsgProc, hInstance, pptView[id].dwThreadId);
                        Sleep(10);
                        pptView[id].state = PPT_OPENED;
                        SendCallback(id, 1, (int)pptView[id].hWnd);
                        SendCallback(id, 2, (int)pptView[id].hWnd2);*/
                    }
                }
            }
        }
    }
	unsafe { CallNextHookEx(hook, nCode, wParam, lParam) }
}

#[no_mangle]
pub extern "C" fn ClosePPT(id: i32) {
    // STUB
}

#[no_mangle]
pub extern "C" fn SetDebug(on_off: bool) {
    // STUB
}

#[no_mangle]
pub extern "C" fn Shutdown() {
    // STUB
}

#[test]
fn it_works() {
}
