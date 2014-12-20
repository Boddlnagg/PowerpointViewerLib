#![allow(unused_variables)]
#![allow(non_snake_case)]

//extern crate libc;
extern crate winapi;
extern crate file_mapping;

//use std::c_str::CString;
use winapi::{DWORD,HHOOK,HINSTANCE,c_int,LPARAM,WPARAM,LRESULT,HANDLE,LPVOID,LPCVOID,BOOL,TRUE,LPCSTR,LPSECURITY_ATTRIBUTES,SIZE_T};
use std::str::from_c_str;
use std::c_str::CString;
use std::io::process::Command;
use std::io::fs::File;
use std::io::FileMode::Open;
use std::io::FileAccess::{Write,Read};
use std::io::timer::sleep;
use std::time::duration::Duration;
use std::slice::bytes::copy_memory;
use file_mapping::FileMapping;
use std::io::Reader;
//use std::mem::transmute;

static mut hInstance: Option<HINSTANCE> = None; // TODO: use mutex?

#[no_mangle]
pub extern "system" fn DllMain(hModule: HANDLE, ulReasonForCall: DWORD, lpReserved: LPVOID) -> BOOL {
    unsafe {
        hInstance = Some(hModule as HINSTANCE);
    }
    TRUE
}

type HOOKPROC = extern "system" fn(c_int, WPARAM, LPARAM) -> LRESULT;
const WH_CBT: c_int = 5;
const PAGE_READWRITE: DWORD = 0x04;

#[link(name = "user32")]
extern "system" {
    fn SetWindowsHookExW(
        idHook: c_int,
        lpfn: HOOKPROC,
        hMod: HINSTANCE,
        dwThreadId: DWORD
    ) -> HHOOK;
    
    fn UnhookWindowsHookEx(
        hhk: HHOOK
    ) -> BOOL;
    
    fn CallNextHookEx(
        hhk: HHOOK,
        nCode: c_int,
        wParam: WPARAM,
        lParam: LPARAM
    ) -> LRESULT;
}

const BUF_SIZE: DWORD = 256;
const szName: &'static str = "MyFileMappingObject";

#[no_mangle]
pub extern "C" fn OpenPPT(exe: *const i8, ppt: *const i8, /*func: extern "C" fn(i32,i32) -> i32,  hParentWnd: *const i32,*/ x: i32, y: i32, width: i32, height: i32) -> i32 {
    let exe = unsafe { from_c_str(exe) };
    let ppt = unsafe { from_c_str(ppt) };
    
    let mut file = match File::open_mode(&Path::new("log.txt"), Open, Write) {
        Ok(f) => f,
        Err(e) => return -1
    };
    let log = &mut file;
    
    let hInst: HINSTANCE = unsafe { match hInstance {
        Some(i) => i,
        None => return -2
    }};
    
    writeln!(log, "OpenPPT {} {}, hInstance: {}", exe, ppt, hInst);
    
    let mut map = match FileMapping::create(szName, BUF_SIZE) {
        Ok(m) => m,
        Err(msg) => return -3
    };
    let buffer = map.get_mut_buffer();
    let mut writer: Vec<u8> = Vec::new();
    
    /*if (globalHook != NULL)
	{
		UnhookWindowsHookEx(globalHook);
	}*/
	let globalHook = unsafe { SetWindowsHookExW(WH_CBT, CbtProc, hInst, 0) }; // 0u32 as HHOOK; // TODO: this line makes the program crash!
    writer.write_le_i32(globalHook as i32);
    copy_memory(buffer, writer.as_slice());
    writeln!(log, "globalHook: {}", globalHook);

	/*if (globalHook == 0)
	{
		DEBUG("OpenPPT: SetWindowsHookEx failed\n");
		ClosePPT(id, true);
		return -1;
	}*/
    let mut process = match Command::new(exe).args(&["/F", "/S", ppt]).spawn() {
        Ok(p) => p,
        Err(e) => return -4
    };
    //sleep(Duration::seconds(5));
    let unhooked = unsafe { UnhookWindowsHookEx(globalHook) };
    writeln!(log, "unhooked: {}", unhooked);
    
    1 // TODO
}

static mut myGlobalHook: Option<HHOOK> = None;

// This hook is started with the PPTVIEW.EXE process and waits for the
// WM_CREATEWND message. At this point (and only this point) can the
// window be resized to the correct size.
// Release the hook as soon as we're complete to free up resources
extern "system" fn CbtProc(nCode: c_int, wParam: WPARAM, lParam: LPARAM) -> LRESULT {
    /*let mut file = match File::open_mode(&Path::new("C:\\Users\\Patrick\\Desktop\\cbtlog.txt"), Open, Write) {
        Ok(f) => f,
        Err(e) => panic!("file error: {}", e),
    };
    let log = &mut file;
    writeln!(log, "In CbtProc!");*/
    let hook: HHOOK;
    unsafe {
        hook = match myGlobalHook {
            None => {
                let map = match unsafe { FileMapping::open(szName, BUF_SIZE) } {
                    Ok(m) => m,
                    Err(_) => return 0
                };
                
                let mut buffer = map.get_buffer();
                let h = match buffer.read_le_i32() {
                    Ok(h) => h as HHOOK,
                    Err(e) => return 0
                };
                myGlobalHook = Some(h);
                h
            },
            Some(h) => h
        }
    }
	/*HHOOK hook = globalHook;
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
	}*/
	unsafe{ CallNextHookEx(hook, nCode, wParam, lParam) }
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
