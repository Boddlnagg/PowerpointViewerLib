#![allow(unused_variable)]
#![allow(non_snake_case)]
#![allow(visible_private_types)]

extern crate libc;
extern crate native;

use std::c_str::CString;
//use libc::HANDLE;
use libc::types::common::c95::c_void;

type LPVOID = *mut c_void;

/*#[no_mangle]
pub extern "C" fn DllMain(hModule: HANDLE, ulReasonForCall: i32, lpReserved: LPVOID) -> bool {
    // doesn't work (is DllMain even called?)
    native::start(0, 0 as *const *const u8, main);
    true
}*/

fn entry() {
    println!("Runtime started.");
}

#[no_mangle]
pub extern "C" fn init_runtime() {
    native::start(0, 0 as *const *const u8, entry);
}

#[no_mangle]
pub extern "C" fn OpenPPT(command: CString, /*func: extern "C" fn(i32,i32) -> i32,  hParentWnd: *const i32,*/ x: i32, y: i32, width: i32, height: i32) -> i32 {
    let cmd = command.clone();
    println!("OpenPPT");
    cmd.as_str().unwrap().len() as i32
    //int OpenPPT(StringBuilder command, CallbackDelegate func, IntPtr hParentWnd, int x, int y, int width, int height);
    // STUB
    //1234
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